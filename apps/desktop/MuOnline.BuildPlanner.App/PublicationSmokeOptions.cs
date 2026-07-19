using System.IO;

namespace MuOnline.BuildPlanner.App;

internal sealed record PublicationSmokeOptions(
    PublicationSmokePhase Phase,
    string DataDirectory,
    string ReportPath)
{
    private const string RequestArgument = "--publication-smoke";

    public static bool IsRequested(IEnumerable<string> arguments) =>
        arguments.Contains(RequestArgument, StringComparer.Ordinal);

    public static PublicationSmokeOptions Parse(IReadOnlyList<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        if (!IsRequested(arguments))
        {
            throw new ArgumentException("The publication smoke flag is required.", nameof(arguments));
        }

        var phaseValue = ReadRequiredValue(arguments, "--phase");
        var dataDirectory = Path.GetFullPath(ReadRequiredValue(arguments, "--data-directory"));
        var reportPath = Path.GetFullPath(ReadRequiredValue(arguments, "--report-path"));
        var phase = phaseValue switch
        {
            "initialize" => PublicationSmokePhase.Initialize,
            "verify-update" => PublicationSmokePhase.VerifyUpdate,
            _ => throw new ArgumentException(
                $"Unsupported publication smoke phase '{phaseValue}'.",
                nameof(arguments)),
        };

        return new PublicationSmokeOptions(phase, dataDirectory, reportPath);
    }

    private static string ReadRequiredValue(IReadOnlyList<string> arguments, string name)
    {
        for (var index = 0; index < arguments.Count - 1; index++)
        {
            if (string.Equals(arguments[index], name, StringComparison.Ordinal))
            {
                var value = arguments[index + 1];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        throw new ArgumentException($"Argument '{name}' requires a value.", nameof(arguments));
    }
}

internal enum PublicationSmokePhase
{
    Initialize,
    VerifyUpdate,
}
