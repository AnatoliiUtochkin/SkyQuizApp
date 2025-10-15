using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.Data;
using SkyQuizApp.Enums;
using SkyQuizApp.Models;
using SkyQuizApp.Services.Interfaces;
using SkyQuizApp.Views.Teacher;

namespace SkyQuizApp.ViewModels.Teacher
{
    public class CreateEditTestViewModel : INotifyPropertyChanged
    {
        private readonly int? _testId;
        private readonly Data.AppDbContext _db;
        private readonly IUserSessionService _session;
        private readonly ILogService _logger;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TimeLimitMinutes { get; set; } = 30;
        public bool ShuffleQuestions { get; set; } = false;
        public int AttemptsLimit { get; set; } = 0;

        private bool _hasDeadline;

        public bool HasDeadline
        {
            get => _hasDeadline;
            set
            {
                _hasDeadline = value;
                OnPropertyChanged();
            }
        }

        private DateTime _deadline = DateTime.UtcNow;

        public DateTime Deadline
        {
            get => _deadline;
            set
            {
                _deadline = value;
                OnPropertyChanged();
                UpdateDeadlineFields();
            }
        }

        private string _deadlineDate = "";

        public string DeadlineDate
        {
            get => _deadlineDate;
            set
            {
                if (_deadlineDate != value)
                {
                    _deadlineDate = value;
                    OnPropertyChanged();
                    TryUpdateDeadline();
                }
            }
        }

        private string _deadlineTime = "";

        public string DeadlineTime
        {
            get => _deadlineTime;
            set
            {
                if (_deadlineTime != value)
                {
                    _deadlineTime = value;
                    OnPropertyChanged();
                    TryUpdateDeadline();
                }
            }
        }

        private void TryUpdateDeadline()
        {
            if (DateTime.TryParseExact($"{DeadlineDate} {DeadlineTime}", "dd.MM.yyyy HH:mm", null,
                                       System.Globalization.DateTimeStyles.None, out var parsed))
            {
                Deadline = parsed;
            }
        }

        private void UpdateDeadlineFields()
        {
            DeadlineDate = Deadline.ToString("dd.MM.yyyy");
            DeadlineTime = Deadline.ToString("HH:mm");
        }

        public ObservableCollection<Question> Questions { get; set; } = new();

        public ObservableCollection<int> AvailableAttempts { get; } =
            new ObservableCollection<int> { 0, 1, 2, 3, 4, 5 };

        public ICommand AddQuestionCommand { get; }
        public ICommand EditQuestionCommand { get; }
        public ICommand DeleteQuestionCommand { get; }
        public ICommand SaveCommand { get; }

        public CreateEditTestViewModel(int? testId)
        {
            var services = (App.Current as App)?.Services
                ?? throw new InvalidOperationException("Сервіси недоступні.");

            _testId = testId;
            _db = services.GetRequiredService<Data.AppDbContext>();
            _session = services.GetRequiredService<IUserSessionService>();
            _logger = services.GetRequiredService<ILogService>();

            AddQuestionCommand = new RelayCommand(_ => AddQuestion());
            EditQuestionCommand = new RelayCommand(question => EditQuestion(question as Question));
            DeleteQuestionCommand = new RelayCommand(question => DeleteQuestion(question as Question));
            SaveCommand = new RelayCommand(_ => Save());

            if (_testId.HasValue)
                LoadTest();
        }
        public CreateEditTestViewModel(int? testId, AppDbContext db, IUserSessionService session, ILogService logger)
        {
            _testId = testId;
            _db = db;
            _session = session;
            _logger = logger;

            AddQuestionCommand = new RelayCommand(_ => AddQuestion());
            EditQuestionCommand = new RelayCommand(question => EditQuestion(question as Question));
            DeleteQuestionCommand = new RelayCommand(question => DeleteQuestion(question as Question));
            SaveCommand = new RelayCommand(_ => Save());

            if (_testId.HasValue)
                LoadTest();
        }


