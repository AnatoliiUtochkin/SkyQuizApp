using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.Data;
using SkyQuizApp.Enums;
using SkyQuizApp.Services.Interfaces;
using SkyQuizApp.Views.Student;

namespace SkyQuizApp.ViewModels.Student
{
    public class TestAttemptsViewModel
    {
        private readonly AppDbContext _db;
        private readonly IServiceProvider _services;
        private readonly ILogService _logger;
        private readonly IUserSessionService _session;

        public ObservableCollection<AttemptDto> Attempts { get; set; } = new();
        public ICommand ReviewAttemptCommand { get; }

        public TestAttemptsViewModel(int testId)
        {
            var app = App.Current as App ?? throw new InvalidOperationException("App is null");

            _db = app.Services.GetRequiredService<AppDbContext>();
            _services = app.Services;
            _logger = app.Services.GetRequiredService<ILogService>();
            _session = app.Services.GetRequiredService<IUserSessionService>();

            ReviewAttemptCommand = new RelayCommand(Review);

            LoadAttempts(testId);
        }

        public TestAttemptsViewModel(int testId, IServiceProvider services)
        {
            _services = services;
            _db = _services.GetRequiredService<AppDbContext>();
            _logger = _services.GetRequiredService<ILogService>();
            _session = _services.GetRequiredService<IUserSessionService>();
            ReviewAttemptCommand = new RelayCommand(Review);
            LoadAttempts(testId);
        }


        private void LoadAttempts(int testId)
        {
            var currentUserId = _session.CurrentUser!.UserID;

            var data = _db.TestSessions
                .Where(ts => ts.TestID == testId && ts.UserID == currentUserId)
                .Include(ts => ts.Result)
                .OrderByDescending(ts => ts.StartedAt)
                .AsNoTracking()
                .ToList();

            Attempts.Clear();

            foreach (var s in data)
            {
                Attempts.Add(new AttemptDto
                {
                    TestSessionID = s.TestSessionID,
                    AttemptDate = s.StartedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
                    Score = s.Result != null ? (double)s.Result.Score : 0
                });
            }
        }


        private void Review(object obj)
        {
            if (obj is int sessionId)
            {
                var window = new ReviewAttemptWindow(sessionId);
                window.ShowDialog();

                if (_session.CurrentUser != null)
                {
                    _logger.Log(LogAction.ResultViewed, $"Переглянуто спробу тесту (SessionID={sessionId})", _session.CurrentUser.UserID);
                }
            }
        }
    }

    public class AttemptDto
    {
        public int TestSessionID { get; set; }
        public string AttemptDate { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}