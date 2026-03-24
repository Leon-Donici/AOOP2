using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using LibraryManagementApp.Services;
using LibraryManagementApp.ViewModels;
using LibraryManagementApp.Views;

namespace LibraryManagementApp;

public partial class App : Application
{
    private DataService? _dataService;
    private Models.LibraryData? _data;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            // Load data
            _dataService = new DataService();
            _data = _dataService.Load();

            // Create services
            var libraryService = new LibraryService(_data);

            // Create main view model
            var mainVm = new MainWindowViewModel(libraryService, _dataService, _data);

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainVm,
            };

            // Save data on shutdown
            desktop.ShutdownRequested += (_, _) =>
            {
                if (_data != null && _dataService != null)
                    _dataService.Save(_data);
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
