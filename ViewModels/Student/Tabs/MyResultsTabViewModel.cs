using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.Data;
using SkyQuizApp.Dto;
using SkyQuizApp.Services.Interfaces;
using SkyQuizApp.Views.Student;

namespace SkyQuizApp.ViewModels.Student.Tabs
{
    public class MyResultsTabViewModel
    {
        private readonly AppDbContext _db;
        private readonly IUserSessionService _session;

        public ObservableCollection<StudentTestInfoDto> StudentTests { get; set; } = new();
        public ICommand ShowTestAttemptsCommand { get; }

        public MyResultsTabViewModel()
        {
            _db = (App.Current as App)!.Services.GetRequiredService<AppDbContext>();
            _session = (App.Current as App)!.Services.GetRequiredService<IUserSessionService>();

            ShowTestAttemptsCommand = new RelayCommand(ShowTestAttempts);

            LoadStudentTests();
        }

        public MyResultsTabViewModel(AppDbContext db, IUserSessionService session)
        {
            _db = db;
            _session = session;

            ShowTestAttemptsCommand = new RelayCommand(ShowTestAttempts);

            LoadStudentTests();
        }

        public void LoadStudentTests()
        {
            StudentTests.Clear();

            var userId = _session.CurrentUser!.UserID;

            var sessions = _db.TestSessions
                .Where(s => s.UserID == userId)
                .Include(s => s.Test)
                .Include(s => s.Result)
                .AsNoTracking()
                .ToList();

            var tests = sessions
                .GroupBy(s => s.TestID)
                .Select(g => new StudentTestInfoDto
                {
                    TestID = g.Key,
                    TestTitle = g.First().Test!.Title,
                    AttemptsCount = g.Count(),
                    AverageScore = g.Any(s => s.Result != null)
                        ? (double)g.Where(s => s.Result != null).Average(s => s.Result!.Score)
                        : 0.0
                });

            foreach (var dto in tests)
                StudentTests.Add(dto);
        }

        private void ShowTestAttempts(object obj)
        {
            if (obj is int testId)
            {
                var window = new TestAttemptsWindow(testId);
                window.ShowDialog();

                LoadStudentTests();
            }
        }
    }
}