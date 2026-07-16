using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia;
using EchoCat.AI;
using EchoCat.Core;
using EchoCat.Desktop.Services;
using EchoCat.Desktop.Skins;
using EchoCat.Desktop.ViewModels;

namespace EchoCat.Desktop.Views;

public partial class SettingsWindow : Window
{
    private readonly PetViewModel _viewModel;
    private readonly ILocalSettingsService _settings;
    private bool _isUpdating;

    public SettingsWindow()
        : this(new PetViewModel(
            new MockAiCompanionService(),
            new SqliteLocalSettingsService(),
            new PetSkinCatalog(),
            new PetStateMachine()),
            new SqliteLocalSettingsService())
    {
    }

    public SettingsWindow(PetViewModel viewModel, ILocalSettingsService settings)
    {
        _viewModel = viewModel;
        _settings = settings;
        InitializeComponent();
        TopmostCheckBox.PropertyChanged += TopmostCheckBox_OnPropertyChanged;
        WelcomeBubbleCheckBox.PropertyChanged += StartupCheckBox_OnPropertyChanged;
        StartHiddenCheckBox.PropertyChanged += StartupCheckBox_OnPropertyChanged;
        BuildSkinList();
        _ = RestoreSettingsAsync();
    }

    public event EventHandler<bool>? TopmostChanged;

    public void SetTopmostState(bool isTopmost)
    {
        _isUpdating = true;
        TopmostCheckBox.IsChecked = isTopmost;
        _isUpdating = false;
    }

    private void BuildSkinList()
    {
        SkinList.Children.Clear();

        foreach (var skin in _viewModel.SkinOptions)
        {
            var button = new Button
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                Padding = new Avalonia.Thickness(12, 8),
                Content = FormatSkinLabel(skin)
            };

            button.Click += (_, _) =>
            {
                if (_viewModel.SwitchSkinCommand.CanExecute(skin.Id))
                {
                    _viewModel.SwitchSkinCommand.Execute(skin.Id);
                    BuildSkinList();
                }
            };

            SkinList.Children.Add(button);
        }
    }

    private string FormatSkinLabel(PetSkinOption skin)
    {
        return skin.Id == _viewModel.ActiveSkinId
            ? $"* {skin.DisplayName}"
            : $"  {skin.DisplayName}";
    }

    private async Task RestoreSettingsAsync()
    {
        var settings = await _settings.LoadAsync();
        _isUpdating = true;
        TopmostCheckBox.IsChecked = settings.IsTopmost;
        WelcomeBubbleCheckBox.IsChecked = settings.ShowWelcomeBubble;
        StartHiddenCheckBox.IsChecked = settings.StartHidden;
        RestoreAiSettings(settings.Ai ?? new AiSettings());
        _isUpdating = false;
    }

    private void TopmostCheckBox_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_isUpdating || e.Property != ToggleButton.IsCheckedProperty)
        {
            return;
        }

        TopmostChanged?.Invoke(this, TopmostCheckBox.IsChecked == true);
    }

    private async void StartupCheckBox_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_isUpdating || e.Property != ToggleButton.IsCheckedProperty)
        {
            return;
        }

        var settings = await _settings.LoadAsync();
        await _settings.SaveAsync(settings with
        {
            ShowWelcomeBubble = WelcomeBubbleCheckBox.IsChecked == true,
            StartHidden = StartHiddenCheckBox.IsChecked == true
        });
    }

    private async void SaveAiButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var ai = new AiSettings(
            Provider: GetSelectedProvider(),
            Endpoint: AiEndpointBox.Text?.Trim() ?? string.Empty,
            Model: AiModelBox.Text?.Trim() ?? string.Empty,
            ApiKey: AiApiKeyBox.Text?.Trim() ?? string.Empty,
            Instructions: AiInstructionsBox.Text?.Trim() ?? string.Empty);

        if (ai.Provider == AiProviderIds.OpenAi && !IsOpenAiComplete(ai))
        {
            AiSaveStatus.Text = "OpenAI 需要完整填写 endpoint、model 和 API Key。";
            return;
        }

        var settings = await _settings.LoadAsync();
        await _settings.SaveAsync(settings with
        {
            Ai = ai
        });

        AiSaveStatus.Text = "已保存，下一条消息生效。";
    }

    private void RestoreAiSettings(AiSettings ai)
    {
        SetSelectedProvider(ai.Provider);
        AiEndpointBox.Text = ai.Endpoint;
        AiModelBox.Text = ai.Model;
        AiApiKeyBox.Text = ai.ApiKey;
        AiInstructionsBox.Text = string.IsNullOrWhiteSpace(ai.Instructions)
            ? AiSettingsDefaults.Instructions
            : ai.Instructions;
        AiSaveStatus.Text = string.Empty;
    }

    private string GetSelectedProvider()
    {
        return (AiProviderBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? AiProviderIds.Mock;
    }

    private void SetSelectedProvider(string? provider)
    {
        var normalized = string.IsNullOrWhiteSpace(provider)
            ? AiProviderIds.Mock
            : provider.Trim().ToLowerInvariant();

        foreach (var item in AiProviderBox.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(item.Tag?.ToString(), normalized, StringComparison.OrdinalIgnoreCase))
            {
                AiProviderBox.SelectedItem = item;
                return;
            }
        }

        AiProviderBox.SelectedIndex = 0;
    }

    private static bool IsOpenAiComplete(AiSettings ai)
    {
        return !string.IsNullOrWhiteSpace(ai.Endpoint)
            && !string.IsNullOrWhiteSpace(ai.Model)
            && !string.IsNullOrWhiteSpace(ai.ApiKey);
    }
}
