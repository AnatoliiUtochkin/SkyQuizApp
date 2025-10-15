using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.DTOs;
using SkyQuizApp.Views.Teacher;

namespace SkyQuizApp.ViewModels.Teacher.Tabs
{
    public class ActivityLogTabViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _services;

        public ObservableCollection<StudentActivityDto> Students { get; set; } = new();

        public ICommand ViewStudentActivityCommand { get; }

        public ActivityLogTabViewModel(IServiceProvider services)
        {
            _services = services;
            ViewStudentActivityCommand = new RelayCommand(s => ViewStudentActivity(s as StudentActivityDto));

            LoadStudents();
        }

        private void LoadStudents()
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
            var session = _services.GetRequiredService<Services.Interfaces.IUserSessionService>();

            if (session.CurrentUser == null)
                return;

            int teacherId = session.CurrentUser.UserID;

            var query = db.TestSessions
                .Include(s => s.User)
                .Include(s => s.Result)
                .Include(s => s.Test)
                .Where(s => s.Test!.UserID == teacherId && s.Result != null)
                .AsNoTracking()
                .ToList();

            var grouped = query
                .GroupBy(s => s.UserID)
                .Select(g =>
                {
                    var user = g.First().User!;
                    return new StudentActivityDto
                    {
                        UserID = user.UserID,
                        FullName = user.FullName,
                        TestCount = g.Count(),
                        AverageScore = g.Average(s => s.Result!.Score)
                    };
                })
                .OrderByDescending(s => s.AverageScore)
                .ToList();

            Students.Clear();
            foreach (var s in grouped)
                Students.Add(s);
        }

        private void ViewStudentActivity(StudentActivityDto? student)
        {
            if (student == null)
                return;

            var window = new StudentActivityWindow(student.UserID);
            window.ShowDialog();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}