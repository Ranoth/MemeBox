using MemeBox.Models;
using NAudio.Wave;
using System.ComponentModel;
using System.Windows;
using MemeBox.Stores;
using WPFUtilsBox.EasyXml;

namespace MemeBox.ViewModels
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        private SettingsStore settingsStore;
        private Settings settings;
        private PlayersStore playersStore;
        private readonly MainWindowViewModel mainWindowViewModel;

        public List<WaveOutCapabilities> AudioOutDevicesList => settingsStore.AudioOutCapabilities;
        private WaveOutCapabilities? selectedOut = new();
        private WaveOutCapabilities? selectedOutAux = new();
        private float volume = 1;

        public WaveOutCapabilities? SelectedOut
        {
            get => selectedOut;
            set
            {
                SetProperty(ref selectedOut, value);
                settings.SetOut = SelectedOut?.ProductName ?? "None";

                StopPlayback();

                UpdateSettingsStore();
            }
        }
        public WaveOutCapabilities? SelectedOutAux
        {
            get => selectedOutAux;
            set
            {
                SetProperty(ref selectedOutAux, value);
                settings.SetOutAux = SelectedOutAux?.ProductName ?? "None";

                StopPlayback();

                UpdateSettingsStore();
            }
        }
        public float Volume
        {
            get => volume;
            set
            {
                SetProperty(ref volume, value);
                settings.Volume = Volume;

                UpdateSettingsStore();
            }
        }

        public SettingsWindowViewModel(SettingsStore settingsStore, PlayersStore playersStore, MainWindowViewModel mainWindowViewModel)
        {
            this.settingsStore = settingsStore;
            this.settings = this.settingsStore.Settings;
            this.playersStore = playersStore;
            this.mainWindowViewModel = mainWindowViewModel;

            selectedOut = AudioOutDevicesList.Find(x =>
            {
                return this.settings.SetOut?.Contains(x.ProductName) ?? false && this.settings.SetOut != "None";
            });

            selectedOutAux = AudioOutDevicesList.Find(x =>
            {
                return this.settings.SetOutAux?.Contains(x.ProductName) ?? false && this.settings.SetOutAux != "None";
            });

            volume = this.settings.Volume;

        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            try
            {
                UpdateSettingsStore();
                XmlBroker.XmlDataWriter(settingsStore.Settings, settingsStore.SettingsFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.ToString());
            }

            mainWindowViewModel.SettingsWindow = null;
        }

        private void UpdateSettingsStore()
        {
            settingsStore.Settings = settings;
        }

        private void StopPlayback()
        {
            playersStore.MainPlayer.Pause();
            playersStore.AuxPlayer.Pause();
        }
    }
}
