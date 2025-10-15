using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using SkyQuizApp.Data;
using SkyQuizApp.Models;

namespace SkyQuizApp.ViewModels.Admin.Tabs
{
    public class ResultsTabViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AppDbContext _context;
        public ObservableCollection<Result> Results { get; } = new();

        public ResultsTabViewModel(AppDbContext context)
        {
            _context = context;
            LoadResultsAsync();
        }

        private async void LoadResultsAsync()
        {
            var results = await _context.Results
                .OrderBy(r => r.ResultID)
                .ToListAsync();

            Results.Clear();
            foreach (var r in results)
                Results.Add(r);
        }

        public void Dispose() => _context.Dispose();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}