namespace MuOnline.BuildPlanner.Application.Builds;

public interface IBuildDraftRepository
{
    Task SaveAsync(BuildDraft draft, CancellationToken cancellationToken = default);

    Task<BuildDraft?> LoadAsync(string id, CancellationToken cancellationToken = default);
}
