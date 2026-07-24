namespace MuOnline.BuildPlanner.Application.Progression;

public interface IProgressionRulesetSnapshotReader
{
    ProgressionRulesetCatalog Read(string snapshotRoot);
}
