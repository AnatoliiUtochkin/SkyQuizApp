using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using SkyQuizApp.Data;
using SkyQuizApp.Models;

namespace SkyQuizApp.ViewModels.Admin.Tabs
{
    public class TestSessionsTabViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AppDbContext _context;

        public ObservableCollection<TestSession> TestSessions { get; } = new();

        public TestSessionsTabViewModel(AppDbContext context)
        {
            _context = context;
            LoadTestSessionsAsync();
        }

        private async void LoadTestSessionsAsync()
        {
            var sessions = await _context.TestSessions
                .OrderBy(s => s.TestSessionID)
                .ToListAsync();

            TestSessions.Clear();
            foreach (var session in sessions)
                TestSessions.Add(session);
        }

        public void Dispose() => _context.Dispose();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}