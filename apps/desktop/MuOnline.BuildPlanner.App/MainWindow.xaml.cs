using System.Windows;
using System.Windows.Controls;
using MuOnline.BuildPlanner.Application.Builds;
using MuOnline.BuildPlanner.Application.Formulas;
using MuOnline.BuildPlanner.Application.Items;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Skills;
using MuOnline.BuildPlanner.Application.Stats;
using MuOnline.BuildPlanner.Domain.Formulas;
using MuOnline.BuildPlanner.Domain.Items;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Skills;
using MuOnline.BuildPlanner.Domain.Stats;

namespace MuOnline.BuildPlanner.App;

public partial class MainWindow : Window
{
    private readonly ProgressionRulesetCatalog _catalog;
    private readonly CalculateProgressionPointBudgetUseCase _useCase;
    private readonly CalculateStatDistributionUseCase _statDistributionUseCase;
    private readonly ExecutableFormulaCatalog _formulaCatalog;
    private readonly CalculateCharacterFormulaUseCase _characterFormulaUseCase;
    private readonly CalculateCharacterBuildUseCase _characterBuildUseCase;
    private readonly SaveBuildDraftUseCase _saveBuildDraftUseCase;
    private readonly LoadBuildDraftUseCase _loadBuildDraftUseCase;
    private readonly SaveBuildUseCase _saveBuildUseCase;
    private readonly LoadBuildUseCase _loadBuildUseCase;
    private readonly ListBuildsUseCase _listBuildsUseCase;
    private readonly ItemCatalog _itemCatalog;
    private readonly EquipItemUseCase _equipItemUseCase;
    private readonly SkillCatalog _skillCatalog;
    private readonly LearnSkillUseCase _learnSkillUseCase;
    private readonly Dictionary<string, TextBox> _allocationInputs =
        new(StringComparer.Ordinal);
    private ProgressionPointBudgetResult? _currentBudget;
    private ProgressionPointBudgetRequest? _currentProgressionRequest;
    private StatDistributionResult? _currentDistribution;
    private bool _isUpdatingFormulaSelection;
    private bool _isUpdatingItemSelection;
    private bool _isUpdatingSkillSelection;

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
        _characterBuildUseCase =
            PublishedProgressionRuleset.CreateCharacterBuildUseCase();
        _saveBuildDraftUseCase = buildDraftServices.SaveUseCase;
        _loadBuildDraftUseCase = buildDraftServices.LoadUseCase;
        _saveBuildUseCase = buildDraftServices.SaveBuildUseCase;
        _loadBuildUseCase = buildDraftServices.LoadBuildUseCase;
        _listBuildsUseCase = buildDraftServices.ListBuildsUseCase;
        _itemCatalog = PublishedProgressionRuleset.ItemCatalog;
        _equipItemUseCase = PublishedProgressionRuleset.CreateEquipItemUseCase();
        _skillCatalog = PublishedProgressionRuleset.Skills;
        _learnSkillUseCase = PublishedProgressionRuleset.CreateLearnSkillUseCase();

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
            RefreshItemSelection();
            RefreshSkillSelection();
            return;
        }

        EvolutionComboBox.ItemsSource = selectedClass.Evolutions;
        EvolutionComboBox.SelectedIndex = 0;
        BuildStatAllocationInputs(selectedClass.Id);
        UpdateHeroStatusAvailability();
        InvalidateCurrentBudget();
        RefreshItemSelection();
        RefreshSkillSelection();
    }

    private void EvolutionSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateHeroStatusAvailability();
        InvalidateCurrentBudget();
        RefreshSkillSelection();
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
            RefreshItemSelection();
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

    private void ProgressionInputChanged(object sender, RoutedEventArgs e)
    {
        InvalidateCurrentBudget();
        EvaluateSelectedSkill();
    }

    private void AllocationInputChanged(object sender, TextChangedEventArgs e)
    {
        if (_currentBudget is not null)
        {
            DistributionResultTextBox.Clear();
            _currentDistribution = null;
            InvalidateFormulaResult();
        }

        EvaluateSelectedItem();
    }

    private void RefreshItemSelection()
    {
        if (ItemComboBox is null)
        {
            return;
        }

        _isUpdatingItemSelection = true;
        try
        {
            if (ClassComboBox.SelectedItem is not ProgressionCharacterOption selectedClass)
            {
                ItemSlotComboBox.ItemsSource = null;
                ItemComboBox.ItemsSource = null;
                return;
            }

            var previousSlot =
                (ItemSlotComboBox.SelectedItem as ItemSlotOption)?.Id;
            var slots = _itemCatalog.Items
                .Where(item => item.AllowedClassIds.Contains(selectedClass.Id))
                .SelectMany(item => item.Slots)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .Select(slot => new ItemSlotOption(slot, slot))
                .ToArray();
            ItemSlotComboBox.ItemsSource = slots;
            ItemSlotComboBox.SelectedItem =
                slots.FirstOrDefault(slot => slot.Id == previousSlot) ??
                slots.FirstOrDefault();
            UpdateItemOptions(selectedClass.Id);
        }
        finally
        {
            _isUpdatingItemSelection = false;
        }

        EvaluateSelectedItem();
    }

    private void UpdateItemOptions(string characterClassId)
    {
        var slot = (ItemSlotComboBox.SelectedItem as ItemSlotOption)?.Id;
        var previousItemId = (ItemComboBox.SelectedItem as ItemDefinition)?.Id;
        var items = _itemCatalog.Items
            .Where(item =>
                item.AllowedClassIds.Contains(characterClassId) &&
                (slot is null || item.Slots.Contains(slot)))
            .OrderBy(item => item.DisplayName, StringComparer.CurrentCulture)
            .ToArray();
        ItemComboBox.ItemsSource = items;
        ItemComboBox.SelectedItem =
            items.FirstOrDefault(item => item.Id == previousItemId) ??
            items.FirstOrDefault();
    }

    private void ItemSlotSelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_isUpdatingItemSelection ||
            ClassComboBox.SelectedItem is not ProgressionCharacterOption selectedClass)
        {
            return;
        }

        _isUpdatingItemSelection = true;
        try
        {
            UpdateItemOptions(selectedClass.Id);
        }
        finally
        {
            _isUpdatingItemSelection = false;
        }

        EvaluateSelectedItem();
    }

    private void ItemSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isUpdatingItemSelection)
        {
            EvaluateSelectedItem();
        }
    }

    private void EvaluateSelectedItem()
    {
        if (ItemResultTextBox is null)
        {
            return;
        }

        if (ClassComboBox.SelectedItem is not ProgressionCharacterOption selectedClass)
        {
            ItemResultTextBox.Text = "Selecciona una clase.";
            return;
        }

        if (ItemComboBox.SelectedItem is not ItemDefinition item)
        {
            ItemResultTextBox.Text =
                "No hay ítems publicados para la clase y ranura seleccionadas.";
            return;
        }

        if (!TryBuildFinalStats(out var finalStats))
        {
            ItemResultTextBox.Text =
                "Calcula el presupuesto y distribuye los puntos para validar el equipado.";
            return;
        }

        try
        {
            var result = _equipItemUseCase.Execute(
                new EquipItemRequest(selectedClass.Id, finalStats, item.Id));
            ItemResultTextBox.Text = FormatItemEquipResult(result);
        }
        catch (ItemEquipException exception)
        {
            ItemResultTextBox.Text =
                $"No elegible ({exception.Code}): " +
                TranslateItemEquipError(exception.Code);
        }
    }

    private bool TryBuildFinalStats(
        out IReadOnlyDictionary<string, long> finalStats)
    {
        finalStats = null!;
        if (_currentProgressionRequest is null ||
            _currentDistribution is null)
        {
            return false;
        }

        var characterClass = _catalog.Classes.Single(
            item => item.Id == _currentProgressionRequest.ClassId);
        var stats = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var statId in characterClass.StatIds)
        {
            var allocation = _currentDistribution.Allocations.TryGetValue(
                statId,
                out var value)
                ? value
                : 0L;
            stats[statId] = checked(
                characterClass.BaseStats[statId].BaseValue + allocation);
        }

        finalStats = stats;
        return true;
    }

    private static string FormatItemEquipResult(EquipItemResult result)
    {
        var lines = new List<string>
        {
            $"Elegible: {result.DisplayName} ({result.ItemId})",
            $"Ranuras: {string.Join(", ", result.Slots)}",
            "Requisitos publicados en nivel +0:",
        };
        lines.AddRange(result.RequiredStats
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => $"- {item.Key}: {item.Value}"));
        lines.Add(
            "Sin bonificaciones ni progresión de nivel: fuera del axioma acotado.");
        return string.Join(Environment.NewLine, lines);
    }

    private static string TranslateItemEquipError(string code) => code switch
    {
        ItemEquipErrorCodes.ItemNotFound =>
            "el ítem no existe en el catálogo publicado.",
        ItemEquipErrorCodes.ClassNotAllowed =>
            "la clase seleccionada no puede equipar este ítem.",
        ItemEquipErrorCodes.RequirementsNotMet =>
            "los stats finales no alcanzan los requisitos publicados del ítem.",
        _ => "se produjo un error de equipado no reconocido.",
    };

    private sealed record ItemSlotOption(string Id, string DisplayName);

    private void RefreshSkillSelection()
    {
        if (SkillComboBox is null)
        {
            return;
        }

        _isUpdatingSkillSelection = true;
        try
        {
            if (EvolutionComboBox.SelectedItem is not ProgressionEvolutionOption selectedEvolution)
            {
                SkillComboBox.ItemsSource = null;
                SkillResultTextBox?.Clear();
                return;
            }

            var previousSkillId = (SkillComboBox.SelectedItem as SkillDefinition)?.Id;
            var skills = _skillCatalog.Skills
                .Where(skill =>
                    skill.AllowedEvolutionIds.Contains(selectedEvolution.Id))
                .OrderBy(skill => skill.RequiredLevel)
                .ThenBy(skill => skill.DisplayName, StringComparer.CurrentCulture)
                .ToArray();
            SkillComboBox.ItemsSource = skills;
            SkillComboBox.SelectedItem =
                skills.FirstOrDefault(skill => skill.Id == previousSkillId) ??
                skills.FirstOrDefault();
        }
        finally
        {
            _isUpdatingSkillSelection = false;
        }

        EvaluateSelectedSkill();
    }

    private void SkillSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isUpdatingSkillSelection)
        {
            EvaluateSelectedSkill();
        }
    }

    private void EvaluateSelectedSkill()
    {
        if (SkillResultTextBox is null)
        {
            return;
        }

        if (EvolutionComboBox.SelectedItem is not ProgressionEvolutionOption selectedEvolution)
        {
            SkillResultTextBox.Text =
                "Selecciona una clase y una evolución para evaluar el aprendizaje.";
            return;
        }

        if (SkillComboBox.SelectedItem is not SkillDefinition selectedSkill)
        {
            SkillResultTextBox.Text =
                "No hay skills publicadas para la evolución seleccionada.";
            return;
        }

        if (!int.TryParse(LevelTextBox.Text, out var level) || level < 1)
        {
            SkillResultTextBox.Text = "El nivel debe ser un número entero.";
            return;
        }

        try
        {
            var result = _learnSkillUseCase.Execute(
                new LearnSkillRequest(
                    selectedEvolution.Id,
                    level,
                    selectedSkill.Id));
            SkillResultTextBox.Text = FormatSkillLearnResult(result);
        }
        catch (SkillLearnException exception)
        {
            SkillResultTextBox.Text =
                $"No aprendible ({exception.Code}): " +
                TranslateSkillLearnError(exception.Code);
        }
    }

    private static string FormatSkillLearnResult(LearnSkillResult result)
    {
        var lines = new List<string>
        {
            $"Aprendible: {result.DisplayName} ({result.SkillId})",
            $"Tipo: {result.Kind}",
            $"Nivel requerido: {result.RequiredLevel}",
            "Evoluciones permitidas según el axioma:",
        };
        lines.AddRange(result.AllowedEvolutionIds
            .OrderBy(id => id, StringComparer.Ordinal)
            .Select(id => $"- {id}"));
        lines.Add(
            "Sin reducción de nivel ni buffRef: fuera del axioma acotado.");
        return string.Join(Environment.NewLine, lines);
    }

    private static string TranslateSkillLearnError(string code) => code switch
    {
        SkillLearnErrorCodes.SkillNotFound =>
            "la skill no existe en el catálogo publicado.",
        SkillLearnErrorCodes.EvolutionNotAllowed =>
            "la evolución seleccionada no puede aprender esta skill.",
        SkillLearnErrorCodes.RequirementsNotMet =>
            "el nivel final no alcanza el nivel requerido publicado de la skill.",
        _ => "se produjo un error de aprendizaje no reconocido.",
    };

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
        RefreshItemSelection();
        RefreshSkillSelection();
    }

    private void ApplyLoadedBuild(CharacterBuild build)
    {
        var selectedClass = _catalog.CharacterOptions.Single(
            item => item.Id == build.CharacterClassId);
        ClassComboBox.SelectedItem = selectedClass;
        EvolutionComboBox.SelectedItem = selectedClass.Evolutions.Single(
            item => item.Id == build.EvolutionId);
        LevelTextBox.Text = build.Level.ToString(
            System.Globalization.CultureInfo.InvariantCulture);

        var questBonus = GetSelectedQuestBonus();
        HeroStatusCheckBox.IsChecked =
            questBonus is not null &&
            build.QuestIds.Contains(questBonus.QuestId, StringComparer.Ordinal);
        ResetCountTextBox.Text = build.ResetCount.ToString(
            System.Globalization.CultureInfo.InvariantCulture);
        PointsPerResetTextBox.Text = build.PointsPerReset.ToString(
            System.Globalization.CultureInfo.InvariantCulture);

        var characterClass = _catalog.Classes.Single(
            item => item.Id == build.CharacterClassId);
        var allocations = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var statId in characterClass.StatIds.Order(StringComparer.Ordinal))
        {
            var allocation = build.Stats[statId] -
                characterClass.BaseStats[statId].BaseValue;
            allocations.Add(statId, allocation);
            _allocationInputs[statId].Text = allocation.ToString(
                System.Globalization.CultureInfo.InvariantCulture);
        }

        _currentProgressionRequest = new ProgressionPointBudgetRequest(
            build.CharacterClassId,
            build.EvolutionId,
            build.Level,
            build.QuestIds);
        _currentBudget = _useCase.Execute(_currentProgressionRequest);
        DistributeStatsButton.IsEnabled = true;
        ResultTextBox.Text = FormatResult(_currentBudget);
        _currentDistribution = _statDistributionUseCase.Execute(
            _currentBudget,
            new ResetPointInputs(build.ResetCount, build.PointsPerReset),
            allocations);
        DistributionResultTextBox.Text = FormatDistributionResult(
            _currentDistribution);
        ConfigureAndCalculateApplicableFormula();
        RefreshItemSelection();
        RefreshSkillSelection();
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
        EvaluateSelectedItem();
    }

    private void EvaluateBuildButtonClick(object sender, RoutedEventArgs e)
    {
        if (_currentProgressionRequest is null ||
            _currentDistribution is null)
        {
            FormulaResultTextBox.Text =
                "Calcula el presupuesto y distribuye los puntos antes de evaluar la build.";
            return;
        }

        try
        {
            var evaluation = _characterBuildUseCase.Execute(
                _currentProgressionRequest,
                _currentDistribution.ResetInputs,
                _currentDistribution.Allocations);
            FormulaResultTextBox.Text = FormatBuildResult(evaluation);
        }
        catch (FormulaContextException exception)
        {
            FormulaResultTextBox.Text =
                $"No se pudo evaluar la build ({exception.Code}): " +
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

    private static string FormatBuildResult(CharacterBuildEvaluation evaluation)
    {
        var lines = new List<string>
        {
            $"Clase: {evaluation.State.CharacterClass.Id} " +
            $"(evolución {evaluation.State.ProgressionRequest.EvolutionId})",
            $"Fórmulas evaluadas: {evaluation.Formulas.Length}",
        };
        foreach (var group in evaluation.Formulas
                     .OrderBy(item => item.Formula.Reference.Id, StringComparer.Ordinal)
                     .ThenBy(item => item.Formula.Reference.Version, StringComparer.Ordinal)
                     .GroupBy(item => BuildGroupLabel(item.Formula), StringComparer.Ordinal)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            lines.Add(string.Empty);
            lines.Add($"== {group.Key} ==");
            lines.AddRange(group.Select(item =>
                $"- {item.Formula.Output.Id}: {item.Calculation.VisibleOutput} " +
                $"[{item.Formula.Reference.Id} v{item.Formula.Reference.Version}] " +
                $"(crudo {item.Calculation.RawOutput})"));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string BuildGroupLabel(FormulaDefinition formula)
    {
        var id = formula.Reference.Id
            .StartsWith("formula-", StringComparison.Ordinal)
                ? formula.Reference.Id["formula-".Length..]
                : formula.Reference.Id;
        var suffixes = new[]
        {
            "dark-knight",
            "dark-wizard",
            "fairy-elf",
            "magic-gladiator",
            "dark-lord",
            "summoner",
        };
        foreach (var suffix in suffixes)
        {
            var marker = $"-{suffix}";
            if (id.EndsWith(marker, StringComparison.Ordinal))
            {
                return id[..^marker.Length];
            }
        }

        return id;
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
        FormulaContextErrorCodes.NoApplicableFormula =>
            "no hay una fórmula derivada publicada aplicable a esta clase y evolución.",
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

    private async void SaveBuildButtonClick(object sender, RoutedEventArgs e)
    {
        var buildId = BuildIdTextBox.Text.Trim();
        var draftId = BuildDraftIdTextBox.Text.Trim();
        if (!IsValidBuildDraftId(buildId) ||
            !IsValidBuildDraftId(draftId))
        {
            BuildResultTextBox.Text =
                "El ID de la build y el del borrador deben usar minúsculas, " +
                "números y guiones simples.";
            return;
        }

        try
        {
            var build = await _saveBuildUseCase.ExecuteAsync(
                new SaveBuildRequest(buildId, draftId),
                CancellationToken.None);
            BuildResultTextBox.Text =
                $"Build '{build.Id}' guardada. " +
                $"{build.Stats.Count} stats del snapshot exacto.";
            RefreshSavedBuilds();
        }
        catch (BuildException exception)
        {
            BuildResultTextBox.Text =
                $"No se pudo guardar ({exception.Code}): " +
                TranslateBuildError(exception.Code);
        }
        catch (BuildDraftException exception)
        {
            BuildResultTextBox.Text =
                $"No se pudo guardar el borrador ({exception.Code}): " +
                TranslateBuildDraftError(exception.Code);
        }
    }

    private async void LoadBuildButtonClick(object sender, RoutedEventArgs e)
    {
        var buildId = BuildIdTextBox.Text.Trim();
        if (!IsValidBuildDraftId(buildId))
        {
            BuildResultTextBox.Text =
                "El ID de la build debe usar minúsculas, números y guiones simples.";
            return;
        }

        await LoadBuildByIdAsync(buildId);
    }

    private async void SavedBuildLoadClick(object sender, RoutedEventArgs e)
    {
        if (SavedBuildsListBox.SelectedItem is not CharacterBuildSummary summary)
        {
            BuildResultTextBox.Text =
                "Selecciona una build guardada de la lista para cargarla.";
            return;
        }

        BuildIdTextBox.Text = summary.Id;
        await LoadBuildByIdAsync(summary.Id);
    }

    private async Task LoadBuildByIdAsync(string buildId)
    {
        try
        {
            var build = await _loadBuildUseCase.ExecuteAsync(
                buildId,
                CancellationToken.None);
            ApplyLoadedBuild(build);
            BuildResultTextBox.Text =
                $"Build '{build.Id}' cargada y aplicada al formulario. " +
                $"{build.Stats.Count} stats revalidados contra el snapshot exacto.";
        }
        catch (BuildException exception)
        {
            BuildResultTextBox.Text =
                $"No se pudo cargar ({exception.Code}): " +
                TranslateBuildError(exception.Code);
        }
        catch (BuildDraftException exception)
        {
            BuildResultTextBox.Text =
                $"No se pudo cargar el borrador fuente ({exception.Code}): " +
                TranslateBuildDraftError(exception.Code);
        }
        catch (StatDistributionException exception)
        {
            BuildResultTextBox.Text =
                $"No se pudo reaplicar ({exception.Code}): " +
                TranslateDistributionError(exception.Code);
        }
        catch (ProgressionPointBudgetException exception)
        {
            BuildResultTextBox.Text =
                $"No se pudo reaplicar ({exception.Code}): {exception.Message}";
        }
    }

    private void WindowLoaded(object sender, RoutedEventArgs e)
    {
        RefreshSavedBuilds();
    }

    private async void RefreshSavedBuilds()
    {
        try
        {
            var builds = await _listBuildsUseCase.ExecuteAsync(CancellationToken.None);
            SavedBuildsListBox.ItemsSource = builds;
            SavedBuildsStatusTextBox.Text = builds.Count == 1
                ? "1 build guardada."
                : $"{builds.Count} builds guardadas.";
        }
        catch (BuildException exception)
        {
            SavedBuildsStatusTextBox.Text =
                $"No se pudo listar ({exception.Code}): " +
                TranslateBuildError(exception.Code);
        }
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

    private static string TranslateBuildError(string code) => code switch
    {
        BuildErrorCodes.NotFound =>
            "no existe una build con ese ID.",
        BuildErrorCodes.SchemaUnsupported =>
            "la build usa una versión de esquema no soportada.",
        BuildErrorCodes.DependencyUnavailable =>
            "no está publicado exactamente el ruleset, dataset o motor del snapshot.",
        BuildErrorCodes.SourceMismatch =>
            "las identidades internas de la build no son coherentes.",
        BuildErrorCodes.RevalidationFailed =>
            "el recálculo no reproduce la caché persistida.",
        BuildErrorCodes.WriteConflict =>
            "la base local siguió ocupada después de los reintentos configurados.",
        _ => "se produjo un error de build no reconocido.",
    };
}
