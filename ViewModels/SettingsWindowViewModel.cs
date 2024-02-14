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
        private PlayersStore playersStore;

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
                if (SelectedOutAux?.ProductName == value?.ProductName || value?.ProductName == null) return;
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
                if (SelectedOut?.ProductName == value?.ProductName || value?.ProductName == null) return;
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

        public SettingsWindowViewModel(SettingsStore settingsStore, PlayersStore playersStore)
        {
            this.settingsStore = settingsStore;
            this.playersStore = playersStore;

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

        private void StopPlayback()
        {
            playersStore.StopPlayers();
            var sound = settingsStore.UserSounds.FirstOrDefault(x => x.Progress != 0);
            if (sound != null) sound.SetProgress(settingsStore, 0);
        }
    }
}
