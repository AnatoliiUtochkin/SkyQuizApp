using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using SkyQuizApp.Data;
using SkyQuizApp.Enums;

namespace SkyQuizApp.ViewModels.Student.Tabs
{
    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;
        private readonly int _userId;

        public double AverageScorePercent { get; set; }
        public double AverageScore12 { get; set; }
        public string LetterGrade { get; set; } = "—";

        public string BestTestTitle { get; set; } = "Немає даних";
        public double BestTestScore { get; set; }
        public DateTime BestTestDate { get; set; } = DateTime.MinValue;

        public string WorstTestTitle { get; set; } = "Немає даних";
        public double WorstTestScore { get; set; }
        public DateTime WorstTestDate { get; set; } = DateTime.MinValue;

        public int TotalTestsTaken { get; set; }

        public SeriesCollection ScoreSeries { get; set; } = new();
        public List<string> DateLabels { get; set; } = new();

        public SeriesCollection PieSeries { get; set; } = new();
        public SeriesCollection AccuracySeries { get; set; } = new();
        public List<string> TypeLabels { get; set; } = new();

        public StatisticsViewModel(AppDbContext context, int userId)
        {
            _context = context;
            _userId = userId;

            try
            {
                LoadData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("StatisticsViewModel error: " + ex);
            }
        }

        public void Refresh()
        {
            ScoreSeries.Clear();
            PieSeries.Clear();
            AccuracySeries.Clear();
            LoadData();
        }

        private void LoadData()
        {
            var sessions = _context.TestSessions.Where(s => s.UserID == _userId).ToList();
            var sessionIds = sessions.Select(s => s.TestSessionID).ToList();
            var results = _context.Results.Where(r => sessionIds.Contains(r.SessionID)).ToList();
            var allTests = _context.Tests.ToList();

            if (results.Count == 0)
            {
                AverageScorePercent = 0;
                AverageScore12 = 0;
                LetterGrade = "—";
                TotalTestsTaken = 0;
                TypeLabels = new();
                DateLabels = new();
                OnAllPropsChanged();
                return;
            }

            TotalTestsTaken = results.Count;
            AverageScorePercent = Math.Round(results.Average(r => (double)r.Score), 1);
            AverageScore12 = Math.Round(AverageScorePercent / 100 * 12, 1);
            LetterGrade = GetLetterGrade(AverageScorePercent);

            var best = results.OrderByDescending(r => r.Score).First();
            var worst = results.OrderBy(r => r.Score).First();

            var bestSession = sessions.FirstOrDefault(s => s.TestSessionID == best.SessionID);
            var worstSession = sessions.FirstOrDefault(s => s.TestSessionID == worst.SessionID);

            var bestTest = allTests.FirstOrDefault(t => t.TestID == bestSession?.TestID);
            var worstTest = allTests.FirstOrDefault(t => t.TestID == worstSession?.TestID);

            BestTestTitle = bestTest?.Title ?? "Немає даних";
            BestTestScore = (double)best.Score;
            BestTestDate = bestSession?.CompletedAt ?? DateTime.MinValue;

            WorstTestTitle = worstTest?.Title ?? "Немає даних";
            WorstTestScore = (double)worst.Score;
            WorstTestDate = worstSession?.CompletedAt ?? DateTime.MinValue;

            var scoreData = sessions.Join(results,
                                          s => s.TestSessionID,
                                          r => r.SessionID,
                                          (s, r) => new { s.CompletedAt, r.Score })
                                     .OrderBy(x => x.CompletedAt)
                                     .ToList();

            DateLabels = scoreData.Select(x => x.CompletedAt.ToString("dd.MM")).ToList();

            if (scoreData.Any())
            {
                ScoreSeries.Add(new LineSeries
                {
                    Values = new ChartValues<double>(scoreData.Select(x => (double)x.Score)),
                    Stroke = Brushes.CornflowerBlue,
                    Fill = Brushes.Transparent,
                    PointGeometrySize = 8
                });
            }

            var testsByMonth = sessions
                .GroupBy(s => new { s.CompletedAt.Year, s.CompletedAt.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Count = g.Count()
                })
                .ToList();

            foreach (var item in testsByMonth)
            {
                PieSeries.Add(new PieSeries
                {
                    Title = item.Month,
                    Values = new ChartValues<int> { item.Count },
                    DataLabels = true
                });
            }

            var questionTypes = Enum.GetValues(typeof(QuestionType)).Cast<QuestionType>();
            var accuracyValues = new ChartValues<double>();
            TypeLabels = new List<string>();

            foreach (var qt in questionTypes.Where(q => q != QuestionType.ShortAnswer))
            {
                var questionIds = _context.Questions
                    .Where(q => q.QuestionType == qt)
                    .Select(q => q.QuestionID)
                    .ToList();

                if (!questionIds.Any())
                {
                    TypeLabels.Add(GetQuestionTypeDescription(qt));
                    accuracyValues.Add(0);
                    continue;
                }

                var answers = _context.UserAnswers
                    .Where(ua => ua.UserID == _userId && questionIds.Contains(ua.QuestionID))
                    .ToList();

                double accuracy = answers.Count > 0
                    ? answers.Count(a => a.IsCorrect) * 100.0 / answers.Count
                    : 0;

                TypeLabels.Add(GetQuestionTypeDescription(qt));
                accuracyValues.Add(Math.Round(accuracy, 1));
            }

            if (accuracyValues.Any())
            {
                AccuracySeries.Add(new ColumnSeries
                {
                    Title = "",
                    Values = accuracyValues,
                    DataLabels = true,
                    LabelPoint = p => $"{p.Y:F1}%",
                    Fill = new SolidColorBrush(Colors.MediumSeaGreen),
                    Foreground = Brushes.White
                });
            }


            OnAllPropsChanged();
        }

        private void OnAllPropsChanged()
        {
            OnPropertyChanged(nameof(AverageScorePercent));
            OnPropertyChanged(nameof(AverageScore12));
            OnPropertyChanged(nameof(LetterGrade));
            OnPropertyChanged(nameof(BestTestTitle));
            OnPropertyChanged(nameof(BestTestScore));
            OnPropertyChanged(nameof(BestTestDate));
            OnPropertyChanged(nameof(WorstTestTitle));
            OnPropertyChanged(nameof(WorstTestScore));
            OnPropertyChanged(nameof(WorstTestDate));
            OnPropertyChanged(nameof(TotalTestsTaken));
            OnPropertyChanged(nameof(ScoreSeries));
            OnPropertyChanged(nameof(DateLabels));
            OnPropertyChanged(nameof(PieSeries));
            OnPropertyChanged(nameof(AccuracySeries));
            OnPropertyChanged(nameof(TypeLabels));
        }

        private string GetLetterGrade(double score) =>
            score switch
            {
                >= 97 => "A+",
                >= 93 => "A",
                >= 90 => "A−",
                >= 87 => "B+",
                >= 83 => "B",
                >= 80 => "B−",
                >= 77 => "C+",
                >= 73 => "C",
                >= 70 => "C−",
                >= 67 => "D+",
                >= 63 => "D",
                >= 60 => "D−",
                _ => "F"
            };

        private string GetQuestionTypeDescription(QuestionType type) =>
            type switch
            {
                QuestionType.SingleChoice => "Один правильний варіант",
                QuestionType.MultipleChoice => "Кілька правильних варіантів",
                QuestionType.ShortAnswer => "Коротка відповідь",
                QuestionType.TrueFalse => "Правда / Неправда",
                QuestionType.Matching => "Встановлення відповідностей",
                _ => type.ToString()
            };

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
