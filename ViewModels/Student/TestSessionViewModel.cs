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
using SkyQuizApp.Views.Student;

namespace SkyQuizApp.ViewModels.Student
{
    public class TestSessionViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;
        private readonly IUserSessionService _session;
        private readonly IServiceProvider _services;
        private System.Timers.Timer _countdownTimer;
        private DateTime _endTime;

        public ObservableCollection<Question> Questions { get; private set; } = new();
        public ObservableCollection<QuestionNavItem> QuestionNavItems { get; private set; } = new();
        public ObservableCollection<MatchingPair> MatchingPairs { get; private set; } = new();

        private readonly Dictionary<int, ObservableCollection<MatchingPair>> _matchingCache = new();
        private readonly Dictionary<int, Dictionary<string, int>> _matchingSelected = new();
        private readonly Dictionary<int, int> _singleSelected = new();
        private readonly Dictionary<int, List<int>> _multiSelected = new();
        private readonly Dictionary<int, string> _shortAnswerCache = new();

        public Dictionary<int, bool> IsAnswerSelected { get; private set; } = new();

        private readonly ILogService _logger;

        private int _currentQuestionIndex;

        public int CurrentQuestionIndex
        {
            get => _currentQuestionIndex;
            set
            {
                if (_currentQuestionIndex != value)
                {
                    SaveAnswer();
                    _currentQuestionIndex = value;
                    OnPropertyChanged();
                    LoadCurrentAnswer();
                    UpdateIsAnswerSelected();
                    OnPropertyChanged(nameof(CurrentQuestion));
                    OnPropertyChanged(nameof(IsFirstQuestion));
                    OnPropertyChanged(nameof(IsNotFirstQuestion));
                    OnPropertyChanged(nameof(NextOrFinishLabel));
                    OnPropertyChanged(nameof(SelectedAnswerId));
                }
            }
        }

        public Question CurrentQuestion => Questions.ElementAtOrDefault(CurrentQuestionIndex) ?? new Question { Text = "(Немає запитання)" };
        public bool IsFirstQuestion => CurrentQuestionIndex == 0;
        public bool IsNotFirstQuestion => !IsFirstQuestion;
        public string NextOrFinishLabel => CurrentQuestionIndex == Questions.Count - 1 ? "Завершити тест" : "Наступне";

        private string _shortAnswerText = string.Empty;

        public string ShortAnswerText
        {
            get => _shortAnswerText;
            set { _shortAnswerText = value; OnPropertyChanged(); }
        }

        private string _timeLeftText = string.Empty;

        public string TimeLeftText
        {
            get => _timeLeftText;
            set { _timeLeftText = value; OnPropertyChanged(); }
        }

        private double _timerProgress;

        public double TimerProgress
        {
            get => _timerProgress;
            set { _timerProgress = value; OnPropertyChanged(); }
        }

