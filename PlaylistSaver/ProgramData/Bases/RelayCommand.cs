using System;
using System.Windows.Input;

namespace PlaylistSaver.ProgramData.Bases
{
    public class RelayCommand : ICommand
    {
        public RelayCommand(Action callback, Func<bool> canExecute = null)
        {
            _callback = callback;
            _canExecute = canExecute ?? (() => true);
        }

        private readonly Action _callback;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute();

        public void Execute(object parameter)
        {
            _callback?.Invoke();
        }
    }
}
