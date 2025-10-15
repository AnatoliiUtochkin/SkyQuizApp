using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.Models;
using SkyQuizApp.Services;
using SkyQuizApp.Services.Interfaces;
using SkyQuizApp.Views.Admin.Tabs;

namespace SkyQuizApp.ViewModels.Admin
{
    public class AdminMainViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _services;
        private readonly IUserSessionService _session;

        private object? _currentView;

        public object? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public User? CurrentUser => _session.CurrentUser;

        public ICommand ShowUsersViewCommand { get; }
        public ICommand ShowTestsViewCommand { get; }
        public ICommand ShowQuestionsViewCommand { get; }
        public ICommand ShowTestSessionsViewCommand { get; }
        public ICommand ShowAnswersViewCommand { get; }
        public ICommand ShowResultsViewCommand { get; }
        public ICommand ShowLogsViewCommand { get; }
        public ICommand ShowTwoFactorCodesViewCommand { get; }
        public ICommand ShowSettingsViewCommand { get; }

        public AdminMainViewModel(IServiceProvider services, IUserSessionService session)
        {
            _services = services;
            _session = session;

            ShowUsersViewCommand = new RelayCommand(_ => CurrentView = _services.GetRequiredService<UsersTab>());
            ShowTestsViewCommand = new RelayCommand(_ => CurrentView = _services.GetRequiredService<TestsTab>());
            ShowQuestionsViewCommand = new RelayCommand(_ => CurrentView = _services.GetRequiredService<QuestionsTab>());
            ShowTestSessionsViewCommand = new RelayCommand(_ => CurrentView = _services.GetRequiredService<TestSessionsTab>());
            ShowAnswersViewCommand = new RelayCommand(_ => CurrentView = _services.GetRequiredService<AnswersTab>());
            ShowResultsViewCommand = new RelayCommand(_ => CurrentView = _services.GetRequiredService<ResultsTab>());
            ShowLogsViewCommand = new RelayCommand(_ => CurrentView = _services.GetRequiredService<LogsTab>());
            ShowTwoFactorCodesViewCommand = new RelayCommand(_ => CurrentView = _services.GetRequiredService<TwoFactorCodesTab>());
            ShowSettingsViewCommand = new RelayCommand(_ => CurrentView = _services.GetRequiredService<AdminSettingsTab>());
        }

        public void HandleDrag(Window window, MouseButtonEventArgs e)
            => WindowHelper.HandleDrag(window, e);

        public void HandleMinimize(Window window)
            => WindowHelper.HandleMinimize(window);

        public void HandleClose(Window window)
            => Application.Current.Shutdown();

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}