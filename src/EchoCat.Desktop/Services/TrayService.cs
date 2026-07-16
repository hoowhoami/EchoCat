using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using EchoCat.Desktop.Views;

namespace EchoCat.Desktop.Services;

public sealed class TrayService : ITrayService
{
    private readonly PetWindow _petWindow;
    private TrayIcon? _trayIcon;

    public TrayService(PetWindow petWindow)
    {
        _petWindow = petWindow;
    }

    public void Initialize()
    {
        if (_trayIcon is not null)
        {
            return;
        }

        var showItem = new NativeMenuItem("显示 EchoCat");
        showItem.Click += (_, _) => ShowPet();

        var hideItem = new NativeMenuItem("隐藏 EchoCat");
        hideItem.Click += (_, _) => _petWindow.Hide();

        var exitItem = new NativeMenuItem("退出");
        exitItem.Click += (_, _) => Shutdown();

        var menu = new NativeMenu
        {
            Items =
            {
                showItem,
                hideItem,
                new NativeMenuItemSeparator(),
                exitItem
            }
        };

        _trayIcon = new TrayIcon
        {
            ToolTipText = "EchoCat",
            IsVisible = true,
            Menu = menu
        };
        _trayIcon.Clicked += (_, _) => ShowPet();
    }

    public void Dispose()
    {
        _trayIcon?.Dispose();
        _trayIcon = null;
    }

    private void ShowPet()
    {
        _petWindow.Show();
        _petWindow.Activate();
    }

    private void Shutdown()
    {
        Dispose();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
        else
        {
            _petWindow.Close();
        }
    }
}
