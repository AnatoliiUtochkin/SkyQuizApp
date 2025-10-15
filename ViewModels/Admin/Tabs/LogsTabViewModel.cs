using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using SkyQuizApp.Data;
using SkyQuizApp.Models;

namespace SkyQuizApp.ViewModels.Admin.Tabs
{
    public class LogsTabViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AppDbContext _context;
        public ObservableCollection<Log> Logs { get; } = new();

        public LogsTabViewModel(AppDbContext context)
        {
            _context = context;
            LoadLogsAsync();
        }

        private async void LoadLogsAsync()
        {
            var logs = await _context.Logs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            Logs.Clear();
            foreach (var log in logs)
                Logs.Add(log);
        }

        public void Dispose() => _context.Dispose();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}