        public int? SelectedAnswerId
        {
            get => _singleSelected.TryGetValue(CurrentQuestion.QuestionID, out var id) ? id : null;
            set
            {
                if (value.HasValue)
                {
                    _singleSelected[CurrentQuestion.QuestionID] = value.Value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SelectAnswerCommand { get; }
        public ICommand NavigateToQuestionCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand NextOrFinishCommand { get; }

        private TestSession _testSession;
        private readonly Test _test;

        public TestSessionViewModel(AppDbContext db, IUserSessionService session, IServiceProvider services, Test test)
        {
            _db = db;
            _session = session;
            _services = services;
            _test = test;
            _logger = services.GetRequiredService<ILogService>();

            SelectAnswerCommand = new RelayCommand(id =>
            {
                if (id is int answerId)
                    HandleSelectAnswer(answerId);
            });

            NavigateToQuestionCommand = new RelayCommand(param => NavigateToQuestion((int)param));
            PreviousCommand = new RelayCommand(_ => CurrentQuestionIndex = Math.Max(CurrentQuestionIndex - 1, 0));
            NextOrFinishCommand = new RelayCommand(_ => ExecuteNextOrFinish());

            LoadQuestions();
            StartSession();
        }

        private void LoadQuestions()
        {
            var questions = _db.Questions
                .Where(q => q.TestID == _test.TestID)
                .Include(q => q.Answers)
                .ToList();

            if (_test.ShuffleQuestions)
            {
                var rng = new Random();

                var matching = questions.Where(q => q.QuestionType == QuestionType.Matching).ToList();
                var nonMatching = questions.Where(q => q.QuestionType != QuestionType.Matching).OrderBy(_ => rng.Next()).ToList();

                matching = matching.OrderBy(_ => rng.Next()).ToList();

                var result = nonMatching;

                if (matching.Count > 0)
                {
                    int insertIndex = Math.Min(2, result.Count);
                    result.Insert(insertIndex, matching[0]);

                    for (int i = 1; i < matching.Count; i++)
                    {
                        int index = rng.Next(insertIndex + 1, result.Count + 1);
                        result.Insert(index, matching[i]);
                    }
                }

                Questions = new ObservableCollection<Question>(result);
            }
            else
            {
                Questions = new ObservableCollection<Question>(questions);
            }

            OnPropertyChanged(nameof(Questions));

            QuestionNavItems = new ObservableCollection<QuestionNavItem>(Questions.Select((q, i) => new QuestionNavItem(i + 1, q)));
            OnPropertyChanged(nameof(QuestionNavItems));

            CurrentQuestionIndex = 0;
        }

        private void LoadCurrentAnswer()
        {
            var q = CurrentQuestion;
            switch (q.QuestionType)
            {
                case QuestionType.ShortAnswer:
                    ShortAnswerText = _shortAnswerCache.TryGetValue(q.QuestionID, out var text) ? text : string.Empty;
                    break;

                case QuestionType.SingleChoice:
                case QuestionType.TrueFalse:
                    OnPropertyChanged(nameof(SelectedAnswerId));
                    break;

                case QuestionType.MultipleChoice:
                    UpdateIsAnswerSelected();
                    break;

                case QuestionType.Matching:
                    LoadMatchingQuestion();
                    break;
            }
        }

        private void LoadMatchingQuestion()
        {
            var q = CurrentQuestion;

            if (_matchingCache.TryGetValue(q.QuestionID, out var cached))
            {
                MatchingPairs = cached;

                if (_matchingSelected.TryGetValue(q.QuestionID, out var selectedMap))
                {
                    foreach (var pair in MatchingPairs)
                    {
                        if (selectedMap.TryGetValue(pair.Left.Text, out var savedAnswerId))
                        {
                            pair.Selected = pair.Options.FirstOrDefault(o => o.AnswerID == savedAnswerId);
                        }
                    }
                }

                OnPropertyChanged(nameof(MatchingPairs));
                return;
            }

            var allPairs = q.Answers
                .Select(a =>
                {
                    var parts = a.Text.Split('=', 2);
                    return new
                    {
                        Left = parts[0].Trim(),
                        Right = parts.Length > 1 ? parts[1].Trim() : string.Empty,
                        Answer = a
                    };
                })
                .Where(p => !string.IsNullOrWhiteSpace(p.Right))
                .ToList();

            var rightOptions = allPairs.Select(p => new
            {
                p.Answer.AnswerID,
                Text = p.Right,
                p.Answer.IsCorrect
            }).ToList();

            var shuffledRightOptions = rightOptions
                .OrderBy(_ => Guid.NewGuid())
                .Select(opt => new Answer
                {
                    AnswerID = opt.AnswerID,
                    Text = opt.Text,
                    IsCorrect = opt.IsCorrect
                })
                .ToList();

            var newPairs = new ObservableCollection<MatchingPair>();

            foreach (var p in allPairs)
            {
                newPairs.Add(new MatchingPair
                {
                    Left = new Answer { Text = p.Left },
                    Options = new ObservableCollection<Answer>(shuffledRightOptions),
                    Selected = null
                });
            }

            _matchingCache[q.QuestionID] = newPairs;
            MatchingPairs = newPairs;
            OnPropertyChanged(nameof(MatchingPairs));
        }

        private void StartSession()
        {
            var now = DateTime.UtcNow;
            _testSession = new TestSession
            {
                UserID = _session.CurrentUser!.UserID,
                TestID = _test.TestID,
                StartedAt = now,
                CompletedAt = _test.Deadline.HasValue && _test.Deadline.Value < now.AddMinutes(_test.TimeLimitMinutes)
                    ? _test.Deadline.Value : now.AddMinutes(_test.TimeLimitMinutes)
            };

            _db.TestSessions.Add(_testSession);
            _db.SaveChanges();

            _logger.Log(LogAction.TestAttempted, $"Почато тест ID={_test.TestID}");

            _endTime = _testSession.CompletedAt;
            StartTimer();
        }

        private void StartTimer()
        {
            var total = (_endTime - _testSession.StartedAt).TotalSeconds;
            _countdownTimer = new System.Timers.Timer(1000);
            _countdownTimer.Elapsed += (s, e) =>
            {
                var remaining = _endTime - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero)
                {
                    _countdownTimer.Stop();
                    Application.Current.Dispatcher.Invoke(SubmitTest);
                }
                else
                {
                    var percent = 100.0 * remaining.TotalSeconds / total;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TimeLeftText = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
                        TimerProgress = percent;
                    });
                }
            };
            _countdownTimer.Start();
        }

