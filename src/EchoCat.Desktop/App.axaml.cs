using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EchoCat.AI;
using EchoCat.Core;
using EchoCat.Desktop.Services;
using EchoCat.Desktop.Skins;
using EchoCat.Desktop.ViewModels;
using EchoCat.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace EchoCat.Desktop;

public partial class App : Application
{
    private ServiceProvider? _services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddSingleton<PetStateMachine>();
        services.AddSingleton<PetSkinCatalog>();
        services.AddSingleton<ILocalSettingsService, SqliteLocalSettingsService>();
        services.AddSingleton<IAiCompanionService, SettingsBackedAiCompanionService>();
        services.AddSingleton<PetViewModel>();
        services.AddSingleton<PetWindow>();
        services.AddSingleton<ITrayService, TrayService>();
        _services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            desktop.MainWindow = _services.GetRequiredService<PetWindow>();
            _services.GetRequiredService<ITrayService>().Initialize();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
