using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using SkyQuizApp.Data;
using SkyQuizApp.Models;

namespace SkyQuizApp.ViewModels.Admin.Tabs
{
    public class AnswersTabViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AppDbContext _context;
        public ObservableCollection<Answer> Answers { get; } = new();

        public AnswersTabViewModel(AppDbContext context)
        {
            _context = context;
            LoadAnswersAsync();
        }

        private async void LoadAnswersAsync()
        {
            var answers = await _context.Answers
                .OrderBy(a => a.AnswerID)
                .AsNoTracking()
                .ToListAsync();

            App.Current.Dispatcher.Invoke(() =>
            {
                Answers.Clear();
                foreach (var a in answers)
                    Answers.Add(a);
            });
        }

        public void Dispose() => _context.Dispose();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}