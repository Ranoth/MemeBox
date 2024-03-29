﻿using MemeBox.Stores;
using MemeBox.ViewModels;
using NAudio.Wave;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxImage = System.Windows.Forms.MessageBoxIcon;
using MessageBoxButton = System.Windows.Forms.MessageBoxButtons;

namespace MemeBox.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(SettingsStore settingsStore, PlayersStore playersStore, MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            var viewModel = new SettingsWindowViewModel(settingsStore, playersStore);
            DataContext = viewModel;
            Closing += (s, e) => mainWindowViewModel.SettingsWindow = null;
        }

        // stop the selected combo box value from changing if the user selects the same device for both main and aux
        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is not System.Windows.Controls.ComboBox comboBox)
                return;

            if (comboBox.Name == "MainAudioOutComboBox" && AuxAudioOutComboBox.SelectedItem != null && comboBox.SelectedItem != null)
            {
                if (((WaveOutCapabilities)comboBox.SelectedItem).ProductName == ((WaveOutCapabilities)AuxAudioOutComboBox.SelectedItem).ProductName)
                {
                    MessageBox.Show("You can't set the same device for both Main and Aux", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (e.RemovedItems.Count > 0) comboBox.SelectedItem = e.RemovedItems[0];
                    else comboBox.SelectedItem = null;
                }
            }
            else if (comboBox.Name == "AuxAudioOutComboBox" && MainAudioOutComboBox.SelectedItem != null && comboBox.SelectedItem != null)
            {
                if (((WaveOutCapabilities)comboBox.SelectedItem).ProductName == ((WaveOutCapabilities)MainAudioOutComboBox.SelectedItem).ProductName)
                {
                    MessageBox.Show("You can't set the same device for both Main and Aux", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (e.RemovedItems.Count > 0) comboBox.SelectedItem = e.RemovedItems[0];
                    else comboBox.SelectedItem = null;
                }
            }
        }
    }
}
