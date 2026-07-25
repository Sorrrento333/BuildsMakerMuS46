using System.Text.Json;
using Microsoft.Data.Sqlite;
using MuOnline.BuildPlanner.Application.Builds;
using Xunit;

namespace MuOnline.BuildPlanner.Data.IntegrationTests;

public sealed class SqliteBuildDraftRepositoryTests
{
    [Fact]
    public async Task SaveAndLoadPreserveExactPayloadAndMetadata()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();
        var repository = database.CreateRepository();
        var draft = CreateDraft("draft-synthetic");
        var expectedPayload = JsonSerializer.Serialize(draft);

        await repository.SaveAsync(draft, TestContext.Current.CancellationToken);
        var loaded = await repository.LoadAsync(
            draft.Id,
            TestContext.Current.CancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(expectedPayload, JsonSerializer.Serialize(loaded));
        using var connection = database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                schema_version,
                ruleset_id,
                ruleset_version,
                dataset_version,
                dataset_hash,
                engine_version,
                payload_json
            FROM build_drafts
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", draft.Id);
        using var reader = command.ExecuteReader();
        Assert.True(reader.Read());
        Assert.Equal(draft.SchemaVersion, reader.GetString(0));
        Assert.Equal(draft.Ruleset.Id, reader.GetString(1));
        Assert.Equal(draft.Ruleset.Version, reader.GetString(2));
        Assert.Equal(draft.Dataset.Version, reader.GetString(3));
        Assert.Equal(draft.Dataset.Hash, reader.GetString(4));
        Assert.Equal(draft.EngineVersion, reader.GetString(5));
        Assert.Equal(expectedPayload, reader.GetString(6));
    }

    [Fact]
    public async Task SaveAtomicallyReplacesExistingRowById()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();
        var repository = database.CreateRepository();
        var original = CreateDraft("draft-synthetic");
        var replacement = original with
        {
            Dataset = new BuildDraftDatasetReference(
                "synthetic-002",
                $"sha256:{new string('1', 64)}"),
            EngineVersion = "0.2.0",
            StatDistribution = original.StatDistribution with
            {
                Allocations = new Dictionary<string, long>(StringComparer.Ordinal)
                {
                    ["stat-alpha"] = 5,
                    ["stat-beta"] = 5,
                },
                SpentPoints = 10,
                RemainingPoints = 0,
            },
        };

        await repository.SaveAsync(original, TestContext.Current.CancellationToken);
        await repository.SaveAsync(replacement, TestContext.Current.CancellationToken);
        var loaded = await repository.LoadAsync(
            replacement.Id,
            TestContext.Current.CancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(JsonSerializer.Serialize(replacement), JsonSerializer.Serialize(loaded));
        using var connection = database.OpenConnection();
        Assert.Equal(1L, ExecuteScalar<long>(
            connection,
            "SELECT COUNT(*) FROM build_drafts;"));
        Assert.Equal(replacement.Dataset.Hash, ExecuteScalar<string>(
            connection,
            "SELECT dataset_hash FROM build_drafts;"));
        Assert.Equal(replacement.EngineVersion, ExecuteScalar<string>(
            connection,
            "SELECT engine_version FROM build_drafts;"));
    }

    [Fact]
    public async Task FailedReplacementRollsBackMetadataAndPayload()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();
        var repository = database.CreateRepository();
        var original = CreateDraft("draft-synthetic");
        var originalPayload = JsonSerializer.Serialize(original);
        await repository.SaveAsync(original, TestContext.Current.CancellationToken);
        using (var connection = database.OpenConnection())
        {
            ExecuteNonQuery(
                connection,
                """
                CREATE TRIGGER reject_synthetic_replacement
                BEFORE UPDATE ON build_drafts
                WHEN NEW.engine_version = 'reject-write'
                BEGIN
                    SELECT RAISE(ABORT, 'synthetic write failure');
                END;
                """);
        }

        var rejected = original with
        {
            EngineVersion = "reject-write",
            Dataset = new BuildDraftDatasetReference(
                "synthetic-rejected",
                $"sha256:{new string('f', 64)}"),
        };

        await Assert.ThrowsAsync<SqliteException>(
            () => repository.SaveAsync(
                rejected,
                TestContext.Current.CancellationToken));

