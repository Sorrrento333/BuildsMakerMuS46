using System.Text.Json;
using Microsoft.Data.Sqlite;
using MuOnline.BuildPlanner.Application.Builds;
using Xunit;

namespace MuOnline.BuildPlanner.Data.IntegrationTests;

public sealed class SqliteBuildRepositoryTests
{
    [Fact]
    public async Task SaveAndLoadPreserveExactPayloadAndMetadata()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();
        var repository = database.CreateRepository();
        var build = CreateBuild("build-synthetic");
        var expectedPayload = JsonSerializer.Serialize(build);

        await repository.SaveAsync(build, TestContext.Current.CancellationToken);
        var loaded = await repository.LoadAsync(
            build.Id,
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
                character_class_id,
                evolution_id,
                payload_json
            FROM builds
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", build.Id);
        using var reader = command.ExecuteReader();
        Assert.True(reader.Read());
        Assert.Equal(build.SchemaVersion, reader.GetString(0));
        Assert.Equal(build.Ruleset.Id, reader.GetString(1));
        Assert.Equal(build.Ruleset.Version, reader.GetString(2));
        Assert.Equal(build.Dataset.Version, reader.GetString(3));
        Assert.Equal(build.Dataset.Hash, reader.GetString(4));
        Assert.Equal(build.EngineVersion, reader.GetString(5));
        Assert.Equal(build.CharacterClassId, reader.GetString(6));
        Assert.Equal(build.EvolutionId, reader.GetString(7));
        Assert.Equal(expectedPayload, reader.GetString(8));
    }

