namespace MuOnline.BuildPlanner.Application.Builds;

public interface IBuildRepository
{
    Task SaveAsync(CharacterBuild build, CancellationToken cancellationToken = default);

    Task<CharacterBuild?> LoadAsync(string id, CancellationToken cancellationToken = default);
}