using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia;
using Avalonia.Threading;
using System.ComponentModel;
using EchoCat.Desktop.Services;
using EchoCat.Desktop.ViewModels;

namespace EchoCat.Desktop.Views;

public partial class PetWindow : Window
{
    private readonly DispatcherTimer _savePlacementTimer;
    private ILocalSettingsService? _settings;
    private MenuItem? _skinMenu;
    private MenuItem? _topmostMenuItem;
    private SettingsWindow? _settingsWindow;
    private bool _ignoreNextTap;
    private bool _startHiddenApplied;

    public PetWindow()
    {
        InitializeComponent();
        _savePlacementTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(450)
        };
        _savePlacementTimer.Tick += async (_, _) =>
        {
            _savePlacementTimer.Stop();
            await SavePlacementAsync();
        };
        DataContextChanged += OnDataContextChanged;
        Opened += async (_, _) => await RestoreWindowSettingsAsync();
        Closing += async (_, _) => await SavePlacementAsync();
        PositionChanged += (_, _) =>
        {
            SchedulePlacementSave();
            UpdateEdgeAwareness();
        };
    }

    public PetWindow(PetViewModel viewModel, ILocalSettingsService settings)
        : this()
    {
        _settings = settings;
        DataContext = viewModel;
    }

    private void Window_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source == this)
        {
            BeginPotentialDrag(e);
        }
    }

    private void PetSurface_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginPotentialDrag(e);
    }

    private void PetSurface_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not PetViewModel viewModel || PetSurface.Bounds.Width <= 0 || PetSurface.Bounds.Height <= 0)
        {
            return;
        }

        var position = e.GetPosition(PetSurface);
        var lookX = (position.X / PetSurface.Bounds.Width - 0.5) * 2;
        var lookY = (position.Y / PetSurface.Bounds.Height - 0.5) * 2;
        viewModel.LookAt(lookX, lookY);
    }

    private void PetSurface_OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is PetViewModel viewModel)
        {
            viewModel.ResetLook();
        }
    }

    private void PetSurface_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is PetViewModel viewModel && viewModel.OpenChatCommand.CanExecute(null))
        {
            viewModel.OpenChatCommand.Execute(null);
            FocusPromptSoon();
        }
    }

    private void PetSurface_OnTapped(object? sender, TappedEventArgs e)
    {
        if (_ignoreNextTap)
        {
            _ignoreNextTap = false;
            return;
        }

        if (DataContext is PetViewModel viewModel && viewModel.PetCommand.CanExecute(null))
        {
            viewModel.PetCommand.Execute(null);
        }
    }

    private void PromptBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not PetViewModel viewModel)
        {
            return;
        }

        if (e.Key == Key.Enter && viewModel.AskCommand.CanExecute(null))
        {
            viewModel.AskCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && viewModel.CloseChatCommand.CanExecute(null))
        {
            viewModel.CloseChatCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void BeginPotentialDrag(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _ignoreNextTap = true;

            if (DataContext is PetViewModel viewModel && viewModel.DraggingCommand.CanExecute(null))
            {
                viewModel.DraggingCommand.Execute(null);
            }

            BeginMoveDrag(e);
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is INotifyPropertyChanged changed)
        {
            changed.PropertyChanged += OnViewModelPropertyChanged;
        }

        BuildContextMenu();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PetViewModel.IsChatOpen)
            && DataContext is PetViewModel { IsChatOpen: true })
        {
            FocusPromptSoon();
        }

        if (e.PropertyName == nameof(PetViewModel.ActiveSkinId))
        {
            RefreshSkinMenuChecks();
        }
    }

    private void FocusPromptSoon()
    {
        Dispatcher.UIThread.Post(() => PromptBox.Focus());
    }

    private async Task RestoreWindowSettingsAsync()
    {
        if (_settings is null)
        {
            return;
        }

        var settings = await _settings.LoadAsync();
        Topmost = settings.IsTopmost;
        RefreshTopmostMenu();

        if (settings.WindowPlacement is not null)
        {
            Position = new PixelPoint(settings.WindowPlacement.X, settings.WindowPlacement.Y);
        }

        UpdateEdgeAwareness();

        if (settings.StartHidden && !_startHiddenApplied)
        {
            _startHiddenApplied = true;
            Dispatcher.UIThread.Post(Hide, DispatcherPriority.Background);
        }
    }

    private async Task SavePlacementAsync()
    {
        if (_settings is null)
        {
            return;
        }

        await _settings.SavePlacementAsync(new PetWindowPlacement(Position.X, Position.Y));
    }

    private void SchedulePlacementSave()
    {
        _savePlacementTimer.Stop();
        _savePlacementTimer.Start();
    }

    private void UpdateEdgeAwareness()
    {
        if (DataContext is not PetViewModel viewModel)
        {
            return;
        }

        var screen = Screens.ScreenFromWindow(this);
        if (screen is null)
        {
            return;
        }

        var workingArea = screen.WorkingArea;
        var scale = RenderScaling;
        var width = (int)Math.Round(Bounds.Width * scale);
        var height = (int)Math.Round(Bounds.Height * scale);
        var threshold = (int)Math.Round(26 * scale);

        var nearEdge = Position.X <= workingArea.X + threshold
            || Position.Y <= workingArea.Y + threshold
            || Position.X + width >= workingArea.Right - threshold
            || Position.Y + height >= workingArea.Bottom - threshold;

        viewModel.SetEdgeProximity(nearEdge);
    }

    private void BuildContextMenu()
    {
        if (DataContext is not PetViewModel viewModel)
        {
            return;
        }

        _skinMenu = new MenuItem
        {
            Header = "皮肤"
        };

        var skinItems = new List<MenuItem>();
        foreach (var skin in viewModel.SkinOptions)
        {
            var item = new MenuItem
            {
                Header = FormatSkinHeader(skin, viewModel.ActiveSkinId)
            };

            item.Click += (_, _) =>
            {
                if (viewModel.SwitchSkinCommand.CanExecute(skin.Id))
                {
                    viewModel.SwitchSkinCommand.Execute(skin.Id);
                }
            };

            skinItems.Add(item);
        }

        _skinMenu.ItemsSource = skinItems;
        _topmostMenuItem = new MenuItem
        {
            Header = Topmost ? "* 置顶" : "  置顶"
        };
        _topmostMenuItem.Click += (_, _) =>
        {
            SetTopmost(!Topmost);
        };

        var hideItem = new MenuItem
        {
            Header = "隐藏"
        };
        hideItem.Click += (_, _) => Hide();

        var settingsItem = new MenuItem
        {
            Header = "设置"
        };
        settingsItem.Click += (_, _) => ShowSettingsWindow(viewModel);

        var exitItem = new MenuItem
        {
            Header = "退出"
        };
        exitItem.Click += (_, _) =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
            else
            {
                Close();
            }
        };

        PetSurface.ContextMenu = new ContextMenu
        {
            ItemsSource = new object[]
            {
                _skinMenu,
                _topmostMenuItem,
                hideItem,
                settingsItem,
                new Separator(),
                exitItem
            }
        };
    }

    private void RefreshSkinMenuChecks()
    {
        if (_skinMenu?.ItemsSource is not IEnumerable<MenuItem> items
            || DataContext is not PetViewModel viewModel)
        {
            return;
        }

        foreach (var item in items)
        {
            var option = viewModel.SkinOptions.FirstOrDefault(skin =>
                item.Header is string header && header.Contains(skin.DisplayName, StringComparison.Ordinal));
            item.Header = option is null
                ? item.Header
                : FormatSkinHeader(option, viewModel.ActiveSkinId);
        }
    }

    private static string FormatSkinHeader(PetSkinOption skin, string activeSkinId)
    {
        return skin.Id == activeSkinId
            ? $"* {skin.DisplayName}"
            : $"  {skin.DisplayName}";
    }

    private void RefreshTopmostMenu()
    {
        if (_topmostMenuItem is not null)
        {
            _topmostMenuItem.Header = Topmost ? "* 置顶" : "  置顶";
        }

        _settingsWindow?.SetTopmostState(Topmost);
    }

    private void ShowSettingsWindow(PetViewModel viewModel)
    {
        if (_settingsWindow is null || !_settingsWindow.IsVisible)
        {
            _settingsWindow = new SettingsWindow(viewModel, _settings ?? new SqliteLocalSettingsService());
            _settingsWindow.TopmostChanged += (_, isTopmost) => SetTopmost(isTopmost);
        }

        _settingsWindow.SetTopmostState(Topmost);
        _settingsWindow.Show(this);
        _settingsWindow.Activate();
    }

    private void SetTopmost(bool isTopmost)
    {
        Topmost = isTopmost;
        RefreshTopmostMenu();
        _ = _settings?.SaveTopmostAsync(Topmost);
    }
}
