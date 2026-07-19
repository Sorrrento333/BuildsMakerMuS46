using System.Data;
using Microsoft.Data.Sqlite;

namespace MuOnline.BuildPlanner.Data;

public static class SqliteBackupService
{
    private const int MaximumReportedIntegrityErrors = 8;

    public static void CreateVerifiedBackup(SqliteConnection sourceConnection, string backupPath)
    {
        EnsureOpenConnection(sourceConnection, nameof(sourceConnection));
        var normalizedBackupPath = NormalizeDatabasePath(backupPath, nameof(backupPath));
        EnsureDifferentDatabase(sourceConnection, normalizedBackupPath, nameof(backupPath));

        var backupDirectory = Path.GetDirectoryName(normalizedBackupPath)!;
        if (!Directory.Exists(backupDirectory))
        {
            throw new DirectoryNotFoundException(
                $"The backup directory '{backupDirectory}' does not exist.");
        }

        var candidatePath = Path.Combine(
            backupDirectory,
            $".{Path.GetFileName(normalizedBackupPath)}.{Guid.NewGuid():N}.candidate");

        try
        {
            using (var candidateConnection = OpenConnection(candidatePath, SqliteOpenMode.ReadWriteCreate))
            {
                sourceConnection.BackupDatabase(candidateConnection);
                EnsureIntegrity(candidateConnection, "The backup candidate failed SQLite integrity verification.");
            }

            File.Move(candidatePath, normalizedBackupPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(candidatePath))
            {
                File.Delete(candidatePath);
            }
        }
    }

    public static void RestoreVerifiedBackup(string backupPath, SqliteConnection destinationConnection)
    {
        EnsureOpenConnection(destinationConnection, nameof(destinationConnection));
        var normalizedBackupPath = NormalizeDatabasePath(backupPath, nameof(backupPath));
        EnsureDifferentDatabase(destinationConnection, normalizedBackupPath, nameof(backupPath));

        if (!File.Exists(normalizedBackupPath))
        {
            throw new FileNotFoundException("The SQLite backup does not exist.", normalizedBackupPath);
        }

        using var backupConnection = OpenConnection(normalizedBackupPath, SqliteOpenMode.ReadOnly);
        EnsureIntegrity(backupConnection, "The SQLite backup cannot be restored because it failed integrity verification.");
        backupConnection.BackupDatabase(destinationConnection);
        EnsureIntegrity(destinationConnection, "The restored SQLite database failed integrity verification.");
    }

    private static SqliteConnection OpenConnection(string path, SqliteOpenMode mode)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = mode,
            Pooling = false,
        }.ToString();
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }

    private static void EnsureIntegrity(SqliteConnection connection, string failureMessage)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA integrity_check;";
            using var reader = command.ExecuteReader();
            var results = new List<string>();
            var isValid = false;

            while (reader.Read())
            {
                var result = reader.GetString(0);
                if (results.Count < MaximumReportedIntegrityErrors)
                {
                    results.Add(result);
                }

                isValid = results.Count == 1 &&
                    string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
            }

            if (!isValid)
            {
                var details = results.Count == 0
                    ? "SQLite returned no integrity result."
                    : string.Join(" | ", results);
                throw new SqliteBackupIntegrityException($"{failureMessage} {details}");
            }
        }
        catch (SqliteException exception)
        {
            throw new SqliteBackupIntegrityException(failureMessage, exception);
        }
    }

    private static void EnsureOpenConnection(SqliteConnection connection, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(connection, parameterName);
        if (connection.State != ConnectionState.Open)
        {
            throw new InvalidOperationException("The SQLite connection must be open.");
        }
    }

    private static string NormalizeDatabasePath(string path, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path, parameterName);
        return Path.GetFullPath(path);
    }

    private static void EnsureDifferentDatabase(
        SqliteConnection connection,
        string otherPath,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(connection.DataSource) ||
            string.Equals(connection.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var connectionPath = Path.GetFullPath(connection.DataSource);
        if (string.Equals(connectionPath, otherPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The source and destination SQLite databases must use different paths.",
                parameterName);
        }
    }
}
