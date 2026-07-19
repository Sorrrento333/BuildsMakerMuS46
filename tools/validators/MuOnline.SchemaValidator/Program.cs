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

foreach (var result in results)
{
    var status = result.MatchesExpectation ? "PASS" : "FAIL";
    Console.WriteLine(
        $"{status}: {result.ContractName} fixture '{result.FixtureKind}' " +
        $"expected valid={result.ExpectedValidity}, actual valid={result.ActualValidity}");
}

return results.All(result => result.MatchesExpectation) ? 0 : 1;
