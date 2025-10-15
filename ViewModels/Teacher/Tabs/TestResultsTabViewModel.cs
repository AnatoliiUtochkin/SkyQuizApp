using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.Models;
using SkyQuizApp.Views.Teacher;

namespace SkyQuizApp.ViewModels.Teacher.Tabs
{
    public class TestResultsTabViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _services;

        public ObservableCollection<Test> Tests { get; set; } = new();

        private Test? _selectedTest;

        public Test? SelectedTest
        {
            get => _selectedTest;
            set { _selectedTest = value; OnPropertyChanged(); }
        }

        public ICommand ViewResultsCommand { get; }

        public TestResultsTabViewModel(IServiceProvider services)
        {
            _services = services;
            ViewResultsCommand = new RelayCommand(t => OpenResults(t as Test));
            LoadTests();
        }

        private void LoadTests()
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
            var session = _services.GetRequiredService<Services.Interfaces.IUserSessionService>();

            if (session.CurrentUser == null)
                throw new InvalidOperationException("Користувач не залогінений.");

            int userId = session.CurrentUser.UserID;

            var tests = db.Tests
                          .Where(t => t.UserID == userId)
                          .OrderByDescending(t => t.CreatedAt)
                          .AsNoTracking()
                          .ToList();

            Tests.Clear();
            foreach (var test in tests)
                Tests.Add(test);
        }

        private void OpenResults(Test? test)
        {
            if (test == null)
                return;

            var window = new TestResultsWindow(test.TestID);
            window.ShowDialog();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}