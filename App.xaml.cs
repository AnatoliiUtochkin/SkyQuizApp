using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkyQuizApp.Configurations;
using SkyQuizApp.Data;
using SkyQuizApp.Services;
using SkyQuizApp.Services.Interfaces;
using SkyQuizApp.ViewModels.Admin;
using SkyQuizApp.ViewModels.Admin.Tabs;
using SkyQuizApp.ViewModels.Auth;
using SkyQuizApp.ViewModels.Student;
using SkyQuizApp.ViewModels.Student.Tabs;
using SkyQuizApp.ViewModels.Teacher;
using SkyQuizApp.ViewModels.Teacher.Tabs;
using SkyQuizApp.Views.Admin;
using SkyQuizApp.Views.Admin.Tabs;
using SkyQuizApp.Views.Auth;
using SkyQuizApp.Views.Student;
using SkyQuizApp.Views.Student.Tabs;
using SkyQuizApp.Views.Teacher;
using SkyQuizApp.Views.Teacher.Tabs;

namespace SkyQuizApp
{
    public partial class App : Application
    {
        private IHost? _host;

        public IServiceProvider Services => _host?.Services
        ?? throw new InvalidOperationException("Сервіси не готові.");

        protected override void OnStartup(StartupEventArgs e)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

                    services.AddSingleton<IUserSessionService, UserSessionService>();
                    services.AddSingleton<IWindowService, WindowService>();
                    services.AddSingleton<ILoggerConfig, LoggerConfig>();
                    services.AddSingleton<IEmailService, EmailService>();
                    services.AddSingleton<ITwoFactorService, TwoFactorService>();

                    services.AddTransient<IAuthService, AuthService>();
                    services.AddTransient<IPasswordHasher, PasswordHasher>();
                    services.AddTransient<IResetPasswordService, ResetPasswordService>();
                    services.AddTransient<ILogService, LogService>();

                    services.AddTransient<LoginView>();
                    services.AddTransient<ResetPasswordView>();
                    services.AddTransient<TwoFactorAuthView>();
                    services.AddTransient<AdminMainView>();

                    services.AddTransient<UsersTab>();
                    services.AddTransient<TestsTab>();
                    services.AddTransient<QuestionsTab>();
                    services.AddTransient<TestSessionsTab>();
                    services.AddTransient<AnswersTab>();
                    services.AddTransient<ResultsTab>();
                    services.AddTransient<LogsTab>();

                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<ResetPasswordViewModel>();
                    services.AddTransient<TwoFactorAuthViewModel>();
                    services.AddTransient<AdminMainViewModel>();
                    services.AddTransient<UsersTabViewModel>();
                    services.AddTransient<TestsTabViewModel>();
                    services.AddTransient<QuestionsTabViewModel>();
                    services.AddTransient<TestSessionsTabViewModel>();
                    services.AddTransient<AnswersTabViewModel>();
                    services.AddTransient<ResultsTabViewModel>();
                    services.AddTransient<LogsTabViewModel>();
                    services.AddTransient<TwoFactorCodesTab>();
                    services.AddTransient<TwoFactorCodesTabViewModel>();
                    services.AddTransient<AdminSettingsTab>();
                    services.AddTransient<AdminSettingsTabViewModel>();

                    services.AddTransient<TeacherMainView>();
                    services.AddTransient<TeacherMainViewModel>();
                    services.AddTransient<MyTestsTab>();
                    services.AddTransient<SettingsTab>();
                    services.AddTransient<ActivityLogTab>();
                    services.AddTransient<TestResultsTab>();
                    services.AddTransient<MyTestsTabViewModel>();
                    services.AddTransient<SettingsTabViewModel>();
                    services.AddTransient<ActivityLogTabViewModel>();
                    services.AddTransient<TestResultsTabViewModel>();

                    services.AddTransient<StudentMainView>();
                    services.AddSingleton<JoinTestTab>();
                    services.AddSingleton<MyResultsTab>();
                    services.AddSingleton<StatisticsTab>();
                    services.AddSingleton<StudentSettingsTab>();
                    services.AddSingleton<StudentMainViewModel>();
                    services.AddSingleton<JoinTestViewModel>();
                    services.AddSingleton<TestSessionWindow>();
                    services.AddSingleton<TestSessionViewModel>();
                    services.AddSingleton<MyResultsTabViewModel>();
                    services.AddSingleton<StudentSettingsViewModel>();

                    services.Configure<SmtpSettings>(context.Configuration.GetSection("Smtp"));
                })
                .Build();

            _host.Start();

            var loggerConfig = new LoggerConfig();
            loggerConfig.Configure();

            base.OnStartup(e);

            var loginView = _host.Services.GetRequiredService<LoginView>();
            loginView.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host is not null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
            base.OnExit(e);
        }
    }
}