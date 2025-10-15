using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.DTOs;
using SkyQuizApp.Enums;
using SkyQuizApp.Services.Interfaces;

namespace SkyQuizApp.ViewModels.Teacher
{
    public class StudentActivityWindowViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _services;
        private readonly ILogService _logger;

        public string StudentEmail { get; set; } = string.Empty;
        public ObservableCollection<StudentTestResultDto> Results { get; set; } = new();

        public StudentActivityWindowViewModel(int studentUserId)
        {
            _services = ((App)App.Current).Services;
            _logger = _services.GetRequiredService<ILogService>();

            LoadData(studentUserId);
        }

        private void LoadData(int studentUserId)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
            var session = _services.GetRequiredService<IUserSessionService>();

            if (session.CurrentUser == null)
                return;

            int teacherId = session.CurrentUser.UserID;

            var sessions = db.TestSessions
                .Include(s => s.Result)
                .Include(s => s.Test)
                .Include(s => s.User)
                .Where(s => s.UserID == studentUserId && s.Test!.UserID == teacherId && s.Result != null)
                .AsNoTracking()
                .ToList();

            if (sessions.Count == 0) return;

            StudentEmail = sessions.First().User?.Email ?? "(невідомо)";

            Results.Clear();
            foreach (var s in sessions)
            {
                Results.Add(new StudentTestResultDto
                {
                    TestTitle = s.Test?.Title ?? "Невідомо",
                    CompletedAt = s.CompletedAt,
                    Score = $"{s.Result!.Score:F1}%",
                    Grade12 = ConvertTo12Scale(s.Result.Score),
                    Grade100 = ((int)Math.Round(s.Result.Score)).ToString()
                });
            }

            _logger.Log(LogAction.ResultViewed, $"Викладач переглянув активність студента: {StudentEmail}", teacherId);

            OnPropertyChanged(nameof(StudentEmail));
            OnPropertyChanged(nameof(Results));
        }

        private string ConvertTo12Scale(decimal score)
        {
            if (score >= 90) return "12";
            if (score >= 85) return "11";
            if (score >= 80) return "10";
            if (score >= 75) return "9";
            if (score >= 70) return "8";
            if (score >= 65) return "7";
            if (score >= 60) return "6";
            if (score >= 55) return "5";
            if (score >= 50) return "4";
            if (score >= 45) return "3";
            if (score >= 35) return "2";
            return "1";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}