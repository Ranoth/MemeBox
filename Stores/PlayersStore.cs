using NAudio.Wave;
using System.Windows.Forms;

namespace MemeBox.Stores
{
    public class PlayersStore
    {
        private SettingsStore settingsStore;
        public WaveOut MainPlayer { get; set; } = new();
        public WaveOut AuxPlayer { get; set; } = new();

        public PlayersStore(SettingsStore settingsStore)
        {
            this.settingsStore = settingsStore;

            UpdatePlayersSettings();

            this.settingsStore.Settings.PropertyChanged += (s, e) => UpdatePlayersSettings();
        }

        // ToDo: Find why MainPlayer.Volume also sets AuxPlayer.Volume but only some times and why settings AuxPlayer.Volume first sort of fixes it
        private void UpdatePlayersSettings()
        {
            AuxPlayer.DeviceNumber = settingsStore.AudioOutCapabilities
                .IndexOf(settingsStore.AudioOutCapabilities
                .FirstOrDefault(x => x.ProductName == settingsStore.Settings.SetOutAux));
            AuxPlayer.Volume = settingsStore.Settings.VolumeAux;

            MainPlayer.DeviceNumber = settingsStore.AudioOutCapabilities
                 .IndexOf(settingsStore.AudioOutCapabilities
                 .FirstOrDefault(x => x.ProductName == settingsStore.Settings.SetOut));
            MainPlayer.Volume = settingsStore.Settings.VolumeMain;
        }
    }
}
