namespace MuOnline.BuildPlanner.Application.Items;

public interface IItemCatalogSnapshotReader
{
    ItemCatalog Read(string snapshotRoot);
}
