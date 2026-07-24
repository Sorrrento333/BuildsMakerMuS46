using System.Windows;
using System.Windows.Controls;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Application.Stats;
using MuOnline.BuildPlanner.Domain.Progression;
using MuOnline.BuildPlanner.Domain.Stats;

namespace MuOnline.BuildPlanner.App;

public partial class MainWindow : Window
{
    private readonly ProgressionRulesetCatalog _catalog;
    private readonly CalculateProgressionPointBudgetUseCase _useCase;
    private readonly CalculateStatDistributionUseCase _statDistributionUseCase;
    private readonly Dictionary<string, TextBox> _allocationInputs =
        new(StringComparer.Ordinal);
    private ProgressionPointBudgetResult? _currentBudget;

    public MainWindow()
    {
        InitializeComponent();
        _catalog = PublishedProgressionRuleset.Catalog;
        _useCase = PublishedProgressionRuleset.CreateUseCase();
        _statDistributionUseCase = PublishedProgressionRuleset.CreateStatDistributionUseCase();

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
            var result = _useCase.Execute(new ProgressionPointBudgetRequest(
                selectedClass.Id,
                selectedEvolution.Id,
                level,
                completedQuestIds));
            _currentBudget = result;
            DistributeStatsButton.IsEnabled = true;
            DistributionResultTextBox.Text =
                "Presupuesto calculado. Ingresa las asignaciones y distribuye los puntos.";
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
            var result = _statDistributionUseCase.Execute(_currentBudget, allocations);
            DistributionResultTextBox.Text = FormatDistributionResult(result);
        }
        catch (StatDistributionException exception)
        {
            DistributionResultTextBox.Text =
                $"No se pudo distribuir ({exception.Code}): " +
                TranslateDistributionError(exception.Code);
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
        }
    }

    private void InvalidateCurrentBudget()
    {
        _currentBudget = null;
        if (DistributeStatsButton is not null)
        {
            DistributeStatsButton.IsEnabled = false;
        }

        DistributionResultTextBox?.Clear();
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
        _ => "se produjo un error de distribución no reconocido.",
    };
}
