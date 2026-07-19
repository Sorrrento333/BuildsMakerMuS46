using System.Text.Json;
using Json.Schema;
using MuOnline.SchemaValidator;

if (args.Length != 1)
{
    throw new ArgumentException("Usage: contract harness <repository-root>");
}

var repositoryRoot = Path.GetFullPath(args[0]);

for (var run = 1; run <= 2; run++)
{
    var results = SchemaContractValidator.ValidateRepository(repositoryRoot);
    if (results.Count != 10 || results.Any(result => !result.MatchesExpectation))
    {
        throw new InvalidOperationException($"Contract run {run} failed.");
    }

    Console.WriteLine($"PASS: source-built contract run {run}: 10/10 fixtures.");
}

var evidenceSchemaPath = Path.Combine(
    repositoryRoot,
    "packages",
    "schemas",
    "v1",
    "evidence.schema.json");
var schema = JsonSchema.FromFile(evidenceSchemaPath);
using var invalidFormatInstance = JsonDocument.Parse("""
    {
      "schemaVersion": "1.0.0",
      "id": "evidence-format-probe",
      "source": {
        "url": "not a uri",
        "title": "Synthetic source",
        "publisher": "Synthetic publisher",
        "usageTerms": "Synthetic test data"
      },
      "consultedOn": "not-a-date",
      "scope": {
        "gameVersion": "synthetic",
        "versionConfidence": "UNKNOWN",
        "serverType": "UNKNOWN"
      },
      "extractedText": "Synthetic text.",
      "interpretation": "Exercises format assertion only.",
      "confidence": "UNVERIFIED",
      "reviewStatus": "PENDING"
    }
    """);
var formatResult = schema.Evaluate(
    invalidFormatInstance.RootElement,
    new EvaluationOptions
    {
        OutputFormat = OutputFormat.List,
        RequireFormatValidation = true,
    });

if (formatResult.IsValid)
{
    throw new InvalidOperationException("The explicit uri/date format probe was accepted.");
}

Console.WriteLine("PASS: source-built validator rejected the explicit uri/date format probe.");
