using System.Collections.ObjectModel;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Data;
using SkyQuizApp.Enums;
using SkyQuizApp.Services.Interfaces;

namespace SkyQuizApp.ViewModels.Student
{
    public class ReviewAttemptViewModel
    {
        private readonly AppDbContext _db;
        private readonly ILogService _logger;
        private readonly IUserSessionService _session;

        public ObservableCollection<ReviewQuestionDto> Questions { get; set; } = new();

        public ReviewAttemptViewModel(int sessionId, AppDbContext db, ILogService logger, IUserSessionService session)
        {
            _db = db;
            _logger = logger;
            _session = session;

            var answers = _db.UserAnswers
                .Where(ua => ua.TestSessionID == sessionId)
                .Include(ua => ua.Answer)
                .Include(ua => ua.Question)
                    .ThenInclude(q => q.Answers)
                .AsNoTracking()
                .ToList();

            var grouped = answers.GroupBy(a => a.QuestionID);

            foreach (var group in grouped)
            {
                var question = group.First().Question!;
                var userAnswers = group
                    .Where(a => a.Answer != null)
                    .Select(a => a.Answer!.Text)
                    .ToList();

                var correctAnswers = question.Answers
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Text)
                    .ToList();

                string status = userAnswers.SetEquals(correctAnswers)
                    ? "correct"
                    : (userAnswers.Intersect(correctAnswers).Any() ? "partial" : "wrong");

                var color = status switch
                {
                    "correct" => new SolidColorBrush(Color.FromArgb(0x88, 0x2E, 0xCC, 0x71)),
                    "partial" => new SolidColorBrush(Color.FromArgb(0x88, 0xF1, 0xC4, 0x0F)),
                    "wrong" => new SolidColorBrush(Color.FromArgb(0x88, 0xE7, 0x4C, 0x3C)),
                    _ => new SolidColorBrush(Color.FromArgb(0x88, 0x44, 0x44, 0x44))
                };

                var foreground = status switch
                {
                    "correct" => new SolidColorBrush(Color.FromRgb(239, 255, 255)),
                    "partial" => new SolidColorBrush(Color.FromRgb(255, 248, 220)),
                    "wrong" => new SolidColorBrush(Color.FromRgb(255, 239, 239)),
                    _ => Brushes.White
                };

                Questions.Add(new ReviewQuestionDto
                {
                    Text = question.Text,
                    UserAnswer = "Ваша відповідь: " + string.Join(", ", userAnswers),
                    CorrectAnswer = "Правильна відповідь: " + string.Join(", ", correctAnswers),
                    BackgroundColor = color,
                    ForegroundColor = foreground
                });
            }

            if (_session.CurrentUser != null)
            {
                _logger.Log(LogAction.ResultViewed, $"Студент переглянув спробу тесту (SessionID={sessionId})", _session.CurrentUser.UserID);
            }
        }

        public ReviewAttemptViewModel(int sessionId)
        {
            var app = App.Current as App
                ?? throw new InvalidOperationException("App is null");

            _db = app.Services.GetRequiredService<AppDbContext>();
            _logger = app.Services.GetRequiredService<ILogService>();
            _session = app.Services.GetRequiredService<IUserSessionService>();

            var answers = _db.UserAnswers
                .Where(ua => ua.TestSessionID == sessionId)
                .Include(ua => ua.Answer)
                .Include(ua => ua.Question)
                    .ThenInclude(q => q.Answers)
                .AsNoTracking()
                .ToList();

            var grouped = answers.GroupBy(a => a.QuestionID);

            foreach (var group in grouped)
            {
                var question = group.First().Question!;
                var userAnswers = group
                    .Where(a => a.Answer != null)
                    .Select(a => a.Answer!.Text)
                    .ToList();

                var correctAnswers = question.Answers
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Text)
                    .ToList();

                string status = userAnswers.SetEquals(correctAnswers)
                    ? "correct"
                    : (userAnswers.Intersect(correctAnswers).Any() ? "partial" : "wrong");

                var color = status switch
                {
                    "correct" => new SolidColorBrush(Color.FromArgb(0x88, 0x2E, 0xCC, 0x71)),
                    "partial" => new SolidColorBrush(Color.FromArgb(0x88, 0xF1, 0xC4, 0x0F)),
                    "wrong" => new SolidColorBrush(Color.FromArgb(0x88, 0xE7, 0x4C, 0x3C)),
                    _ => new SolidColorBrush(Color.FromArgb(0x88, 0x44, 0x44, 0x44))
                };

                var foreground = status switch
                {
                    "correct" => new SolidColorBrush(Color.FromRgb(239, 255, 255)),
                    "partial" => new SolidColorBrush(Color.FromRgb(255, 248, 220)),
                    "wrong" => new SolidColorBrush(Color.FromRgb(255, 239, 239)),
                    _ => Brushes.White
                };

                Questions.Add(new ReviewQuestionDto
                {
                    Text = question.Text,
                    UserAnswer = "Ваша відповідь: " + string.Join(", ", userAnswers),
                    CorrectAnswer = "Правильна відповідь: " + string.Join(", ", correctAnswers),
                    BackgroundColor = color,
                    ForegroundColor = foreground
                });
            }

            if (_session.CurrentUser != null)
            {
                _logger.Log(LogAction.ResultViewed, $"Студент переглянув спробу тесту (SessionID={sessionId})", _session.CurrentUser.UserID);
            }
        }
    }

    public class ReviewQuestionDto
    {
        public string Text { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public Brush ForegroundColor { get; set; } = Brushes.White;
        public Brush BackgroundColor { get; set; } = Brushes.Gray;
    }

    internal static class Extensions
    {
        public static bool SetEquals<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            return new HashSet<T>(source).SetEquals(other);
        }
    }
}