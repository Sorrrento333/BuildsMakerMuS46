using System.Windows;
using System.Windows.Controls;
using MuOnline.BuildPlanner.Application.Builds;
using MuOnline.BuildPlanner.Application.Formulas;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Stats;
using MuOnline.BuildPlanner.Domain.Formulas;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;

namespace MuOnline.BuildPlanner.App;

public partial class MainWindow : Window
{
    private readonly ProgressionRulesetCatalog _catalog;
    private readonly CalculateProgressionPointBudgetUseCase _useCase;
    private readonly CalculateStatDistributionUseCase _statDistributionUseCase;
    private readonly ExecutableFormulaCatalog _formulaCatalog;
    private readonly CalculateCharacterFormulaUseCase _characterFormulaUseCase;
    private readonly SaveBuildDraftUseCase _saveBuildDraftUseCase;
    private readonly LoadBuildDraftUseCase _loadBuildDraftUseCase;
    private readonly Dictionary<string, TextBox> _allocationInputs =
        new(StringComparer.Ordinal);
    private ProgressionPointBudgetResult? _currentBudget;
    private ProgressionPointBudgetRequest? _currentProgressionRequest;
    private StatDistributionResult? _currentDistribution;
    private bool _isUpdatingFormulaSelection;

    public MainWindow()
        : this(PublishedBuildDraftServices.CreateDefault())
    {
    }

    private MainWindow(PublishedBuildDraftServices buildDraftServices)
    {
        ArgumentNullException.ThrowIfNull(buildDraftServices);
        InitializeComponent();
        _catalog = PublishedProgressionRuleset.Catalog;
        _useCase = PublishedProgressionRuleset.CreateUseCase();
        _statDistributionUseCase = PublishedProgressionRuleset.CreateStatDistributionUseCase();
        _formulaCatalog = PublishedProgressionRuleset.FormulaCatalog;
        _characterFormulaUseCase =
            PublishedProgressionRuleset.CreateCharacterFormulaUseCase();
        _saveBuildDraftUseCase = buildDraftServices.SaveUseCase;
        _loadBuildDraftUseCase = buildDraftServices.LoadUseCase;

        ClassComboBox.ItemsSource = _catalog.CharacterOptions
            .OrderBy(item => item.DisplayName, StringComparer.CurrentCulture)
            .ToArray();
        ClassComboBox.SelectedIndex = 0;
    }