    [Fact]
    public async Task SaveAtomicallyReplacesExistingRowById()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();
        var repository = database.CreateRepository();
        var original = CreateBuild("build-synthetic");
        var replacement = original with
        {
            Dataset = new BuildDraftDatasetReference(
                "synthetic-002",
                $"sha256:{new string('1', 64)}"),
            EngineVersion = "0.2.0",
            Stats = new Dictionary<string, long>(StringComparer.Ordinal)
            {
                ["stat-alpha"] = 15,
                ["stat-beta"] = 9,
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
            "SELECT COUNT(*) FROM builds;"));
        Assert.Equal(replacement.Dataset.Hash, ExecuteScalar<string>(
            connection,
            "SELECT dataset_hash FROM builds;"));
        Assert.Equal(replacement.EngineVersion, ExecuteScalar<string>(
            connection,
            "SELECT engine_version FROM builds;"));
    }

    [Fact]
    public async Task FailedReplacementRollsBackMetadataAndPayload()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();
        var repository = database.CreateRepository();
        var original = CreateBuild("build-synthetic");
        var originalPayload = JsonSerializer.Serialize(original);
        await repository.SaveAsync(original, TestContext.Current.CancellationToken);
        using (var connection = database.OpenConnection())
        {
            ExecuteNonQuery(
                connection,
                """
                CREATE TRIGGER reject_synthetic_replacement
                BEFORE UPDATE ON builds
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
            "SELECT engine_version FROM builds;"));
        Assert.Equal(original.Dataset.Version, ExecuteScalar<string>(
            verificationConnection,
            "SELECT dataset_version FROM builds;"));
        Assert.Equal(originalPayload, ExecuteScalar<string>(
            verificationConnection,
            "SELECT payload_json FROM builds;"));
    }

    [Fact]
    public async Task ReopenedRepositoryLoadsPersistedBuild()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();
        var build = CreateBuild("build-reopened");
        await database.CreateRepository().SaveAsync(
            build,
            TestContext.Current.CancellationToken);

        var reopenedRepository = database.CreateRepository();
        var loaded = await reopenedRepository.LoadAsync(
            build.Id,
            TestContext.Current.CancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(JsonSerializer.Serialize(build), JsonSerializer.Serialize(loaded));
    }

    [Fact]
    public async Task ListReturnsEveryPersistedBuildOrderedById()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();
        var repository = database.CreateRepository();
        var second = CreateBuild("build-second");
        var first = CreateBuild("build-first");
        await repository.SaveAsync(second, TestContext.Current.CancellationToken);
        await repository.SaveAsync(first, TestContext.Current.CancellationToken);

        var listed = await repository.ListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["build-first", "build-second"], listed.Select(item => item.Id));
        var summary = listed[0];
        Assert.Equal(CharacterBuild.CurrentSchemaVersion, summary.SchemaVersion);
        Assert.Equal(first.CharacterClassId, summary.CharacterClassId);
        Assert.Equal(first.EvolutionId, summary.EvolutionId);
        Assert.Equal(first.Level, summary.Level);
        Assert.Equal(first.ResetCount, summary.ResetCount);
        Assert.Equal(first.PointsPerReset, summary.PointsPerReset);
        Assert.Equal(first.Dataset.Version, summary.DatasetVersion);
    }

    [Fact]
    public async Task ListOnEmptyDatabaseReturnsNoBuilds()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();

        var listed = await database.CreateRepository()
            .ListAsync(TestContext.Current.CancellationToken);

        Assert.Empty(listed);
    }

    [Fact]
    public async Task ListPerformsNoDatabaseMutation()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();
        var repository = database.CreateRepository();
        await repository.SaveAsync(
            CreateBuild("build-synthetic"),
            TestContext.Current.CancellationToken);
        using var beforeConnection = database.OpenConnection();
        var changesBefore = ExecuteScalar<long>(
            beforeConnection,
            "SELECT total_changes();");
        var countBefore = ExecuteScalar<long>(
            beforeConnection,
            "SELECT COUNT(*) FROM builds;");

        var listed = await repository.ListAsync(TestContext.Current.CancellationToken);

        Assert.Single(listed);
        using var afterConnection = database.OpenConnection();
        Assert.Equal(countBefore, ExecuteScalar<long>(
            afterConnection,
            "SELECT COUNT(*) FROM builds;"));
        Assert.Equal(changesBefore, ExecuteScalar<long>(
            afterConnection,
            "SELECT total_changes();"));
    }

    [Fact]
    public async Task LoadMissingBuildPerformsNoDatabaseMutation()
    {
        using var database = new TemporarySqliteDatabase();
        database.ApplyMigrations();
        var repository = database.CreateRepository();
        using var beforeConnection = database.OpenConnection();
        var changesBefore = ExecuteScalar<long>(
            beforeConnection,
            "SELECT total_changes();");

        var loaded = await repository.LoadAsync(
            "build-missing",
            TestContext.Current.CancellationToken);

        Assert.Null(loaded);
        using var afterConnection = database.OpenConnection();
        Assert.Equal(0L, ExecuteScalar<long>(
            afterConnection,
            "SELECT COUNT(*) FROM builds;"));
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
                INSERT INTO builds (
                    id,
                    schema_version,
                    ruleset_id,
                    ruleset_version,
                    dataset_version,
                    dataset_hash,
                    engine_version,
                    character_class_id,
                    evolution_id,
                    payload_json
                )
                VALUES (
                    'blocking-build',
                    '1.0.0',
                    'ruleset-synthetic',
                    '1.0.0',
                    'synthetic-001',
                    'sha256:synthetic',
                    '0.1.0',
                    'class-synthetic',
                    'evolution-synthetic',
                    '{}'
                );
                """;
            command.ExecuteNonQuery();
        }

        var exception = await Assert.ThrowsAsync<BuildException>(
            () => database.CreateRepository().SaveAsync(
                CreateBuild("build-conflicted"),
                TestContext.Current.CancellationToken));

        Assert.Equal(BuildErrorCodes.WriteConflict, exception.Code);
        Assert.IsType<SqliteWriteContentionException>(exception.InnerException);
    }

    private static CharacterBuild CreateBuild(string id) =>
        new(
            CharacterBuild.CurrentSchemaVersion,
            id,
            new BuildDraftVersionedReference("ruleset-synthetic", "1.0.0"),
            new BuildDraftDatasetReference(
                "synthetic-001",
                $"sha256:{new string('0', 64)}"),
            "0.1.0",
            "class-synthetic",
            "evolution-synthetic",
            3,
            new Dictionary<string, long>(StringComparer.Ordinal)
            {
                ["stat-alpha"] = 14,
                ["stat-beta"] = 9,
            },
            ["quest-synthetic"],
            2,
            100,
            []);

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
            $"mu-build-planner-build-tests-{Guid.NewGuid():N}.sqlite");

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

        public SqliteBuildRepository CreateRepository() =>
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