        using var verificationConnection = database.OpenConnection();
        Assert.Equal(original.EngineVersion, ExecuteScalar<string>(
            verificationConnection,
            "SELECT engine_version FROM build_drafts;"));
        Assert.Equal(original.Dataset.Version, ExecuteScalar<string>(
            verificationConnection,
            "SELECT dataset_version FROM build_drafts;"));
        Assert.Equal(originalPayload, ExecuteScalar<string>(
            verificationConnection,
            "SELECT payload_json FROM build_drafts;"));
    }

    [Fact]
    public async Task ReopenedRepositoryLoadsPersistedDraft()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();
        var draft = CreateDraft("draft-reopened");
        await database.CreateRepository().SaveAsync(
            draft,
            TestContext.Current.CancellationToken);

        var reopenedRepository = database.CreateRepository();
        var loaded = await reopenedRepository.LoadAsync(
            draft.Id,
            TestContext.Current.CancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(JsonSerializer.Serialize(draft), JsonSerializer.Serialize(loaded));
    }

    [Fact]
    public async Task LoadMissingDraftPerformsNoDatabaseMutation()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();
        var repository = database.CreateRepository();
        using var beforeConnection = database.OpenConnection();
        var changesBefore = ExecuteScalar<long>(
            beforeConnection,
            "SELECT total_changes();");

        var loaded = await repository.LoadAsync(
            "draft-missing",
            TestContext.Current.CancellationToken);

        Assert.Null(loaded);
        using var afterConnection = database.OpenConnection();
        Assert.Equal(0L, ExecuteScalar<long>(
            afterConnection,
            "SELECT COUNT(*) FROM build_drafts;"));
        Assert.Equal(changesBefore, ExecuteScalar<long>(
            beforeConnection,
            "SELECT total_changes();"));
    }

    [Fact]
    public async Task ExhaustedWriteContentionUsesStableApplicationCode()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();
        using var blocker = database.OpenConnection();
        using var blockingTransaction = blocker.BeginTransaction(deferred: false);
        using (var command = blocker.CreateCommand())
        {
            command.Transaction = blockingTransaction;
            command.CommandText = """
                INSERT INTO build_drafts (
                    id,
                    schema_version,
                    ruleset_id,
                    ruleset_version,
                    dataset_version,
                    dataset_hash,
                    engine_version,
                    payload_json
                )
                VALUES (
                    'blocking-draft',
                    '1.0.0',
                    'ruleset-synthetic',
                    '1.0.0',
                    'synthetic-001',
                    'sha256:synthetic',
                    '0.1.0',
                    '{}'
                );
                """;
            command.ExecuteNonQuery();
        }

        var exception = await Assert.ThrowsAsync<BuildDraftException>(
            () => database.CreateRepository().SaveAsync(
                CreateDraft("draft-conflicted"),
                TestContext.Current.CancellationToken));

        Assert.Equal(BuildDraftErrorCodes.WriteConflict, exception.Code);
        Assert.IsType<SqliteWriteContentionException>(exception.InnerException);
    }

    private static BuildDraft CreateDraft(string id) =>
        new(
            BuildDraft.CurrentSchemaVersion,
            id,
            new BuildDraftVersionedReference("ruleset-synthetic", "1.0.0"),
            new BuildDraftDatasetReference(
                "synthetic-001",
                $"sha256:{new string('0', 64)}"),
            "0.1.0",
            new BuildDraftProgressionInputs(
                "class-synthetic",
                "evolution-synthetic",
                3,
                []),
            new BuildDraftResetInputs(2, 100),
            new BuildDraftStatDistribution(
                BuildDraftStatDistribution.CurrentSchemaVersion,
                "ruleset-synthetic",
                "class-synthetic",
                new BuildDraftVersionedReference("progression-synthetic", "1.0.0"),
                10,
                new BuildDraftResetInputs(2, 100),
                200,
                210,
                new Dictionary<string, long>(StringComparer.Ordinal)
                {
                    ["stat-alpha"] = 4,
                    ["stat-beta"] = 3,
                },
                7,
                203));

    private static T ExecuteScalar<T>(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return (T)command.ExecuteScalar()!;
    }

    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private sealed class TemporarySqliteDatabase : IDisposable
    {
        private readonly string path = Path.Combine(
            Path.GetTempPath(),
            $"mu-build-planner-draft-tests-{Guid.NewGuid():N}.sqlite");

        private string ConnectionString => new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
        }.ToString();

        public void ApplyMigrations()
        {
            using var connection = OpenConnection();
            new SqliteMigrationRunner().Apply(
                connection,
                SqliteBuildDraftMigrations.All);
        }

        public SqliteBuildDraftRepository CreateRepository() =>
            new(
                ConnectionString,
                new SqliteWriteContentionPolicy(
                    new SqliteWriteContentionOptions(
                        commandTimeoutSeconds: 1,
                        maximumRetryCount: 0,
                        retryDelay: TimeSpan.Zero)));

        public SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public void Dispose()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
