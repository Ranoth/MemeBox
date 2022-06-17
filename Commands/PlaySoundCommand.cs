using Commands;

namespace MemeBox.Commands
{
    public class PlaySoundCommand : CommandBase
    {
        private Action<object> execute;
        public PlaySoundCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new NullReferenceException(nameof(execute));

            this.execute = execute;
            base.canExecute = canExecute;
        }

        public PlaySoundCommand(Action<object> execute) : this (execute, null)
        {

        }

        public override void Execute(object? parameter)
        {
            execute.Invoke(parameter);
        }
    }
}
