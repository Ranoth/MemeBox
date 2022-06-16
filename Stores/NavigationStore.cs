using MemeBox.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemeBox.Stores
{
    public class NavigationStore
    {
        private ViewModelBase currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => currentViewModel;
            set
            {
                currentViewModel = value;
                CurrentViewModelChanged?.Invoke();
            }
        }

        public event Action? CurrentViewModelChanged;
        public List<ViewModelBase> ViewModelsSaved { get; set; } = new List<ViewModelBase>();
    }
}
