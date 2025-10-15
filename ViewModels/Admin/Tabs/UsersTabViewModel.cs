using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkyQuizApp.Data;
using SkyQuizApp.Enums;
using SkyQuizApp.Models;
using SkyQuizApp.Services;
using SkyQuizApp.Services.Interfaces;

namespace SkyQuizApp.ViewModels.Admin.Tabs
{
    public class UsersTabViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IUserSessionService _session;
        private readonly IPasswordHasher _passwordHasher = new PasswordHasher();
        private readonly ILogService _logger;

        public ObservableCollection<User> Users { get; } = new();
        public User? CurrentUser => _session.CurrentUser;
        public List<UserRole> Roles { get; } = Enum.GetValues(typeof(UserRole)).Cast<UserRole>().ToList();

        public UsersTabViewModel(AppDbContext context, IUserSessionService session)
        {
            _context = context;
            _session = session;

            var app = App.Current as App
                ?? throw new InvalidOperationException("Сервіси недоступні.");
            _logger = app.Services.GetRequiredService<ILogService>();

            LoadUsersAsync();
        }

        private async void LoadUsersAsync()
        {
            var usersFromDb = await _context.Users.AsNoTracking().ToListAsync();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Users.Clear();
                foreach (var u in usersFromDb)
                    Users.Add(u);
            });
        }

        public User AddUser()
        {
            var now = DateTime.UtcNow;
            var newUser = new User
            {
                Email = string.Empty,
                FullName = string.Empty,
                PasswordHash = string.Empty,
                Role = UserRole.Student,
                IsBlocked = false,
                IsTwoFactorEnabled = false,
                CreatedAt = now,
                LastLogin = now
            };
            _context.Users.Add(newUser);
            Users.Add(newUser);

            _logger.Log(LogAction.UserCreated, $"Створено нового користувача (тимчасовий) о {now}");

            return newUser;
        }

        public void DeleteUser(User user)
        {
            if (user == null || user.UserID == CurrentUser?.UserID) return;

            try
            {
                _context.Users.Attach(user);
                _context.Users.Remove(user);
                _context.SaveChanges();
                Users.Remove(user);

                _logger.Log(LogAction.UserDeleted, $"Користувача ID={user.UserID} видалено.");
            }
            catch (Exception ex)
            {
                _logger.LogError(LogAction.UserDeleted, ex, $"Помилка при видаленні користувача ID={user.UserID}");
                MessageBox.Show($"Помилка при видаленні користувача.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateUser(User user)
        {
            if (user == null) return;

            try
            {
                if (!string.IsNullOrWhiteSpace(user.PasswordHash) && !user.PasswordHash.StartsWith("$"))
                {
                    user.PasswordHash = _passwordHasher.HashPassword(user.PasswordHash);
                    _logger.Log(LogAction.PasswordChange, $"Оновлено пароль для користувача ID={user.UserID}");
                }

                if (user.UserID == 0)
                {
                    _context.Users.Add(user);
                    _logger.Log(LogAction.UserCreated, $"Додано нового користувача з email {user.Email}");
                }
                else
                {
                    var tracked = _context.ChangeTracker.Entries<User>()
                        .FirstOrDefault(e => e.Entity.UserID == user.UserID);

                    if (tracked != null)
                    {
                        tracked.CurrentValues.SetValues(user);
                    }
                    else
                    {
                        _context.Users.Attach(user);
                        var entry = _context.Entry(user);
                        entry.Property(u => u.Email).IsModified = true;
                        entry.Property(u => u.FullName).IsModified = true;
                        entry.Property(u => u.Role).IsModified = true;
                        entry.Property(u => u.IsBlocked).IsModified = true;
                        entry.Property(u => u.IsTwoFactorEnabled).IsModified = true;
                        entry.Property(u => u.PasswordHash).IsModified = true;
                    }

                    _logger.Log(LogAction.UserUpdated, $"Оновлено користувача ID={user.UserID}");
                }

                _context.SaveChanges();
                ReloadUser(user.UserID);
            }
            catch (Exception ex)
            {
                _logger.LogError(LogAction.UserUpdated, ex, $"Помилка при оновленні користувача ID={user.UserID}");
                MessageBox.Show($"Помилка при оновленні користувача.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReloadUser(int userId)
        {
            var index = Users.ToList().FindIndex(u => u.UserID == userId);
            if (index == -1) return;

            var updated = _context.Users.AsNoTracking().FirstOrDefault(u => u.UserID == userId);
            if (updated == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Users[index] = updated;
            });
        }

        public string HashPassword(string plainPassword)
        {
            return _passwordHasher.HashPassword(plainPassword);
        }

        public void Dispose() => _context.Dispose();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
