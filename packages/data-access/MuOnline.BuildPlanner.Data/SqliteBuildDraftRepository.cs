using System.Text.Json;
using Microsoft.Data.Sqlite;
using MuOnline.BuildPlanner.Application.Builds;

namespace MuOnline.BuildPlanner.Data;

public sealed class SqliteBuildDraftRepository : IBuildDraftRepository
{
    private const string UpsertSql = """
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
            $id,
            $schemaVersion,
            $rulesetId,
            $rulesetVersion,
            $datasetVersion,
            $datasetHash,
            $engineVersion,
            $payloadJson
        )
        ON CONFLICT(id) DO UPDATE SET
            schema_version = excluded.schema_version,
            ruleset_id = excluded.ruleset_id,
            ruleset_version = excluded.ruleset_version,
            dataset_version = excluded.dataset_version,
            dataset_hash = excluded.dataset_hash,
            engine_version = excluded.engine_version,
            payload_json = excluded.payload_json;
        """;

    private const string LoadSql = """
        SELECT payload_json
        FROM build_drafts
        WHERE id = $id;
        """;

    private readonly string connectionString;
    private readonly SqliteWriteContentionPolicy writePolicy;

    public SqliteBuildDraftRepository(
        string connectionString,
        SqliteWriteContentionPolicy writePolicy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(writePolicy);
        this.connectionString = connectionString;
        this.writePolicy = writePolicy;
    }

    public Task SaveAsync(
        BuildDraft draft,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(draft);
        cancellationToken.ThrowIfCancellationRequested();
        var payload = JsonSerializer.Serialize(draft);

        using var connection = OpenConnection();
        try
        {
            writePolicy.Execute(
                connection,
                (writeConnection, transaction) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using var command = writeConnection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = UpsertSql;
                    command.Parameters.AddWithValue("$id", draft.Id);
                    command.Parameters.AddWithValue("$schemaVersion", draft.SchemaVersion);
                    command.Parameters.AddWithValue("$rulesetId", draft.Ruleset.Id);
                    command.Parameters.AddWithValue("$rulesetVersion", draft.Ruleset.Version);
                    command.Parameters.AddWithValue("$datasetVersion", draft.Dataset.Version);
                    command.Parameters.AddWithValue("$datasetHash", draft.Dataset.Hash);
                    command.Parameters.AddWithValue("$engineVersion", draft.EngineVersion);
                    command.Parameters.AddWithValue("$payloadJson", payload);
                    command.ExecuteNonQuery();
                });
        }
        catch (SqliteWriteContentionException exception)
        {
            throw new BuildDraftException(
                BuildDraftErrorCodes.WriteConflict,
                $"Build draft '{draft.Id}' could not be saved because SQLite remained locked.",
                exception);
        }

        return Task.CompletedTask;
    }

    public Task<BuildDraft?> LoadAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = LoadSql;
        command.Parameters.AddWithValue("$id", id);
        var payload = command.ExecuteScalar() as string;

        if (payload is null)
        {
            return Task.FromResult<BuildDraft?>(null);
        }

        var draft = JsonSerializer.Deserialize<BuildDraft>(payload)
            ?? throw new JsonException($"Stored build draft '{id}' deserialized to null.");
        return Task.FromResult<BuildDraft?>(draft);
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }
}
