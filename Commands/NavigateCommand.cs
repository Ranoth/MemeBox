using MemeBox.Stores;
using MemeBox.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
