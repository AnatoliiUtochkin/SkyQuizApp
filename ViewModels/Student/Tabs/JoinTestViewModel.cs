using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.Data;
using SkyQuizApp.Services.Interfaces;
using SkyQuizApp.Views.Student;

namespace SkyQuizApp.ViewModels.Student.Tabs
{
    public class JoinTestViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;
        private readonly IServiceProvider _services;
        private readonly IUserSessionService _session;

        public JoinTestViewModel(IServiceProvider services, AppDbContext db, IUserSessionService session)
        {
            _services = services;
            _db = db;
            _session = session;

            JoinTestCommand = new RelayCommand(_ => JoinTest());
        }

        private string _testKey = string.Empty;

        public string TestKey
        {
            get => _testKey;
            set
            {
                _testKey = value;
                OnPropertyChanged();
            }
        }

        public ICommand JoinTestCommand { get; }

        private void JoinTest()
        {
            if (string.IsNullOrWhiteSpace(TestKey))
            {
                MessageBox.Show("Введіть код тесту.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var test = _db.Tests.FirstOrDefault(t => t.TestKey == TestKey);

            if (test == null)
            {
                MessageBox.Show("Тест не знайдено або код неправильний.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var userId = _session.CurrentUser!.UserID;
            var attemptsMade = _db.TestSessions.Count(s => s.TestID == test.TestID && s.UserID == userId);

            if (test.AttemptsLimit > 0 && attemptsMade >= test.AttemptsLimit)
            {
                MessageBox.Show($"Ви вже використали всі дозволені спроби проходження тесту.", "Доступ заборонено", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (test.Deadline.HasValue)
            {
                var now = DateTime.UtcNow;
                var minutesLeft = (test.Deadline.Value - now).TotalMinutes;

                if (minutesLeft < 5)
                {
                    MessageBox.Show("Термін здачі цього тесту вичерпано або залишилось менше 5 хв.", "Недоступно", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (minutesLeft < test.TimeLimitMinutes)
                {
                    MessageBox.Show($"Увага! До дедлайну залишилось лише {Math.Floor(minutesLeft)} хв.\nЧас проходження буде обмежено.", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            var viewModel = ActivatorUtilities.CreateInstance<TestSessionViewModel>(_services, test);
            var window = new TestSessionWindow
            {
                DataContext = viewModel
            };
            window.Show();

            var mainView = Application.Current.Windows.OfType<StudentMainView>().FirstOrDefault();
            mainView?.HideTemporarily();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}