        private void SaveAnswer()
        {
            var q = CurrentQuestion;
            var userId = _session.CurrentUser!.UserID;
            var existing = _db.UserAnswers.Where(a => a.QuestionID == q.QuestionID && a.TestSessionID == _testSession.TestSessionID && a.UserID == userId);
            _db.UserAnswers.RemoveRange(existing);

            switch (q.QuestionType)
            {
                case QuestionType.SingleChoice:
                case QuestionType.TrueFalse:
                    if (_singleSelected.TryGetValue(q.QuestionID, out var sid))
                        AddUserAnswer(q, sid);
                    break;

                case QuestionType.MultipleChoice:
                    if (_multiSelected.TryGetValue(q.QuestionID, out var mids))
                        foreach (var id in mids)
                            AddUserAnswer(q, id);
                    break;

                case QuestionType.ShortAnswer:
                    if (!string.IsNullOrWhiteSpace(ShortAnswerText))
                    {
                        _shortAnswerCache[q.QuestionID] = ShortAnswerText;
                        var answer = new Answer { QuestionID = q.QuestionID, Text = ShortAnswerText, IsCorrect = false };
                        _db.Answers.Add(answer);
                        _db.SaveChanges();
                        AddUserAnswer(q, answer.AnswerID);
                    }
                    break;

                case QuestionType.Matching:
                    var selectedMap = new Dictionary<string, int>();
                    foreach (var pair in MatchingPairs)
                    {
                        if (pair.Selected != null)
                        {
                            AddUserAnswer(q, pair.Selected.AnswerID);
                            selectedMap[pair.Left.Text] = pair.Selected.AnswerID;
                        }
                    }
                    _matchingSelected[q.QuestionID] = selectedMap;
                    break;
            }

            _db.SaveChanges();
        }

        private void AddUserAnswer(Question q, int answerId)
        {
            var ans = q.Answers.FirstOrDefault(a => a.AnswerID == answerId);
            _db.UserAnswers.Add(new UserAnswer
            {
                TestSessionID = _testSession.TestSessionID,
                QuestionID = q.QuestionID,
                AnswerID = answerId,
                UserID = _session.CurrentUser!.UserID,
                IsCorrect = ans?.IsCorrect ?? false,
                AnsweredAt = DateTime.UtcNow
            });
        }

        private void HandleSelectAnswer(int answerId)
        {
            var q = CurrentQuestion;
            switch (q.QuestionType)
            {
                case QuestionType.MultipleChoice:
                    if (!_multiSelected.ContainsKey(q.QuestionID))
                        _multiSelected[q.QuestionID] = new List<int>();

                    var list = _multiSelected[q.QuestionID];
                    if (list.Contains(answerId)) list.Remove(answerId);
                    else list.Add(answerId);

                    UpdateIsAnswerSelected();
                    break;

                case QuestionType.SingleChoice:
                case QuestionType.TrueFalse:
                    _singleSelected[q.QuestionID] = answerId;
                    OnPropertyChanged(nameof(SelectedAnswerId));
                    break;
            }
        }

        private void UpdateIsAnswerSelected()
        {
            IsAnswerSelected.Clear();
            var q = CurrentQuestion;
            if (q.QuestionType == QuestionType.MultipleChoice && _multiSelected.TryGetValue(q.QuestionID, out var selectedIds))
            {
                foreach (var answer in q.Answers)
                    IsAnswerSelected[answer.AnswerID] = selectedIds.Contains(answer.AnswerID);
            }
            OnPropertyChanged(nameof(IsAnswerSelected));
        }

