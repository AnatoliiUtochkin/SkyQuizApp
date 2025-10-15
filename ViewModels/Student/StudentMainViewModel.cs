using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.Data;
using SkyQuizApp.Models;
using SkyQuizApp.Services.Interfaces;
using SkyQuizApp.ViewModels.Student.Tabs;
using SkyQuizApp.Views.Student.Tabs;

public class StudentMainViewModel : INotifyPropertyChanged
{
    private readonly IServiceProvider _services;
    private readonly IUserSessionService _session;
    private readonly AppDbContext _dbContext;

    private object? _currentView;

    public object? CurrentView
    {
        get => _currentView;
        set { _currentView = value; OnPropertyChanged(); }
    }

    public User? CurrentUser => _session.CurrentUser;

    public ICommand ShowJoinTestViewCommand { get; }
    public ICommand ShowMyResultsViewCommand { get; }
    public ICommand ShowStatisticsViewCommand { get; }
    public ICommand ShowSettingsViewCommand { get; }

    private readonly JoinTestTab _joinTestTab;

    private readonly MyResultsTab _myResultsTab;
    private readonly StatisticsTab _statisticsTab;
    private readonly StudentSettingsTab _studentSettingsTab;

    public StudentMainViewModel(IServiceProvider services, IUserSessionService session, AppDbContext dbContext)
    {
        _services = services;
        _session = session;
        _dbContext = dbContext;

        _joinTestTab = _services.GetRequiredService<JoinTestTab>();
        _myResultsTab = _services.GetRequiredService<MyResultsTab>();
        _studentSettingsTab = _services.GetRequiredService<StudentSettingsTab>();

        if (_session.CurrentUser is not null)
        {
            _statisticsTab = new StatisticsTab(_dbContext, _session.CurrentUser.UserID);
        }

        ShowJoinTestViewCommand = new RelayCommand(_ => CurrentView = _joinTestTab);
        ShowMyResultsViewCommand = new RelayCommand(_ => CurrentView = _myResultsTab);
        ShowMyResultsViewCommand = new RelayCommand(_ =>
        {
            CurrentView = _myResultsTab;
            if (_myResultsTab.DataContext is MyResultsTabViewModel vm)
                vm.LoadStudentTests();
        });
        ShowStatisticsViewCommand = new RelayCommand(_ => CurrentView = _statisticsTab);
        ShowSettingsViewCommand = new RelayCommand(_ => CurrentView = _studentSettingsTab);
    }

    public void HandleDrag(Window window, MouseButtonEventArgs e)
        => SkyQuizApp.Services.WindowHelper.HandleDrag(window, e);

    public void HandleMinimize(Window window)
        => SkyQuizApp.Services.WindowHelper.HandleMinimize(window);

    public void HandleClose(Window window)
        => Application.Current.Shutdown();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}