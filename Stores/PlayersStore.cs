using NAudio.Wave;

namespace MemeBox.Stores
{
    public class PlayersStore
    {
        private readonly SettingsStore settingsStore;
        public WaveOut MainPlayer { get; set; } = new();
        public WaveOut AuxPlayer { get; set; } = new();

        public PlayersStore(SettingsStore settingsStore)
        {
            this.settingsStore = settingsStore;

            UpdatePlayersSettings();

            this.settingsStore.SettingsChanged += () => UpdatePlayersSettings();
        }

        private void UpdatePlayersSettings()
        {
            MainPlayer.DeviceNumber = settingsStore.AudioOutCapabilities
                 .IndexOf(settingsStore.AudioOutCapabilities
                 .Find(x => x.ProductName == settingsStore.Settings.SetOut));
            MainPlayer.Volume = settingsStore.Settings.Volume;

            AuxPlayer.DeviceNumber = settingsStore.AudioOutCapabilities
                .IndexOf(settingsStore.AudioOutCapabilities
                .Find(x => x.ProductName == settingsStore.Settings.SetOutAux));
            AuxPlayer.Volume = settingsStore.Settings.Volume;
        }
    }
}