        private void NavigateToQuestion(int questionId)
        {
            var index = Questions.ToList().FindIndex(q => q.QuestionID == questionId);
            if (index >= 0)
                CurrentQuestionIndex = index;
        }

        private void ExecuteNextOrFinish()
        {
            if (CurrentQuestionIndex == Questions.Count - 1)
            {
                var result = MessageBox.Show("Ви впевнені, що хочете завершити тест?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    SubmitTest();
            }
            else
            {
                CurrentQuestionIndex++;
            }
        }

        private void SubmitTest()
        {
            SaveAnswer();
            _countdownTimer?.Stop();

            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("Тест завершено. Відповіді збережено.", "Завершено", MessageBoxButton.OK);

                var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
                window?.Close();

                var score = CalculateScore();
                _db.Results.Add(new Result
                {
                    SessionID = _testSession.TestSessionID,
                    Score = score
                });
                _db.SaveChanges();

                _logger.Log(LogAction.TestSubmitted, $"Завершено тест ID={_test.TestID}, SessionID={_testSession.TestSessionID}");

                var resultWindow = new TestResultWindow(CalculateScore());
                resultWindow.ShowDialog();

                var studentMainView = Application.Current.Windows.OfType<StudentMainView>().FirstOrDefault();
                studentMainView?.RestoreAfterTest();
            });
        }

        public decimal CalculateScore()
        {
            decimal totalPoints = 0;
            decimal earnedPoints = 0;

            foreach (var q in Questions)
            {
                switch (q.QuestionType)
                {
                    case QuestionType.SingleChoice:
                    case QuestionType.TrueFalse:
                        totalPoints += 1;
                        var single = _db.UserAnswers.FirstOrDefault(a => a.TestSessionID == _testSession.TestSessionID && a.QuestionID == q.QuestionID && a.UserID == _session.CurrentUser!.UserID);
                        if (single?.IsCorrect == true)
                            earnedPoints += 1;
                        break;

                    case QuestionType.MultipleChoice:
                        totalPoints += 1;
                        var correctAnswers = q.Answers.Where(a => a.IsCorrect).Select(a => a.AnswerID).ToHashSet();
                        var selectedAnswers = _db.UserAnswers.Where(a => a.TestSessionID == _testSession.TestSessionID && a.QuestionID == q.QuestionID && a.UserID == _session.CurrentUser!.UserID).Select(a => a.AnswerID).ToHashSet();
                        if (selectedAnswers.SetEquals(correctAnswers))
                            earnedPoints += 1;
                        break;

                    case QuestionType.Matching:
                        var correctPairs = q.Answers.Count(a => a.IsCorrect);
                        totalPoints += correctPairs;
                        var userMatches = _db.UserAnswers.Where(a => a.TestSessionID == _testSession.TestSessionID && a.QuestionID == q.QuestionID && a.UserID == _session.CurrentUser!.UserID).ToList();
                        earnedPoints += userMatches.Count(a => a.IsCorrect);
                        break;

                    case QuestionType.ShortAnswer:
                        var userAnswer = _db.UserAnswers.Include(ua => ua.Answer).FirstOrDefault(a => a.QuestionID == q.QuestionID && a.TestSessionID == _testSession.TestSessionID && a.UserID == _session.CurrentUser!.UserID);
                        var correct = q.Answers.FirstOrDefault(a => a.IsCorrect);
                        if (userAnswer != null && correct != null)
                        {
                            totalPoints += 1;
                            double similarity = CalculateSimilarity(userAnswer.Answer.Text, correct.Text);
                            if (similarity >= 0.85)
                                earnedPoints += 1;
                            else if (similarity >= 0.6)
                                earnedPoints += 0.5m;
                        }
                        break;
                }
            }

            return totalPoints == 0 ? 0 : Math.Round((earnedPoints / totalPoints) * 100, 2);
        }

        private double CalculateSimilarity(string input, string expected)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(expected))
                return 0;

            input = input.Trim().ToLower();
            expected = expected.Trim().ToLower();

            int distance = LevenshteinDistance(input, expected);
            return 1.0 - (double)distance / Math.Max(input.Length, expected.Length);
        }

        private int LevenshteinDistance(string s, string t)
        {
            int[,] d = new int[s.Length + 1, t.Length + 1];
            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[s.Length, t.Length];
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}