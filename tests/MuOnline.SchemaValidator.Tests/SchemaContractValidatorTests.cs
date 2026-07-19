using MuOnline.SchemaValidator;
using Xunit;

namespace MuOnline.SchemaValidator.Tests;

public sealed class SchemaContractValidatorTests
{
    [Fact]
    public void AllVersionOneFixturesMatchTheirExpectedValidity()
    {
        var results = SchemaContractValidator.ValidateRepository(FindRepositoryRoot());

        Assert.Equal(10, results.Count);
        Assert.Collection(
            results,
            result => AssertResult(result, "evidence", "valid", expectedValidity: true),
            result => AssertResult(result, "evidence", "invalid", expectedValidity: false),
            result => AssertResult(result, "formula", "valid", expectedValidity: true),
            result => AssertResult(result, "formula", "invalid", expectedValidity: false),
            result => AssertResult(result, "character-class", "valid", expectedValidity: true),
            result => AssertResult(result, "character-class", "invalid", expectedValidity: false),
            result => AssertResult(result, "server-profile", "valid", expectedValidity: true),
            result => AssertResult(result, "server-profile", "invalid", expectedValidity: false),
            result => AssertResult(result, "build", "valid", expectedValidity: true),
            result => AssertResult(result, "build", "invalid", expectedValidity: false));
    }

    [Fact]
    public void RepositoryValidationCanRunMoreThanOnceInTheSameProcess()
    {
        var repositoryRoot = FindRepositoryRoot();

        var firstResults = SchemaContractValidator.ValidateRepository(repositoryRoot);
        var secondResults = SchemaContractValidator.ValidateRepository(repositoryRoot);

        Assert.All(firstResults, result => Assert.True(result.MatchesExpectation));
        Assert.All(secondResults, result => Assert.True(result.MatchesExpectation));
    }

    private static void AssertResult(
        FixtureValidationResult result,
        string expectedContract,
        string expectedKind,
        bool expectedValidity)
    {
        Assert.Equal(expectedContract, result.ContractName);
        Assert.Equal(expectedKind, result.FixtureKind);
        Assert.Equal(expectedValidity, result.ExpectedValidity);
        Assert.Equal(expectedValidity, result.ActualValidity);
        Assert.True(result.MatchesExpectation);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "MUOnline.BuildPlanner.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
