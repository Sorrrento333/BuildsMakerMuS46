namespace MuOnline.BuildPlanner.Application.Formulas;

public interface IExecutableFormulaSnapshotReader
{
    ExecutableFormulaCatalog Read(string snapshotRoot);
}