    private void ClassSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ClassComboBox.SelectedItem is not ProgressionCharacterOption selectedClass)
        {
            EvolutionComboBox.ItemsSource = null;
            HeroStatusCheckBox.IsEnabled = false;
            HeroStatusCheckBox.IsChecked = false;
            BuildStatAllocationInputs(null);
            InvalidateCurrentBudget();
            return;
        }

        EvolutionComboBox.ItemsSource = selectedClass.Evolutions;
        EvolutionComboBox.SelectedIndex = 0;
        BuildStatAllocationInputs(selectedClass.Id);
        UpdateHeroStatusAvailability();
        InvalidateCurrentBudget();
    }

    private void EvolutionSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateHeroStatusAvailability();
        InvalidateCurrentBudget();
    }

    private void UpdateHeroStatusAvailability()
    {
        var questBonus = GetSelectedQuestBonus();
        var selectedEvolution = EvolutionComboBox.SelectedItem as ProgressionEvolutionOption;
        var isEligible = questBonus is not null &&
            selectedEvolution is not null &&
            questBonus.EligibleEvolutionIds.Contains(selectedEvolution.Id);

        HeroStatusCheckBox.IsEnabled = isEligible;
        if (!isEligible)
        {
            HeroStatusCheckBox.IsChecked = false;
        }
    }

    private void CalculateButtonClick(object sender, RoutedEventArgs e)
    {
        if (ClassComboBox.SelectedItem is not ProgressionCharacterOption selectedClass ||
            EvolutionComboBox.SelectedItem is not ProgressionEvolutionOption selectedEvolution)
        {
            ResultTextBox.Text = "Selecciona una clase y una evolución.";
            return;
        }

        if (!int.TryParse(LevelTextBox.Text, out var level))
        {
            ResultTextBox.Text = "El nivel debe ser un número entero.";
            return;
        }

        var questBonus = GetSelectedQuestBonus();
        var completedQuestIds =
            HeroStatusCheckBox.IsChecked == true && questBonus is not null
                ? new[] { questBonus.QuestId }
                : Array.Empty<string>();

        try
        {
            var request = new ProgressionPointBudgetRequest(
                selectedClass.Id,
                selectedEvolution.Id,
                level,
                completedQuestIds);
            var result = _useCase.Execute(request);
            _currentBudget = result;
            _currentProgressionRequest = request;
            _currentDistribution = null;
            DistributeStatsButton.IsEnabled = true;
            DistributionResultTextBox.Text =
                "Presupuesto calculado. Ingresa las asignaciones y distribuye los puntos.";
            InvalidateFormulaResult();
            ResultTextBox.Text = FormatResult(result);
        }
        catch (ProgressionPointBudgetException exception)
        {
            InvalidateCurrentBudget();
            ResultTextBox.Text = $"No se pudo calcular ({exception.Code}): {exception.Message}";
        }
    }

    private void DistributeStatsButtonClick(object sender, RoutedEventArgs e)
    {
        if (_currentBudget is null)
        {
            DistributionResultTextBox.Text =
                "Calcula primero el presupuesto para las entradas actuales.";
            return;
        }

        var allocations = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var (statId, input) in _allocationInputs)
        {
            if (!long.TryParse(input.Text, out var value))
            {
                DistributionResultTextBox.Text =
                    $"La asignación de '{statId}' debe ser un número entero.";
                return;
            }

            allocations.Add(statId, value);
        }

        try
        {
            if (!TryReadResetInputs(out var resetInputs, out var validationMessage))
            {
                DistributionResultTextBox.Text = validationMessage;
                return;
            }

            var result = _statDistributionUseCase.Execute(
                _currentBudget,
                resetInputs,
                allocations);
            _currentDistribution = result;
            DistributionResultTextBox.Text = FormatDistributionResult(result);
            ConfigureAndCalculateApplicableFormula();
        }
        catch (StatDistributionException exception)
        {
            DistributionResultTextBox.Text =
                $"No se pudo distribuir ({exception.Code}): " +
                TranslateDistributionError(exception.Code);
        }
    }

    private async void SaveBuildDraftButtonClick(object sender, RoutedEventArgs e)
    {
        if (!TryReadDraftInputs(
                out var progressionInputs,
                out var resetInputs,
                out var allocations,
                out var validationMessage))
        {
            BuildDraftResultTextBox.Text = validationMessage;
            return;
        }

        try
        {
            var draft = await _saveBuildDraftUseCase.ExecuteAsync(
                new SaveBuildDraftRequest(
                    BuildDraftIdTextBox.Text.Trim(),
                    progressionInputs,
                    resetInputs,
                    allocations));
            BuildDraftResultTextBox.Text =
                $"Borrador '{draft.Id}' guardado. " +
                $"Dataset {draft.Dataset.Version} ({draft.Dataset.Hash[..15]}…).";
        }
        catch (BuildDraftException exception)
        {
            BuildDraftResultTextBox.Text =
                $"No se pudo guardar ({exception.Code}): " +
                TranslateBuildDraftError(exception.Code);
        }
        catch (StatDistributionException exception)
        {
            BuildDraftResultTextBox.Text =
                $"No se pudo guardar ({exception.Code}): " +
                TranslateDistributionError(exception.Code);
        }
        catch (ProgressionPointBudgetException exception)
        {
            BuildDraftResultTextBox.Text =
                $"No se pudo guardar ({exception.Code}): {exception.Message}";
        }
    }

    private async void LoadBuildDraftButtonClick(object sender, RoutedEventArgs e)
    {
        var id = BuildDraftIdTextBox.Text.Trim();
        if (!IsValidBuildDraftId(id))
        {
            BuildDraftResultTextBox.Text =
                "El ID debe usar minúsculas, números y guiones simples.";
            return;
        }

        try
        {
            var draft = await _loadBuildDraftUseCase.ExecuteAsync(id);
            ApplyLoadedDraft(draft);
            BuildDraftResultTextBox.Text =
                $"Borrador '{draft.Id}' cargado y revalidado contra el snapshot exacto.";
        }
        catch (BuildDraftException exception)
        {
            BuildDraftResultTextBox.Text =
                $"No se pudo cargar ({exception.Code}): " +
                TranslateBuildDraftError(exception.Code);
        }
    }

    private QuestPointBonusRule? GetSelectedQuestBonus()
    {
        if (ClassComboBox.SelectedItem is not ProgressionCharacterOption selectedClass)
        {
            return null;
        }

        return _catalog.Rules
            .Single(rule => rule.AppliesToClassIds.Contains(selectedClass.Id))
            .QuestBonus;
    }

    private void BuildStatAllocationInputs(string? characterClassId)
    {
        _allocationInputs.Clear();
        StatsAllocationPanel.Children.Clear();

        if (characterClassId is null)
        {
            return;
        }

        var characterClass = _catalog.Classes.Single(item => item.Id == characterClassId);
        foreach (var statId in characterClass.StatIds.Order(StringComparer.Ordinal))
        {
            var row = new Grid
            {
                Margin = new Thickness(0, 4, 0, 4),
            };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var label = new TextBlock
            {
                Text = statId,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var input = new TextBox
            {
                Text = "0",
                MinHeight = 28,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            input.TextChanged += AllocationInputChanged;
            Grid.SetColumn(input, 1);
            row.Children.Add(label);
            row.Children.Add(input);
            StatsAllocationPanel.Children.Add(row);
            _allocationInputs.Add(statId, input);
        }
    }

    private void ProgressionInputChanged(object sender, RoutedEventArgs e) =>
        InvalidateCurrentBudget();

    private void AllocationInputChanged(object sender, TextChangedEventArgs e)
    {
        if (_currentBudget is not null)
        {
            DistributionResultTextBox.Clear();
            _currentDistribution = null;
            InvalidateFormulaResult();
        }
    }

    private bool TryReadDraftInputs(
        out BuildDraftProgressionInputs progressionInputs,
        out BuildDraftResetInputs resetInputs,
        out IReadOnlyDictionary<string, long> allocations,
        out string validationMessage)
    {
        progressionInputs = null!;
        resetInputs = null!;
        allocations = null!;
        validationMessage = string.Empty;
        var id = BuildDraftIdTextBox.Text.Trim();
        if (!IsValidBuildDraftId(id))
        {
            validationMessage =
                "El ID debe usar minúsculas, números y guiones simples.";
            return false;
        }

        if (ClassComboBox.SelectedItem is not ProgressionCharacterOption selectedClass ||
            EvolutionComboBox.SelectedItem is not ProgressionEvolutionOption selectedEvolution)
        {
            validationMessage = "Selecciona una clase y una evolución.";
            return false;
        }

        if (!int.TryParse(LevelTextBox.Text, out var level))
        {
            validationMessage = "El nivel debe ser un número entero.";
            return false;
        }

        var parsedAllocations = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var (statId, input) in _allocationInputs)
        {
            if (!long.TryParse(input.Text, out var value))
            {
                validationMessage =
                    $"La asignación de '{statId}' debe ser un número entero.";
                return false;
            }

            parsedAllocations.Add(statId, value);
        }

        var questBonus = GetSelectedQuestBonus();
        var completedQuestIds =
            HeroStatusCheckBox.IsChecked == true && questBonus is not null
                ? new[] { questBonus.QuestId }
                : Array.Empty<string>();
        progressionInputs = new BuildDraftProgressionInputs(
            selectedClass.Id,
            selectedEvolution.Id,
            level,
            completedQuestIds);
        if (!TryReadResetInputs(out var domainResetInputs, out validationMessage))
        {
            return false;
        }

        resetInputs = new BuildDraftResetInputs(
            domainResetInputs.ResetCount,
            domainResetInputs.PointsPerReset);
        allocations = parsedAllocations;
        return true;
    }

    private void ApplyLoadedDraft(BuildDraft draft)
    {
        var selectedClass = _catalog.CharacterOptions.Single(
            item => item.Id == draft.ProgressionInputs.CharacterClassId);
        ClassComboBox.SelectedItem = selectedClass;
        EvolutionComboBox.SelectedItem = selectedClass.Evolutions.Single(
            item => item.Id == draft.ProgressionInputs.EvolutionId);
        LevelTextBox.Text = draft.ProgressionInputs.Level.ToString(
            System.Globalization.CultureInfo.InvariantCulture);

        var questBonus = GetSelectedQuestBonus();
        HeroStatusCheckBox.IsChecked =
            questBonus is not null &&
            draft.ProgressionInputs.CompletedQuestIds.Contains(
                questBonus.QuestId,
                StringComparer.Ordinal);
        ResetCountTextBox.Text = draft.ResetInputs.ResetCount.ToString(
            System.Globalization.CultureInfo.InvariantCulture);
        PointsPerResetTextBox.Text = draft.ResetInputs.PointsPerReset.ToString(
            System.Globalization.CultureInfo.InvariantCulture);

        foreach (var (statId, value) in draft.StatDistribution.Allocations)
        {
            _allocationInputs[statId].Text = value.ToString(
                System.Globalization.CultureInfo.InvariantCulture);
        }

        _currentProgressionRequest = new ProgressionPointBudgetRequest(
            draft.ProgressionInputs.CharacterClassId,
            draft.ProgressionInputs.EvolutionId,
            draft.ProgressionInputs.Level,
            draft.ProgressionInputs.CompletedQuestIds);
        _currentBudget = _useCase.Execute(_currentProgressionRequest);
        DistributeStatsButton.IsEnabled = true;
        ResultTextBox.Text = FormatResult(_currentBudget);
        _currentDistribution = new StatDistributionResult(
                draft.StatDistribution.RulesetId,
                draft.StatDistribution.CharacterClassId,
                draft.StatDistribution.ProgressionRule.Id,
                draft.StatDistribution.ProgressionRule.Version,
                draft.StatDistribution.EarnedPoints,
                new ResetPointInputs(
                    draft.StatDistribution.ResetInputs.ResetCount,
                    draft.StatDistribution.ResetInputs.PointsPerReset),
                draft.StatDistribution.ResetPoints,
                draft.StatDistribution.TotalDistributablePoints,
                draft.StatDistribution.Allocations,
                draft.StatDistribution.SpentPoints,
                draft.StatDistribution.RemainingPoints);
        DistributionResultTextBox.Text = FormatDistributionResult(
            _currentDistribution);
        ConfigureAndCalculateApplicableFormula();
    }

    private void InvalidateCurrentBudget()
    {
        _currentBudget = null;
        _currentProgressionRequest = null;
        _currentDistribution = null;
        if (DistributeStatsButton is not null)
        {
            DistributeStatsButton.IsEnabled = false;
        }

        DistributionResultTextBox?.Clear();
        InvalidateFormulaResult();
    }

    private void ResetInputChanged(object sender, TextChangedEventArgs e)
    {
        if (TotalResetPointsTextBox is null)
        {
            return;
        }

        if (!long.TryParse(ResetCountTextBox.Text, out var resetCount) ||
            !long.TryParse(PointsPerResetTextBox.Text, out var pointsPerReset) ||
            resetCount < 0 ||
            pointsPerReset < 0)
        {
            TotalResetPointsTextBox.Text = "Entrada inválida";
        }
        else
        {
            try
            {
                TotalResetPointsTextBox.Text = checked(resetCount * pointsPerReset)
                    .ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                TotalResetPointsTextBox.Text = "Fuera de rango";
            }
        }

        DistributionResultTextBox?.Clear();
        _currentDistribution = null;
        InvalidateFormulaResult();
        BuildDraftResultTextBox?.Clear();
    }

    private void ConfigureAndCalculateApplicableFormula()
    {
        if (_currentProgressionRequest is null ||
            _currentBudget is null ||
            _currentDistribution is null)
        {
            FormulaResultTextBox.Text =
                "Distribuye los puntos antes de calcular atributos derivados.";
            return;
        }

        var options = _formulaCatalog.Formulas
            .Where(formula =>
                formula.Applicability.CharacterClassId ==
                    _currentProgressionRequest.ClassId &&
                formula.Applicability.EvolutionIds.Contains(
                    _currentProgressionRequest.EvolutionId))
            .OrderBy(formula => formula.Output.Id, StringComparer.Ordinal)
            .ThenBy(formula => formula.Reference.Id, StringComparer.Ordinal)
            .ThenBy(formula => formula.Reference.Version, StringComparer.Ordinal)
            .Select(formula => new FormulaSelectionOption(
                formula.Reference,
                $"{formula.Output.Id} — {formula.Reference.Id} " +
                $"v{formula.Reference.Version}"))
            .ToArray();
        if (options.Length == 0)
        {
            InvalidateFormulaSelection();
            FormulaResultTextBox.Text =
                "No hay una fórmula derivada publicada para esta clase y evolución.";
            return;
        }

        var previousReference =
            (FormulaComboBox.SelectedItem as FormulaSelectionOption)?.Reference;
        _isUpdatingFormulaSelection = true;
        try
        {
            FormulaComboBox.ItemsSource = options;
            FormulaComboBox.SelectedItem = options.FirstOrDefault(
                option => option.Reference == previousReference) ?? options[0];
        }
        finally
        {
            _isUpdatingFormulaSelection = false;
        }

        CalculateAndDisplaySelectedFormula();
    }

    private void FormulaSelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!_isUpdatingFormulaSelection)
        {
            CalculateAndDisplaySelectedFormula();
        }
    }

    private void CalculateAndDisplaySelectedFormula()
    {
        if (_currentProgressionRequest is null ||
            _currentDistribution is null ||
            FormulaComboBox.SelectedItem is not FormulaSelectionOption selected)
        {
            return;
        }

        try
        {
            var result = _characterFormulaUseCase.Execute(
                selected.Reference,
                _currentProgressionRequest,
                _currentDistribution.ResetInputs,
                _currentDistribution.Allocations);
            FormulaResultTextBox.Text = FormatFormulaResult(result);
        }
        catch (FormulaContextException exception)
        {
            FormulaResultTextBox.Text =
                $"No se pudo resolver el contexto ({exception.Code}): " +
                TranslateFormulaContextError(exception.Code);
        }
        catch (FormulaCalculationException exception)
        {
            FormulaResultTextBox.Text =
                $"No se pudo calcular ({exception.Code}): {exception.Message}";
        }
        catch (FormulaExecutionException exception)
        {
            FormulaResultTextBox.Text =
                $"No se pudo ejecutar ({exception.Code}): {exception.Message}";
        }
    }

    private void InvalidateFormulaResult()
    {
        InvalidateFormulaSelection();
        FormulaResultTextBox?.Clear();
    }

    private void InvalidateFormulaSelection()
    {
        if (FormulaComboBox is null)
        {
            return;
        }

        _isUpdatingFormulaSelection = true;
        try
        {
            FormulaComboBox.ItemsSource = null;
        }
        finally
        {
            _isUpdatingFormulaSelection = false;
        }
    }

    private static string FormatFormulaResult(
        CharacterFormulaCalculationResult result)
    {
        var formula = result.Formula;
        var lines = new List<string>
        {
            $"{formula.OutputId}: {formula.VisibleOutput}",
            $"Fórmula: {formula.Trace.FormulaReference.Id} " +
            $"v{formula.Trace.FormulaReference.Version}",
            "Traza contextual:",
        };
        lines.AddRange(result.ContextTrace.Select(item =>
            item.Kind == FormulaContextResolutionKind.CharacterLevel
                ? $"- {item.InputId} ← {item.ContextValueId}: " +
                  $"{item.ResolvedValue} (nivel validado)"
                : $"- {item.InputId} ← {item.ContextValueId}: " +
                  $"{item.BaseValue} + {item.Allocation} = {item.ResolvedValue} " +
                  $"[CHECKED_ADD; {string.Join(", ", item.EvidenceRefs)}]"));
        if (result.DependencyTrace.Length != 0)
        {
            lines.Add("Traza de dependencias:");
            lines.AddRange(result.DependencyTrace.Select(item =>
                $"- {item.ConsumerFormulaReference.Id}.{item.InputId} ← " +
                $"{item.FormulaReference.Id} " +
                $"v{item.FormulaReference.Version} [{item.OutputStage}]: " +
                item.ResolvedValue));
        }

        lines.Add("Traza aritmética:");
        lines.AddRange(formula.Trace.Steps.Select(
            step => $"- {step.StepId}: {step.Value}"));
        lines.Add(
            $"Salida cruda: {formula.RawOutput}; visible: {formula.VisibleOutput}; " +
            $"redondeo: {formula.Trace.Rounding.Mode}.");
        return string.Join(Environment.NewLine, lines);
    }

    private static string TranslateFormulaContextError(string code) => code switch
    {
        FormulaContextErrorCodes.StateMismatch =>
            "el estado validado no coincide con la fórmula solicitada.",
        FormulaContextErrorCodes.SourceNotSupported =>
            "la fórmula usa una fuente de input todavía no soportada.",
        FormulaContextErrorCodes.ValueNotResolvable =>
            "un valor contextual no puede obtenerse del estado validado.",
        FormulaContextErrorCodes.BaseStatMissing =>
            "falta el valor base canónico requerido.",
        FormulaContextErrorCodes.AllocationMissing =>
            "falta una asignación validada requerida.",
        FormulaContextErrorCodes.ArithmeticOverflow =>
            "la suma comprobada de base y asignación excede el rango permitido.",
        FormulaContextErrorCodes.DependencyCycle =>
            "las fórmulas dependientes forman un ciclo.",
        FormulaContextErrorCodes.DependencyIncoherent =>
            "una dependencia no declara una referencia y etapa de salida coherentes.",
        _ => "se produjo un error de contexto no reconocido.",
    };

    private sealed record FormulaSelectionOption(
        FormulaReference Reference,
        string DisplayName);

    private bool TryReadResetInputs(
        out ResetPointInputs resetInputs,
        out string validationMessage)
    {
        resetInputs = null!;
        validationMessage = string.Empty;
        if (!long.TryParse(ResetCountTextBox.Text, out var resetCount))
        {
            validationMessage = "La cantidad de resets debe ser un número entero.";
            return false;
        }

        if (!long.TryParse(PointsPerResetTextBox.Text, out var pointsPerReset))
        {
            validationMessage = "Los puntos por reset deben ser un número entero.";
            return false;
        }

        resetInputs = new ResetPointInputs(resetCount, pointsPerReset);
        return true;
    }

    private static string FormatResult(ProgressionPointBudgetResult result)
    {
        var lines = new List<string>
        {
            $"Puntos ganados: {result.EarnedPoints}",
            $"Regla: {result.ProgressionRuleId} v{result.ProgressionRuleVersion}",
            "Traza:",
        };
        lines.AddRange(result.Contributions.Select(contribution =>
            $"- {contribution.Kind}: {contribution.AwardedLevelCount} × " +
            $"{contribution.PointsPerLevel} = {contribution.EarnedPoints} " +
            $"({contribution.SourceId})"));
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatDistributionResult(StatDistributionResult result)
    {
        var lines = new List<string>
        {
            $"Puntos por nivel/quests: {result.EarnedPoints}",
            $"Resets: {result.ResetInputs.ResetCount}",
            $"Puntos por reset: {result.ResetInputs.PointsPerReset}",
            $"Puntos totales por resets: {result.ResetPoints}",
            $"Puntos distribuibles totales: {result.TotalDistributablePoints}",
            $"Puntos gastados: {result.SpentPoints}",
            $"Puntos restantes: {result.RemainingPoints}",
            "Asignaciones:",
        };
        lines.AddRange(result.Allocations
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => $"- {item.Key}: {item.Value}"));
        return string.Join(Environment.NewLine, lines);
    }

    private static string TranslateDistributionError(string code) => code switch
    {
        StatDistributionErrorCodes.AllocationNegative =>
            "las asignaciones no pueden ser negativas.",
        StatDistributionErrorCodes.StatNotAvailable =>
            "se recibió un stat que no está disponible para la clase seleccionada.",
        StatDistributionErrorCodes.StatAllocationMissing =>
            "falta una asignación para uno de los stats disponibles.",
        StatDistributionErrorCodes.AllocationExceedsEarnedPoints =>
            "la suma asignada supera los puntos ganados.",
        StatDistributionErrorCodes.AllocationOverflow =>
            "la suma de asignaciones excede el rango numérico permitido.",
        StatDistributionErrorCodes.BudgetSourceMismatch =>
            "el presupuesto no corresponde a la clase, ruleset o regla cargados.",
        StatDistributionErrorCodes.ResetCountNegative =>
            "la cantidad de resets no puede ser negativa.",
        StatDistributionErrorCodes.PointsPerResetNegative =>
            "los puntos por reset no pueden ser negativos.",
        StatDistributionErrorCodes.ResetPointsOverflow =>
            "el total de puntos por resets excede el rango permitido.",
        StatDistributionErrorCodes.TotalDistributablePointsOverflow =>
            "el presupuesto distribuible total excede el rango permitido.",
        _ => "se produjo un error de distribución no reconocido.",
    };

    private static bool IsValidBuildDraftId(string id)
    {
        if (string.IsNullOrWhiteSpace(id) ||
            id[0] == '-' ||
            id[^1] == '-')
        {
            return false;
        }

        var previousWasHyphen = false;
        foreach (var character in id)
        {
            if (character == '-')
            {
                if (previousWasHyphen)
                {
                    return false;
                }

                previousWasHyphen = true;
                continue;
            }

            if (!char.IsAsciiLetterLower(character) && !char.IsAsciiDigit(character))
            {
                return false;
            }

            previousWasHyphen = false;
        }

        return true;
    }

    private static string TranslateBuildDraftError(string code) => code switch
    {
        BuildDraftErrorCodes.NotFound =>
            "no existe un borrador con ese ID.",
        BuildDraftErrorCodes.SchemaUnsupported =>
            "el borrador usa una versión de schema no soportada.",
        BuildDraftErrorCodes.DependencyUnavailable =>
            "no está disponible exactamente el ruleset, dataset o motor guardado.",
        BuildDraftErrorCodes.SourceMismatch =>
            "las identidades internas del borrador no son coherentes.",
        BuildDraftErrorCodes.RevalidationFailed =>
            "el recálculo no reproduce la caché persistida.",
        BuildDraftErrorCodes.WriteConflict =>
            "la base local siguió ocupada después de los reintentos configurados.",
        _ => "se produjo un error de borrador no reconocido.",
    };
}
