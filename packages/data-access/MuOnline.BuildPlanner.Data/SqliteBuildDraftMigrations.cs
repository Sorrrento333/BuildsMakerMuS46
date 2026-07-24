namespace MuOnline.BuildPlanner.Data;

public static class SqliteBuildDraftMigrations
{
    public static IReadOnlyList<SqliteMigration> All { get; } =
        Array.AsReadOnly(
        [
            new SqliteMigration(
                1,
                "create_build_drafts",
                """
                CREATE TABLE build_drafts (
                    id TEXT NOT NULL PRIMARY KEY,
                    schema_version TEXT NOT NULL,
                    ruleset_id TEXT NOT NULL,
                    ruleset_version TEXT NOT NULL,
                    dataset_version TEXT NOT NULL,
                    dataset_hash TEXT NOT NULL,
                    engine_version TEXT NOT NULL,
                    payload_json TEXT NOT NULL
                );
                """),
        ]);
}
