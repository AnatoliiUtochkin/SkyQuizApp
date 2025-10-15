using System.Collections.ObjectModel;
using System.ComponentModel;
using SkyQuizApp.Models;

namespace SkyQuizApp.ViewModels.Student
{
    public class MatchingPair : INotifyPropertyChanged
    {
        public Answer Left { get; set; } = null!;
        public ObservableCollection<Answer> Options { get; set; } = new();

        private Answer? _selected;

        public Answer? Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
                }
            }
        }

        public void SelectByAnswerId(int answerId)
        {
            Selected = Options.FirstOrDefault(a => a.AnswerID == answerId);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}