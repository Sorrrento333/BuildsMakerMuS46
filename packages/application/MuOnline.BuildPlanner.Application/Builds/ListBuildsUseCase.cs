namespace MuOnline.BuildPlanner.Application.Builds;

public sealed class ListBuildsUseCase
{
    private readonly IBuildRepository buildRepository;

    public ListBuildsUseCase(IBuildRepository buildRepository)
    {
        ArgumentNullException.ThrowIfNull(buildRepository);
        this.buildRepository = buildRepository;
    }

    public async Task<IReadOnlyList<CharacterBuildSummary>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var builds = await buildRepository.ListAsync(cancellationToken).ConfigureAwait(false);
        return builds
            .OrderBy(item => item.Id, StringComparer.Ordinal)
            .ToArray();
    }
}