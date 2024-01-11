using WPFUtilsBox.Commands;
using MemeBox.Stores;
using MemeBox.ViewModels;

namespace MemeBox.Commands
{
    public class NavigateCommand : CommandBase
    {
        private Func<ViewModelBase> execute;
        private NavigationStore navigationStore;
        public NavigateCommand(Func<ViewModelBase> execute, NavigationStore navigationStore)
        {
            this.execute = execute;
            this.navigationStore = navigationStore;
        }

        public override void Execute(object? parameter)
        {
            navigationStore.CurrentViewModel = execute();
        }
    }
}
