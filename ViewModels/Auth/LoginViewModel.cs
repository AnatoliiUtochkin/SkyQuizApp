using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.Data;
using SkyQuizApp.Enums;
using SkyQuizApp.Models;
using SkyQuizApp.Services;
using SkyQuizApp.Services.Interfaces;
using SkyQuizApp.Views.Admin;
using SkyQuizApp.Views.Auth;
using SkyQuizApp.Views.Student;
using SkyQuizApp.Views.Teacher;

namespace SkyQuizApp.ViewModels.Auth
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IWindowService _windowService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;
        private readonly IUserSessionService _session;
        private readonly ILogService _logger;

        public ICommand OpenResetPasswordCommand { get; }
        public ICommand HandleLoginButtonCommand { get; }

        public User? AuthenticatedUser { get; private set; }

        public LoginViewModel(
            IWindowService windowService,
            IServiceProvider serviceProvider,
            IAuthService authService,
            AppDbContext context,
            IUserSessionService session)
        {
            _windowService = windowService;
            _serviceProvider = serviceProvider;
            _authService = authService;
            _context = context;
            _session = session;
            _logger = serviceProvider.GetRequiredService<ILogService>();

            OpenResetPasswordCommand = new RelayCommand(OpenResetPassword);
            HandleLoginButtonCommand = new RelayCommand(async param => await HandleLoginAsync(param));
        }

        private string _email = string.Empty;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _password = string.Empty;

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public void HandleDrag(Window window, MouseButtonEventArgs e)
            => WindowHelper.HandleDrag(window, e);

        public void HandleMinimize(Window window)
            => WindowHelper.HandleMinimize(window);

        public void HandleClose(Window window)
            => Application.Current.Shutdown();

        public void OpenResetPassword(object? parameter)
        {
            _windowService.SetCurrentWindow((Window)parameter!);
            var resetView = _serviceProvider.GetRequiredService<ResetPasswordView>();
            resetView.Show();
            _windowService.CloseWindow();
        }

        private async Task HandleLoginAsync(object? parameter)
        {
            var loginResult = await _authService.IsSuccessfulLogin(Email, Password);
            var user = await _authService.GetUserByEmailAsync(Email);

            if (loginResult.IsSuccess && user != null)
            {
                AuthenticatedUser = user;
                _session.CurrentUser = user;

                user.LastLogin = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.Log(LogAction.UserLogin, $"Успішний вхід: {user.Email}", user.UserID);

                if (user.IsTwoFactorEnabled)
                {
                    await OpenTwoFactorAuthViewAsync(parameter);
                    return;
                }

                _windowService.SetCurrentWindow((Window)parameter!);

                switch (user.Role)
                {
                    case UserRole.Administrator:
                        MessageBox.Show("Вітаємо, адміністратор!", "Успішний вхід", MessageBoxButton.OK, MessageBoxImage.Information);
                        _serviceProvider.GetRequiredService<AdminMainView>().Show();
                        break;

                    case UserRole.Teacher:
                        MessageBox.Show("Вітаємо, викладач!", "Успішний вхід", MessageBoxButton.OK, MessageBoxImage.Information);
                        _serviceProvider.GetRequiredService<TeacherMainView>().Show();
                        break;

                    case UserRole.Student:
                        MessageBox.Show("Вітаємо!", "Успішний вхід", MessageBoxButton.OK, MessageBoxImage.Information);
                        _serviceProvider.GetRequiredService<StudentMainView>().Show();
                        break;
                }

                _windowService.CloseWindow();
            }
            else
            {
                if (user != null)
                {
                    switch (loginResult.FailureReason)
                    {
                        case LoginFailureReason.InvalidPassword:
                            _logger.Log(LogAction.PasswordRejected, $"Хибний пароль: {user.Email}", user.UserID);
                            break;

                        case LoginFailureReason.UserBlocked:
                            _logger.Log(LogAction.UserBlocked, $"Спроба входу заблокованим користувачем: {user.Email}", user.UserID);
                            break;
                    }
                }

                switch (loginResult.FailureReason)
                {
                    case LoginFailureReason.UserNotFound:
                        MessageBox.Show("Користувача не знайдено", "Помилка входу", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;

                    case LoginFailureReason.InvalidPassword:
                        MessageBox.Show("Невірний пароль", "Помилка входу", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;

                    case LoginFailureReason.UserBlocked:
                        MessageBox.Show("Користувача заблоковано. Зверніться до адміністратора.", "Помилка входу", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
        }

        private async Task OpenTwoFactorAuthViewAsync(object? parameter)
        {
            var twoFactorAuthViewModel = _serviceProvider.GetRequiredService<TwoFactorAuthViewModel>();
            var twoFactorAuthView = _serviceProvider.GetRequiredService<TwoFactorAuthView>();

            twoFactorAuthViewModel.SetCurrentUser(AuthenticatedUser!);
            await twoFactorAuthViewModel.InitializeAsync();

            _windowService.SetCurrentWindow((Window)parameter!);
            twoFactorAuthView.Show();
            _windowService.CloseWindow();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}