        private void LoadTest()
        {
            var test = _db.Tests.Include(t => t.User)
                                .Include(t => t.Questions)
                                .ThenInclude(q => q.Answers)
                                .FirstOrDefault(t => t.TestID == _testId.Value);
            if (test != null)
            {
                Title = test.Title;
                Description = test.Description;
                TimeLimitMinutes = test.TimeLimitMinutes;
                ShuffleQuestions = test.ShuffleQuestions;
                HasDeadline = test.Deadline.HasValue;
                Deadline = test.Deadline ?? DateTime.UtcNow;
                AttemptsLimit = test.AttemptsLimit;

                Questions = new ObservableCollection<Question>(test.Questions?.ToList() ?? []);
            }
        }

        private void AddQuestion()
        {
            var window = new CreateEditQuestionWindow();
            if (window.ShowDialog() == true && window.Tag is Question question)
            {
                Questions.Add(question);
            }
        }

        private void EditQuestion(Question? question)
        {
            if (question == null)
                return;

            var window = new CreateEditQuestionWindow(question);
            if (window.ShowDialog() == true && window.Tag is Question editedQuestion)
            {
                var index = Questions.IndexOf(question);
                if (index >= 0)
                {
                    Questions[index] = editedQuestion;
                }
            }
        }

        private void DeleteQuestion(Question? question)
        {
            if (question != null)
                Questions.Remove(question);
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                MessageBox.Show("Назва тесту обов'язкова.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TimeLimitMinutes < 1 || TimeLimitMinutes > 300)
            {
                MessageBox.Show("Ліміт часу має бути від 1 до 300 хвилин.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_session.CurrentUser == null)
            {
                MessageBox.Show("Користувач не залогінений.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (HasDeadline && Deadline <= DateTime.UtcNow)
            {
                MessageBox.Show("Термін здачі повинен бути в майбутньому.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Test test;
            if (_testId.HasValue)
            {
                test = _db.Tests.First(t => t.TestID == _testId.Value);
                test.Title = Title;
                test.Description = Description;
                test.TimeLimitMinutes = TimeLimitMinutes;
                test.ShuffleQuestions = ShuffleQuestions;
                test.Deadline = HasDeadline ? Deadline : null;
                test.AttemptsLimit = AttemptsLimit;

                _logger.Log(LogAction.TestUpdated, $"Тест ID={test.TestID} оновлено.");
            }
            else
            {
                test = new Test
                {
                    Title = Title,
                    Description = Description,
                    TimeLimitMinutes = TimeLimitMinutes,
                    ShuffleQuestions = ShuffleQuestions,
                    Deadline = HasDeadline ? Deadline : null,
                    AttemptsLimit = AttemptsLimit,
                    UserID = _session.CurrentUser.UserID,
                    TestKey = GenerateUniqueTestKey()
                };
                _db.Tests.Add(test);
                _db.SaveChanges();

                _logger.Log(LogAction.TestCreated, $"Тест створено ID={test.TestID}.");
            }

            if (_testId.HasValue)
            {
                var oldQuestions = _db.Questions
                    .Include(q => q.Answers)
                    .Where(q => q.TestID == test.TestID)
                    .ToList();

                foreach (var q in oldQuestions)
                {
                    if (q.Answers != null && q.Answers.Any())
                    {
                        _db.Answers.RemoveRange(q.Answers);
                    }
                }

                _db.Questions.RemoveRange(oldQuestions);
                _db.SaveChanges();
            }

            foreach (var q in Questions)
            {
                q.TestID = test.TestID;
                q.QuestionID = 0;

                if (q.Answers != null)
                {
                    foreach (var answer in q.Answers)
                    {
                        answer.AnswerID = 0;
                    }
                }

                _db.Questions.Add(q);
            }

            _db.SaveChanges();

            MessageBox.Show("Тест збережено успішно.");

            var window = App.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (window != null)
            {
                window.Tag = test;
                window.DialogResult = true;
                window.Close();
            }
        }

        private string GenerateUniqueTestKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string key;
            do
            {
                key = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
            } while (_db.Tests.Any(t => t.TestKey == key));

            return key;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}