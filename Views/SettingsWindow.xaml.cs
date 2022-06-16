using MemeBox.Stores;
using MemeBox.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
