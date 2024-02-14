using MemeBox.Models;
using MemeBox.Stores;
using MemeBox.ViewModels;
using System.Windows;

namespace MemeBox.Views
{
    /// <summary>
    /// Interaction logic for KeysBindsWindow.xaml
    /// </summary>
    public partial class KeysBindsWindow : Window
    {
        private KeyBindsWindowViewModel viewModel;
        public KeysBindsWindow(SettingsStore settingsStore, Sound soundToUpdate)
        {
            InitializeComponent();
            viewModel = new KeyBindsWindowViewModel(settingsStore, this, soundToUpdate);
            DataContext = viewModel;
            KeyUp += viewModel.OnKeyUp;
        }
        public KeysBindsWindow(SettingsStore settingsStore, string buttonName)
        {
            InitializeComponent();
            viewModel = new KeyBindsWindowViewModel(settingsStore, this, buttonName);
            DataContext = viewModel;
            KeyUp += viewModel.OnKeyUp;
        }
    }
}
