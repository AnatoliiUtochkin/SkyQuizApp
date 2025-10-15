using Microsoft.EntityFrameworkCore;
using SkyQuizApp.Data;
using SkyQuizApp.Enums;
using SkyQuizApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SkyQuizApp.ViewModels.Admin.Tabs
{
    public class TestsTabViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AppDbContext _context;

        public ObservableCollection<Test> Tests { get; } = new();

        public TestsTabViewModel(AppDbContext context)
        {
            _context = context;
            LoadTestsAsync();
        }

        private async void LoadTestsAsync()
        {
            var testsFromDb = await _context.Tests.OrderBy(t => t.TestID).ToListAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Tests.Clear();
                foreach (var test in testsFromDb)
                    Tests.Add(test);
            });
        }

        public Test AddTest()
        {
            var newTest = new Test
            {
                Title = string.Empty,
                Description = string.Empty,
                TestKey = GenerateUniqueTestKey(),
                UserID = 1,
                CreatedAt = DateTime.UtcNow,
                TimeLimitMinutes = 60
            };

            Tests.Add(newTest);
            return newTest;
        }

        public string GenerateUniqueTestKey()
        {
            var random = new Random();
            string key;
            do
            {
                key = new string(Enumerable.Range(0, 6)
                    .Select(_ => (char)random.Next('A', 'Z' + 1))
                    .ToArray());
            } while (_context.Tests.Any(t => t.TestKey == key));
            return key;
        }

        public void DeleteTest(Test test)
        {
            if (test == null) return;

            try
            {
                if (_context.Entry(test).State == EntityState.Detached)
                    _context.Tests.Attach(test);

                var questions = _context.Questions
                    .Where(q => q.TestID == test.TestID)
                    .ToList();
                var questionIds = questions.Select(q => q.QuestionID).ToList();

                var answers = _context.Answers
                    .Where(a => questionIds.Contains(a.QuestionID))
                    .ToList();
                var answerIds = answers.Select(a => a.AnswerID).ToList();

                var userAnswersByAnswers = _context.UserAnswers
                    .Where(ua => answerIds.Contains(ua.AnswerID))
                    .ToList();

                var userAnswersByQuestions = _context.UserAnswers
                    .Where(ua => questionIds.Contains(ua.QuestionID))
                    .ToList();

                _context.UserAnswers.RemoveRange(userAnswersByAnswers);
                _context.UserAnswers.RemoveRange(userAnswersByQuestions);

                _context.Answers.RemoveRange(answers);
                _context.Questions.RemoveRange(questions);

                var sessions = _context.TestSessions
                    .Where(ts => ts.TestID == test.TestID)
                    .ToList();
                var sessionIds = sessions.Select(s => s.TestSessionID).ToList();

                var results = _context.Results
                    .Where(r => sessionIds.Contains(r.SessionID))
                    .ToList();
                _context.Results.RemoveRange(results);

                var orphanedUserAnswers = _context.UserAnswers
                    .Where(ua => sessionIds.Contains(ua.TestSessionID))
                    .ToList();
                _context.UserAnswers.RemoveRange(orphanedUserAnswers);

                _context.TestSessions.RemoveRange(sessions);
                _context.Tests.Remove(test);

                _context.SaveChanges();

                MessageBox.Show("Тест та всі пов’язані дані успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                ReloadAllTests();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Помилка при видаленні тесту: {ex.Message}",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                foreach (var entry in _context.ChangeTracker.Entries().ToList())
                    entry.State = EntityState.Unchanged;
            }
        }

        public void UpdateTest(Test test)
        {
            if (test == null) return;

            if (!_context.Users.Any(u => u.UserID == test.UserID))
            {
                MessageBox.Show("Користувача з таким ID не існує.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                ReloadTest(test.TestID);
                return;
            }

            try
            {
                bool isNew = test.TestID == 0;

                var tracked = _context.ChangeTracker.Entries<Test>()
                    .FirstOrDefault(e => e.Entity.TestID == test.TestID);

                if (tracked != null)
                {
                    tracked.CurrentValues.SetValues(test);
                }
                else
                {
                    if (isNew)
                        _context.Tests.Add(test);
                    else
                        _context.Tests.Attach(test);
                }

                int changes = _context.SaveChanges();

                if (changes > 0)
                    MessageBox.Show("Тест успішно оновлено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                ReloadAllTests();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                ReloadTest(test.TestID);
            }
        }


        public void ReloadTest(int testId)
        {
            var index = Tests.ToList().FindIndex(t => t.TestID == testId);
            if (index == -1) return;

            var updated = _context.Tests.AsNoTracking().FirstOrDefault(t => t.TestID == testId);
            if (updated == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Tests[index] = updated;
            });
        }

        private async void ReloadAllTests()
        {
            var testsFromDb = await _context.Tests.AsNoTracking().ToListAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Tests.Clear();
                foreach (var test in testsFromDb)
                    Tests.Add(test);
            });
        }

        public void Dispose() => _context.Dispose();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}