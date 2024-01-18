using MemeBox.Models;
using NAudio.Wave;
using System.ComponentModel;
using System.Windows;
using MemeBox.Stores;
using WPFUtilsBox.EasyXml;
using System.Windows.Threading;

namespace MemeBox.ViewModels
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        private SettingsStore settingsStore;
        private PlayersStore playersStore;
        private readonly MainWindowViewModel mainWindowViewModel;

        public List<WaveOutCapabilities> AudioOutDevicesList => settingsStore.AudioOutCapabilities;
        private WaveOutCapabilities? selectedOut = new();
        private WaveOutCapabilities? selectedOutAux = new();
        private float volumeMain = 1;
        private float volumeAux = 1;

        public WaveOutCapabilities? SelectedOut
        {
            get => selectedOut;
            set
            {
                if (SelectedOutAux?.ProductName == value?.ProductName) return;
                SetProperty(ref selectedOut, value);
                settingsStore.Settings.SetOut = SelectedOut?.ProductName ?? "None";

                StopPlayback();
            }
        }
        public WaveOutCapabilities? SelectedOutAux
        {
            get => selectedOutAux;
            set
            {
                if (SelectedOut?.ProductName == value?.ProductName) return;
                SetProperty(ref selectedOutAux, value);
                settingsStore.Settings.SetOutAux = SelectedOutAux?.ProductName ?? "None";

                StopPlayback();
            }
        }

        public float VolumeMain
        {
            get => volumeMain;
            set
            {
                SetProperty(ref volumeMain, value);
                settingsStore.Settings.VolumeMain = volumeMain;
            }
        }
        public float VolumeAux
        {
            get => volumeAux;
            set
            {
                SetProperty(ref volumeAux, value);
                settingsStore.Settings.VolumeAux = volumeAux;
            }
        }

        public SettingsWindowViewModel(SettingsStore settingsStore, PlayersStore playersStore, MainWindowViewModel mainWindowViewModel)
        {
            this.settingsStore = settingsStore;
            this.playersStore = playersStore;
            this.mainWindowViewModel = mainWindowViewModel;

            selectedOut = AudioOutDevicesList.Find(x =>
            {
                return this.settingsStore.Settings.SetOut?.Contains(x.ProductName) ?? false && this.settingsStore.Settings.SetOut != "None";
            });

            selectedOutAux = AudioOutDevicesList.Find(x =>
            {
                return this.settingsStore.Settings.SetOutAux?.Contains(x.ProductName) ?? false && this.settingsStore.Settings.SetOutAux != "None";
            });

            volumeMain = settingsStore.Settings.VolumeMain;
            volumeAux = settingsStore.Settings.VolumeAux;
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            try
            {
                XmlBroker.XmlDataWriter(settingsStore.Settings, settingsStore.SettingsFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.ToString());
            }

            mainWindowViewModel.SettingsWindow = null;
        }

        private void StopPlayback()
        {
            playersStore.MainPlayer.Pause();
            playersStore.AuxPlayer.Pause();
        }
    }
}
