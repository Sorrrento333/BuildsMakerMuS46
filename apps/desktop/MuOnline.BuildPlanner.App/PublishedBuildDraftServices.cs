using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using MuOnline.BuildPlanner.Application.Builds;
using MuOnline.BuildPlanner.Data;

namespace MuOnline.BuildPlanner.App;

internal sealed record PublishedBuildDraftServices(
    SaveBuildDraftUseCase SaveUseCase,
    LoadBuildDraftUseCase LoadUseCase,
    BuildDraftRuntimeContext RuntimeContext,
    string DatabasePath,
    MigrationApplicationResult MigrationResult)
{
    private const string DatabaseFileName = "build-planner.sqlite";
    private const string RulesetVersion = "1.0.0";
    private const string DatasetVersion = "2026-07-28.2";
    private const string EngineVersion = "0.2.0";

    public static PublishedBuildDraftServices CreateDefault()
    {
        var localApplicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localApplicationData))
        {
            throw new InvalidOperationException(
                "Windows did not provide a local application data directory.");
        }

        return Create(Path.Combine(localApplicationData, "MuOnline.BuildPlanner"));
    }

    public static PublishedBuildDraftServices Create(
        string dataDirectory,
        IReadOnlyCollection<SqliteMigration>? additionalMigrations = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);
        var normalizedDataDirectory = Path.GetFullPath(dataDirectory);
        EnsureSeparateFromBinaryDirectory(normalizedDataDirectory);
        Directory.CreateDirectory(normalizedDataDirectory);
        var databasePath = Path.Combine(normalizedDataDirectory, DatabaseFileName);
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
        }.ToString();

        IReadOnlyList<SqliteMigration> migrations = additionalMigrations is null
            ? SqliteBuildDraftMigrations.All
            : [.. SqliteBuildDraftMigrations.All, .. additionalMigrations];
        MigrationApplicationResult migrationResult;
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            migrationResult = new SqliteMigrationRunner().Apply(
                connection,
                migrations);
        }

        var writePolicy = new SqliteWriteContentionPolicy(
            new SqliteWriteContentionOptions(
                commandTimeoutSeconds: 2,
                maximumRetryCount: 2,
                retryDelay: TimeSpan.FromMilliseconds(150)));
        var repository = new SqliteBuildDraftRepository(connectionString, writePolicy);
        var context = new BuildDraftRuntimeContext(
            PublishedProgressionRuleset.Catalog,
            new BuildDraftVersionedReference(
                PublishedProgressionRuleset.Catalog.RulesetId,
                RulesetVersion),
            new BuildDraftDatasetReference(
                DatasetVersion,
                ComputeDatasetHash(PublishedProgressionRuleset.SnapshotRoot)),
            EngineVersion);

        return new PublishedBuildDraftServices(
            new SaveBuildDraftUseCase(repository, context),
            new LoadBuildDraftUseCase(repository, context),
            context,
            databasePath,
            migrationResult);
    }

    private static string ComputeDatasetHash(string snapshotRoot)
    {
        var files = Directory.GetFiles(
                snapshotRoot,
                "*.json",
                SearchOption.AllDirectories)
            .Select(path => new
            {
                FullPath = path,
                RelativePath = Path.GetRelativePath(snapshotRoot, path)
                    .Replace(Path.DirectorySeparatorChar, '/'),
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToArray();
        if (files.Length == 0)
        {
            throw new InvalidDataException(
                $"The published snapshot '{snapshotRoot}' contains no JSON files.");
        }

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var file in files)
        {
            hash.AppendData(Encoding.UTF8.GetBytes(file.RelativePath));
            hash.AppendData([0]);
            hash.AppendData(File.ReadAllBytes(file.FullPath));
            hash.AppendData([0]);
        }

        return $"sha256:{Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant()}";
    }

    private static void EnsureSeparateFromBinaryDirectory(string dataDirectory)
    {
        var normalizedDataDirectory = EnsureTrailingSeparator(dataDirectory);
        var normalizedBinaryDirectory = EnsureTrailingSeparator(
            Path.GetFullPath(AppContext.BaseDirectory));
        if (normalizedDataDirectory.StartsWith(
                normalizedBinaryDirectory,
                StringComparison.OrdinalIgnoreCase) ||
            normalizedBinaryDirectory.StartsWith(
                normalizedDataDirectory,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The user data directory must be separate from the binary directory.");
        }
    }

    private static string EnsureTrailingSeparator(string path) =>
        Path.EndsInDirectorySeparator(path)
            ? path
            : string.Concat(path, Path.DirectorySeparatorChar);
}
