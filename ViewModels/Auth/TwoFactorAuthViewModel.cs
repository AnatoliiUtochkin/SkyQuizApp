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
using SkyQuizApp.Services;
using SkyQuizApp.Services.Interfaces;
using SkyQuizApp.Views.Admin;
using SkyQuizApp.Views.Student;
using SkyQuizApp.Views.Teacher;

namespace SkyQuizApp.ViewModels.Auth
{
    public class TwoFactorAuthViewModel : INotifyPropertyChanged
    {
        private readonly ITwoFactorService _twoFactorService;
        private readonly IWindowService _windowService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppDbContext _context;
        private readonly ILogService _logger;

        public User CurrentUser { get; set; }

        private string _enteredCode = string.Empty;

        public string EnteredCode
        {
            get => _enteredCode;
            set
            {
                _enteredCode = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmCodeCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public TwoFactorAuthViewModel(
            ITwoFactorService twoFactorService,
            IWindowService windowService,
            IServiceProvider serviceProvider,
            AppDbContext context)
        {
            _twoFactorService = twoFactorService;
            _windowService = windowService;
            _serviceProvider = serviceProvider;
            _context = context;
            _logger = serviceProvider.GetRequiredService<ILogService>();
            ConfirmCodeCommand = new RelayCommand(async param => await ConfirmCodeAsync(param));
        }

        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public async Task InitializeAsync()
        {
            await _twoFactorService.GenerateAndSendCodeAsync(CurrentUser.UserID, CurrentUser.Email);
            _logger.Log(LogAction.CodeRequested, $"2FA код надіслано: {CurrentUser.Email}", CurrentUser.UserID);
        }

        private async Task ConfirmCodeAsync(object? parameter)
        {
            var codeEntity = await _context.TwoFactorCodes
                .FirstOrDefaultAsync(code => code.Code == EnteredCode && code.ExpiresAt > DateTime.UtcNow);

            if (codeEntity != null)
            {
                bool result = await _twoFactorService.VerifyCodeAsync(codeEntity.UserID, EnteredCode);
                var user = await _context.Users.FindAsync(codeEntity.UserID);

                if (result && user != null)
                {
                    _logger.Log(LogAction.CodeVerified, $"Код 2FA підтверджено: {user.Email}", user.UserID);

                    switch (user.Role)
                    {
                        case UserRole.Teacher:
                            MessageBox.Show("Код підтверджено! Вітаємо, адміністратор.");

                            var teacherMainView = _serviceProvider.GetRequiredService<TeacherMainView>();
                            teacherMainView.Show();
                            break;

                        case UserRole.Student:
                            MessageBox.Show("Код підтверджено!");

                            var studentMainView = _serviceProvider.GetRequiredService<StudentMainView>();
                            studentMainView.Show();
                            break;
 
                        case UserRole.Administrator:
                            MessageBox.Show("Код підтверджено!");

                            var mainView = _serviceProvider.GetRequiredService<AdminMainView>();
                            mainView.Show();
                            break;
                    }

                    if (parameter is Window window)
                        window.Close();
                }
                else
                {
                    _logger.Log(LogAction.CodeRejected, $"Хибний код 2FA: {EnteredCode}", user?.UserID ?? 0);
                    MessageBox.Show("Невірний або прострочений код.");
                }
            }
            else
            {
                _logger.Log(LogAction.CodeRejected, $"Хибний або недійсний код: {EnteredCode}", CurrentUser.UserID);
                MessageBox.Show("Невірний або прострочений код.");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void HandleDrag(Window window, MouseButtonEventArgs e) => WindowHelper.HandleDrag(window, e);

        public void HandleMinimize(Window window) => WindowHelper.HandleMinimize(window);

        public void HandleClose(Window window) => Application.Current.Shutdown();
    }
}