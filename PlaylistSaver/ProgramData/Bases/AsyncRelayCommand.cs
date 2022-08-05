using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlaylistSaver.ProgramData.Bases
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, Task> _parameterExecute;
        private readonly Func<Task> _execute;
        private readonly Func<object, bool> _canExecute;

        private long isExecuting;

        public AsyncRelayCommand(Func<object, Task> parameterExecute, Func<object, bool> canExecute = null)
        {
            this._parameterExecute = parameterExecute;
            this._canExecute = canExecute ?? (o => true);
        }

        public AsyncRelayCommand(Func<Task> execute, Func<object, bool> canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute ?? (o => true);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public static void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public bool CanExecute(object parameter)
        {
            if (Interlocked.Read(ref isExecuting) != 0)
                return false;

            return _canExecute(parameter);
        }

        public async void Execute(object parameter)
        {
            Interlocked.Exchange(ref isExecuting, 1);
            RaiseCanExecuteChanged();

            try
            {
                if (_execute != null)
                    await _execute();
                if (_parameterExecute != null)
                    await _parameterExecute(parameter);
            }
            finally
            {
                Interlocked.Exchange(ref isExecuting, 0);
                RaiseCanExecuteChanged();
            }
        }
    }
}
