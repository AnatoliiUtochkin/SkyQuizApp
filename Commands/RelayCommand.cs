using System.Windows.Input;

namespace SkyQuizApp.Commands
{
    public class RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}