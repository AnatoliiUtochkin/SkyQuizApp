using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using SkyQuizApp.Data;
using SkyQuizApp.Models;

namespace SkyQuizApp.ViewModels.Admin.Tabs
{
    public class QuestionsTabViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AppDbContext _context;
        public ObservableCollection<Question> Questions { get; } = new();

        public QuestionsTabViewModel(AppDbContext context)
        {
            _context = context;
            LoadQuestionsAsync();
        }

        private async void LoadQuestionsAsync()
        {
            var fromDb = await _context.Questions
                .OrderBy(q => q.QuestionID)
                .AsNoTracking()
                .ToListAsync();

            App.Current.Dispatcher.Invoke(() =>
            {
                Questions.Clear();
                foreach (var q in fromDb)
                    Questions.Add(q);
            });
        }

        public void Dispose() => _context.Dispose();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}