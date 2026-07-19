using System.IO;
using System.Text.Json;
using System.Windows;

namespace MuOnline.BuildPlanner.App;

public partial class App : Application
{
    private static readonly JsonSerializerOptions ReportSerializerOptions = new()
    {
        WriteIndented = true,
    };

    protected override void OnStartup(StartupEventArgs e)
    {
        if (PublicationSmokeOptions.IsRequested(e.Args))
        {
            RunPublicationSmoke(e.Args);
            return;
        }

        base.OnStartup(e);
        new MainWindow().Show();
    }

    private void RunPublicationSmoke(string[] arguments)
    {
        var exitCode = 1;
        string? reportPath = null;

        try
        {
            var options = PublicationSmokeOptions.Parse(arguments);
            reportPath = options.ReportPath;
            var report = PublicationSmokeRunner.Run(options);
            WriteReport(reportPath, report);
            exitCode = 0;
        }
        catch (Exception exception)
        {
            if (!string.IsNullOrWhiteSpace(reportPath))
            {
                WriteReport(reportPath, PublicationSmokeReport.Failed(exception));
            }
        }

        Shutdown(exitCode);
    }

    private static void WriteReport(string reportPath, PublicationSmokeReport report)
    {
        var directory = Path.GetDirectoryName(reportPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(report, ReportSerializerOptions);
        File.WriteAllText(reportPath, json);
    }
}
