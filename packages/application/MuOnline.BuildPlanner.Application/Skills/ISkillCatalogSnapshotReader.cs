namespace MuOnline.BuildPlanner.Application.Skills;

public interface ISkillCatalogSnapshotReader
{
    SkillCatalog Read(string snapshotRoot);
}
