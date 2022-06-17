using MemeBox.Stores;
using MemeBox.ViewModels;
using System.Windows;

namespace MemeBox.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(SettingsStore settingsStore, PlayersStore playersStore, MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            var viewModel = new SettingsWindowViewModel(settingsStore, playersStore, mainWindowViewModel);
            DataContext = viewModel;
            Closing += viewModel.OnWindowClosing;
        }
    }
}
