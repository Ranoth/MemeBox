using MemeBox.Stores;
using MemeBox.ViewModels;
using NAudio.Wave;
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

        // stop the selected combo box value from changing if the user selects the same device for both main and aux
        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is not System.Windows.Controls.ComboBox comboBox)
                return;

            if (comboBox.Name == "MainAudioOutComboBox" && AuxAudioOutComboBox.SelectedItem != null)
            {
                if (((WaveOutCapabilities)comboBox.SelectedItem).ProductName == ((WaveOutCapabilities)AuxAudioOutComboBox.SelectedItem).ProductName)
                {
                    MessageBox.Show("You can't set the same device for both Main and Aux", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    comboBox.SelectedItem = e.RemovedItems[0];
                }
            }
            else if (comboBox.Name == "AuxAudioOutComboBox" && MainAudioOutComboBox.SelectedItem != null)
            {
                if (((WaveOutCapabilities)comboBox.SelectedItem).ProductName == ((WaveOutCapabilities)MainAudioOutComboBox.SelectedItem).ProductName)
                {
                    MessageBox.Show("You can't set the same device for both Main and Aux", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    comboBox.SelectedItem = e.RemovedItems[0];
                }
            }
        }
    }
}
