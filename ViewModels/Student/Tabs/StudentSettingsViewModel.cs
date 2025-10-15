using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.Data;
using SkyQuizApp.Enums;
using SkyQuizApp.Services;
using SkyQuizApp.Services.Interfaces;

namespace SkyQuizApp.ViewModels.Student
{
    public class StudentSettingsViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _context;
        private readonly IUserSessionService _session;
        private readonly ILogService _logger;
        private readonly IPasswordHasher _hasher;

        public event PropertyChangedEventHandler? PropertyChanged;

        private string _email = string.Empty;

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _newPassword = string.Empty;
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (_newPassword != value)
                {
                    _newPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isTwoFactorEnabled;

        public bool IsTwoFactorEnabled
        {
            get => _isTwoFactorEnabled;
            set
            {
                if (_isTwoFactorEnabled != value)
                {
                    if (!value)
                    {
                        var confirm = MessageBox.Show("Ви впевнені, що хочете вимкнути двофакторну автентифікацію?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (confirm != MessageBoxResult.Yes)
                            return;
                    }

                    _isTwoFactorEnabled = value;
                    OnPropertyChanged();
                    UpdateTwoFactorSetting();
                }
            }
        }

        public string CreatedAtFormatted => _session.CurrentUser?.CreatedAt.ToString("dd.MM.yyyy HH:mm") ?? string.Empty;

        public ICommand ChangeEmailCommand { get; }

        public StudentSettingsViewModel(AppDbContext context, IUserSessionService session)
        {
            _context = context;
            _session = session;

            _hasher = new PasswordHasher();

            _logger = ((App)Application.Current).Services.GetRequiredService<ILogService>();

            var user = session.CurrentUser;
            if (user != null)
            {
                Email = user.Email;
                IsTwoFactorEnabled = user.IsTwoFactorEnabled;
            }

            ChangeEmailCommand = new RelayCommand(_ => ChangeEmail());
        }

        private void ChangeEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Email не може бути порожнім.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var user = _session.CurrentUser;
            if (user == null) return;

            user.Email = Email;
            _context.Users.Update(user);
            _context.SaveChanges();

            _logger.Log(LogAction.EmailChange, $"Email змінено на {Email}", user.UserID);

            MessageBox.Show("Email успішно оновлено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateTwoFactorSetting()
        {
            var user = _session.CurrentUser;
            if (user == null) return;

            user.IsTwoFactorEnabled = _isTwoFactorEnabled;
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public ICommand ChangePasswordCommand => new RelayCommand(_ => ChangePassword());

        private void ChangePassword()
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                MessageBox.Show("Пароль не може бути порожнім.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (NewPassword.Length < 6)
            {
                MessageBox.Show("Пароль має містити щонайменше 6 символів.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = _session.CurrentUser;
            if (user == null) return;

            user.PasswordHash = _hasher.HashPassword(NewPassword);
            _context.Users.Update(user);
            _context.SaveChanges();

            _logger.Log(LogAction.PasswordChange, "Пароль змінено.", user.UserID);
            MessageBox.Show("Пароль успішно змінено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

            NewPassword = string.Empty;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}