using MuOnline.SchemaValidator;

var repositoryRoot = args.Length switch
{
    0 => Directory.GetCurrentDirectory(),
    2 when args[0] == "--repository-root" => args[1],
    _ => throw new ArgumentException(
        "Usage: dotnet run --project tools/validators/MuOnline.SchemaValidator " +
        "[--repository-root <path>]")
};

var results = SchemaContractValidator.ValidateRepository(repositoryRoot);
var rulesetResults = SchemaContractValidator.ValidateRulesetRecords(repositoryRoot);
var progressionCaseResults =
    ProgressionReferenceCaseValidator.ValidateRepository(repositoryRoot);
var progressionRuleReferenceResults =
    ProgressionReferenceCaseValidator.ValidateRuleReferences(repositoryRoot);

foreach (var result in results)
{
    var status = result.MatchesExpectation ? "PASS" : "FAIL";
    Console.WriteLine(
        $"{status}: {result.ContractName} fixture '{result.FixtureKind}' " +
        $"expected valid={result.ExpectedValidity}, actual valid={result.ActualValidity}");
}

foreach (var result in rulesetResults)
{
    var status = result.ActualValidity ? "PASS" : "FAIL";
    Console.WriteLine(
        $"{status}: {result.ContractName} ruleset record '{result.RecordId}' " +
        $"actual valid={result.ActualValidity}");
}

foreach (var result in progressionCaseResults)
{
    var status = result.MatchesExpectation ? "PASS" : "FAIL";
    Console.WriteLine(
        $"{status}: progression reference case '{result.CaseId}' " +
        $"expected valid={result.ExpectedValidity}, actual valid={result.ActualValidity}, " +
        $"expected points={result.ExpectedEarnedPoints}, actual points={result.ActualEarnedPoints}, " +
        $"error={result.ActualErrorCode ?? "none"}");
}

foreach (var result in progressionRuleReferenceResults)
{
    var status = result.IsValid ? "PASS" : "FAIL";
    Console.WriteLine(
        $"{status}: progression rule '{result.RuleId}' status={result.Status}, " +
        $"resolved test cases={result.TestCaseRefs.Count}, " +
        $"errors={string.Join(" | ", result.Errors.DefaultIfEmpty("none"))}");
}

return results.All(result => result.MatchesExpectation) &&
       rulesetResults.All(result => result.ActualValidity) &&
       progressionCaseResults.All(result => result.MatchesExpectation) &&
       progressionRuleReferenceResults.All(result => result.IsValid)
    ? 0
    : 1;
