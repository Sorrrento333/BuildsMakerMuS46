using System.Runtime.InteropServices;
using System.Text.Json;

internal sealed record Inputs(long Value, long Multiplier, long Divisor);
internal sealed record Trace(string Fixture, Inputs Inputs, string Operation, string Rounding, long Result);

internal static class SyntheticCalculation
{
    internal static Trace Calculate(long value, long multiplier, long divisor)
    {
        if (divisor <= 0) throw new ArgumentOutOfRangeException(nameof(divisor));
        var product = checked(value * multiplier);
        return new("TECH-SPIKE-ONLY", new(value, multiplier, divisor),
            "floor((value * multiplier) / divisor)", "FLOOR", product / divisor);
    }
}

internal sealed partial class SqliteDatabase : IDisposable
{
    private nint handle;

    internal SqliteDatabase(string path)
    {
        Check(Native.sqlite3_open_v2(Utf8(path), out handle, 0x00000002 | 0x00000004, 0), "open");
    }

    internal Trace RoundTrip(Trace trace)
    {
        Execute("CREATE TABLE traces (id INTEGER PRIMARY KEY, payload TEXT NOT NULL)");
        var payload = JsonSerializer.Serialize(trace).Replace("'", "''", StringComparison.Ordinal);
        Execute($"INSERT INTO traces (payload) VALUES ('{payload}')");
        nint statement = 0;
        Check(Native.sqlite3_prepare_v2(handle, Utf8("SELECT payload FROM traces WHERE id = 1"), -1, out statement, 0), "prepare");
        try
        {
            if (Native.sqlite3_step(statement) != 100) throw new InvalidOperationException("SQLite returned no row");
            var json = Marshal.PtrToStringUTF8(Native.sqlite3_column_text(statement, 0))
                ?? throw new InvalidOperationException("SQLite returned null");
            return JsonSerializer.Deserialize<Trace>(json)
                ?? throw new InvalidOperationException("Could not deserialize trace");
        }
        finally { Native.sqlite3_finalize(statement); }
    }

    private void Execute(string sql)
    {
        var result = Native.sqlite3_exec(handle, Utf8(sql), 0, 0, out var error);
        if (result != 0)
        {
            var message = Marshal.PtrToStringUTF8(error) ?? "unknown SQLite error";
            Native.sqlite3_free(error);
            throw new InvalidOperationException(message);
        }
    }

    private static byte[] Utf8(string value) => System.Text.Encoding.UTF8.GetBytes(value + '\0');
    private static void Check(int result, string operation)
    {
        if (result != 0) throw new InvalidOperationException($"SQLite {operation} failed with code {result}");
    }

    public void Dispose()
    {
        if (handle != 0) Native.sqlite3_close(handle);
        handle = 0;
    }

    private static partial class Native
    {
        [LibraryImport("winsqlite3.dll")] internal static partial int sqlite3_open_v2(byte[] filename, out nint database, int flags, nint vfs);
        [LibraryImport("winsqlite3.dll")] internal static partial int sqlite3_close(nint database);
        [LibraryImport("winsqlite3.dll")] internal static partial int sqlite3_exec(nint database, byte[] sql, nint callback, nint argument, out nint error);
        [LibraryImport("winsqlite3.dll")] internal static partial void sqlite3_free(nint value);
        [LibraryImport("winsqlite3.dll")] internal static partial int sqlite3_prepare_v2(nint database, byte[] sql, int bytes, out nint statement, nint tail);
        [LibraryImport("winsqlite3.dll")] internal static partial int sqlite3_step(nint statement);
        [LibraryImport("winsqlite3.dll")] internal static partial nint sqlite3_column_text(nint statement, int column);
        [LibraryImport("winsqlite3.dll")] internal static partial int sqlite3_finalize(nint statement);
    }
}

internal static class Program
{
    private static int Main(string[] args)
    {
        return args.Contains("--test", StringComparer.Ordinal) ? RunTests() : RunDemo();
    }

    private static int RunDemo()
    {
        Console.WriteLine(JsonSerializer.Serialize(SyntheticCalculation.Calculate(10, 3, 4)));
        return 0;
    }

    private static int RunTests()
    {
        var path = Path.Combine(Path.GetTempPath(), $"mu-tech-spike-cs-{Environment.ProcessId}.sqlite");
        try
        {
            var expected = SyntheticCalculation.Calculate(10, 3, 4);
            Assert(expected.Result == 7 && expected.Rounding == "FLOOR" && expected.Fixture == "TECH-SPIKE-ONLY", "trace");
            AssertThrows<ArgumentOutOfRangeException>(() => SyntheticCalculation.Calculate(10, 3, 0), "invalid divisor");
            AssertThrows<OverflowException>(() => SyntheticCalculation.Calculate(long.MaxValue, 2, 1), "overflow");
            using var database = new SqliteDatabase(path);
            Assert(database.RoundTrip(expected) == expected, "SQLite round-trip");
            Console.WriteLine("PASS: 4 C# spike checks");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"FAIL: {exception}");
            return 1;
        }
        finally { File.Delete(path); }
    }

    private static void Assert(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException($"Assertion failed: {name}");
    }

    private static void AssertThrows<T>(Action action, string name) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}: {name}");
    }
}
