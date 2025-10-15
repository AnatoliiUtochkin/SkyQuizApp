using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Commands;
using SkyQuizApp.Enums;
using SkyQuizApp.Services;
using SkyQuizApp.Services.Interfaces;
using SkyQuizApp.Views.Auth;

namespace SkyQuizApp.ViewModels.Auth
{
    public class ResetPasswordViewModel
    {
        private readonly IWindowService _windowService;
        private readonly IResetPasswordService _resetPasswordService;
        private readonly ILogService _logger;

        public ICommand BackToLoginCommand { get; }
        public ICommand SendButtonCommand { get; }

        public ResetPasswordViewModel(
            IWindowService windowService,
            IResetPasswordService resetPasswordService)
        {
            _windowService = windowService;
            _resetPasswordService = resetPasswordService;
            _logger = ((App)Application.Current).Services.GetRequiredService<ILogService>();

            BackToLoginCommand = new RelayCommand(BackToLogin);
            SendButtonCommand = new RelayCommand(async param => await HandleSendButtonAsync(param));
        }

        public void HandleDrag(Window window, MouseButtonEventArgs e) => WindowHelper.HandleDrag(window, e);

        public void HandleMinimize(Window window) => WindowHelper.HandleMinimize(window);

        public void HandleClose(Window window) => Application.Current.Shutdown();

        public void BackToLogin(object? parameter)
        {
            _windowService.SetCurrentWindow((Window)parameter!);
            _windowService.OpenWindow<LoginView>();
        }

        public async Task HandleSendButtonAsync(object? parameter)
        {
            if (parameter is Window window)
            {
                var emailBox = window.FindName("txtEmail") as TextBox;

                if (emailBox == null || string.IsNullOrWhiteSpace(emailBox.Text))
                {
                    MessageBox.Show("Введіть email для скидання паролю.");
                    return;
                }

                string userEmail = emailBox.Text.Trim();

                bool result = await _resetPasswordService.SendResetPasswordEmail(userEmail);

                if (result)
                {
                    MessageBox.Show("Новий пароль надіслано на вашу електронну пошту. Перевірте вхідну скриньку або папку спам.");
                    _logger.Log(LogAction.CodeRequested, $"Скидання паролю ініційовано: {userEmail}");
                }
                else
                {
                    MessageBox.Show("Користувача з таким email не знайдено.");
                    _logger.Log(LogAction.CodeRejected, $"Спроба скидання паролю: невідомий email — {userEmail}");
                }
            }
        }
    }
}