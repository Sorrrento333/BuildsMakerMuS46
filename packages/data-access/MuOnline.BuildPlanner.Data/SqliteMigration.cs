using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MuOnline.BuildPlanner.Data;

public sealed class SqliteMigration
{
    public SqliteMigration(long version, string name, string sql)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        Version = version;
        Name = name;
        Sql = sql;
        Checksum = ComputeChecksum(version, name, sql);
    }

    public long Version { get; }

    public string Name { get; }

    public string Sql { get; }

    public string Checksum { get; }

    private static string ComputeChecksum(long version, string name, string sql)
    {
        var canonicalContent = string.Concat(
            version.ToString(CultureInfo.InvariantCulture),
            "\n",
            name,
            "\n",
            sql);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalContent));

        return Convert.ToHexStringLower(hash);
    }
}
