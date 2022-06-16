using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MemeBox.Commands
{
    public abstract class CommandBase : ICommand
    {
        public Predicate<object> canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public virtual bool CanExecute(object? parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }

        public abstract void Execute(object? parameter);
    }
}
