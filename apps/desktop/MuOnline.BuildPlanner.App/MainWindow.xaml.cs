using System.Windows;
using System.Windows.Controls;
using MuOnline.BuildPlanner.Application.Progression;
using MuOnline.BuildPlanner.Domain.Progression;

namespace MuOnline.BuildPlanner.App;

public partial class MainWindow : Window
{
    private readonly ProgressionRulesetCatalog _catalog;
    private readonly CalculateProgressionPointBudgetUseCase _useCase;

    public MainWindow()
    {
        InitializeComponent();
        _catalog = PublishedProgressionRuleset.Catalog;
        _useCase = PublishedProgressionRuleset.CreateUseCase();

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
            return;
        }

        EvolutionComboBox.ItemsSource = selectedClass.Evolutions;
        EvolutionComboBox.SelectedIndex = 0;
        UpdateHeroStatusAvailability();
    }

    private void EvolutionSelectionChanged(object sender, SelectionChangedEventArgs e) =>
        UpdateHeroStatusAvailability();

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
            ResultTextBox.Text = FormatResult(result);
        }
        catch (ProgressionPointBudgetException exception)
        {
            ResultTextBox.Text = $"No se pudo calcular ({exception.Code}): {exception.Message}";
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
}
