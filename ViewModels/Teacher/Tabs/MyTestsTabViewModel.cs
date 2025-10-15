using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.Models;
using SkyQuizApp.Views.Teacher;

namespace SkyQuizApp.ViewModels.Teacher.Tabs
{
    public class MyTestsTabViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _services;

        public ObservableCollection<Test> Tests { get; set; } = new();

        private Test? _selectedTest;

        public Test? SelectedTest
        {
            get => _selectedTest;
            set { _selectedTest = value; OnPropertyChanged(); }
        }

        public ICommand CreateTestCommand { get; }
        public ICommand EditTestCommand { get; }
        public ICommand DeleteTestCommand { get; }
        public ICommand CopyKeyCommand { get; }

        public MyTestsTabViewModel(IServiceProvider services)
        {
            _services = services;

            CreateTestCommand = new RelayCommand(_ => CreateTest());
            EditTestCommand = new RelayCommand(t => EditTest(t as Test));
            DeleteTestCommand = new RelayCommand(t => DeleteTest(t as Test));
            CopyKeyCommand = new RelayCommand(t => CopyKey(t as Test));

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

        private void CreateTest()
        {
            var window = new CreateEditTestWindow();
            if (window.ShowDialog() == true && window.Tag is Test newTest)
            {
                Tests.Insert(0, newTest);
            }
        }

        private void EditTest(Test? test)
        {
            if (test == null)
                return;

            var window = new CreateEditTestWindow(test.TestID);
            if (window.ShowDialog() == true)
            {
                LoadTests();
            }
        }


        public void DeleteTest(Test test)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();

            if (test == null) return;

            var confirm = MessageBox.Show(
            $"Ви впевнені, що хочете видалити тест \"{test.Title}\"?",
            "Підтвердження видалення",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                if (db.Entry(test).State == EntityState.Detached)
                    db.Tests.Attach(test);

                var questions = db.Questions
                    .Where(q => q.TestID == test.TestID)
                    .ToList();
                var questionIds = questions.Select(q => q.QuestionID).ToList();

                var answers = db.Answers
                    .Where(a => questionIds.Contains(a.QuestionID))
                    .ToList();
                db.Answers.RemoveRange(answers);

                var answerIds = answers.Select(a => a.AnswerID).ToList();
                var userAnswersByAnswers = db.UserAnswers
                    .Where(ua => answerIds.Contains(ua.AnswerID))
                    .ToList();
                var userAnswersByQuestions = db.UserAnswers
                    .Where(ua => questionIds.Contains(ua.QuestionID))
                    .ToList();
                db.UserAnswers.RemoveRange(userAnswersByAnswers);
                db.UserAnswers.RemoveRange(userAnswersByQuestions);

                db.Questions.RemoveRange(questions);

                var sessions = db.TestSessions
                    .Where(ts => ts.TestID == test.TestID)
                    .ToList();
                var sessionIds = sessions.Select(s => s.TestSessionID).ToList();

                var results = db.Results
                    .Where(r => sessionIds.Contains(r.SessionID))
                    .ToList();
                db.Results.RemoveRange(results);

                var orphanedUserAnswers = db.UserAnswers
                    .Where(ua => sessionIds.Contains(ua.TestSessionID))
                    .ToList();
                db.UserAnswers.RemoveRange(orphanedUserAnswers);

                db.TestSessions.RemoveRange(sessions);

                db.Tests.Remove(test);

                db.SaveChanges();

                MessageBox.Show("Тест та всі пов’язані дані успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTests();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Помилка при видаленні тесту: {ex.Message}",
                    "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                foreach (var entry in db.ChangeTracker.Entries().ToList())
                    entry.State = EntityState.Unchanged;
            }
        }


        private void CopyKey(Test? test)
        {
            if (test == null)
                return;

            Clipboard.SetText(test.TestKey);
            MessageBox.Show("Код тесту скопійовано до буфера обміну.", "Скопійовано", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}