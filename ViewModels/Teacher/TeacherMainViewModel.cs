using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.Services.Interfaces;
using SkyQuizApp.Views.Teacher.Tabs;

namespace SkyQuizApp.ViewModels.Teacher
{
    public class TeacherMainViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _services;
        private readonly IUserSessionService _session;

        private object? _currentView;

        public object? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand ShowMyTestsViewCommand { get; }
        public ICommand ShowTestResultsViewCommand { get; }
        public ICommand ShowActivityLogViewCommand { get; }
        public ICommand ShowSettingsViewCommand { get; }

        public TeacherMainViewModel(IServiceProvider services, IUserSessionService session)
        {
            _services = services;
            _session = session;

            ShowMyTestsViewCommand = new RelayCommand(_ => CurrentView = _services.GetRequiredService<MyTestsTab>());
            ShowTestResultsViewCommand = new RelayCommand(_ => CurrentView = _services.GetRequiredService<TestResultsTab>());
            ShowActivityLogViewCommand = new RelayCommand(_ => CurrentView = _services.GetRequiredService<ActivityLogTab>());
            ShowSettingsViewCommand = new RelayCommand(_ => CurrentView = _services.GetRequiredService<SettingsTab>());
        }

        public void HandleDrag(Window window, MouseButtonEventArgs e)
            => Services.WindowHelper.HandleDrag(window, e);

        public void HandleMinimize(Window window)
            => Services.WindowHelper.HandleMinimize(window);

        public void HandleClose(Window window)
            => Application.Current.Shutdown();

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}