using CommunityToolkit.Mvvm.Messaging;
using MemeBox.Models;
using NAudio.Utils;
using NAudio.Wave;
using System;
using WPFUtilsBox.EasyXml;
using MessageBox = System.Windows.Forms.MessageBox;

namespace MemeBox.Stores
{
    public class PlayersStore
    {
        private SettingsStore settingsStore;
        public WaveOut MainPlayer { get; set; } = new();
        public WaveOut AuxPlayer { get; set; } = new();
        public AudioFileReader? MainPlayerAudioFileReader;
        public AudioFileReader? AuxPlayerAudioFileReader;
        public event Action? PlaybackStateChanged;
        public bool WasPaused { get; set; } = false;
        public PlayersStore(SettingsStore settingsStore)
        {
            this.settingsStore = settingsStore;

            UpdatePlayersSettings();

            this.settingsStore.Settings.PropertyChanged += (s, e) => UpdatePlayersSettings();
        }

        // ToDo: Find why MainPlayer.Volume also sets AuxPlayer.Volume but only some times and why settings AuxPlayer.Volume first functionally fixes it
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

            try
            {
                XmlBroker.XmlDataWriter(settingsStore.Settings, settingsStore.SettingsFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.ToString());
            }
        }

        public void InitPlayers(Sound sound)
        {
            if (MainPlayer.DeviceNumber != -1)
            {
                if (MainPlayerAudioFileReader != null) MainPlayerAudioFileReader.Dispose();
                MainPlayerAudioFileReader = new AudioFileReader(sound.Path);
                MainPlayer.Init(new WaveChannel32(MainPlayerAudioFileReader));
            }
            if (AuxPlayer.DeviceNumber != -1)
            {
                if (AuxPlayerAudioFileReader != null) AuxPlayerAudioFileReader.Dispose();
                AuxPlayerAudioFileReader = new AudioFileReader(sound.Path);
                AuxPlayer.Init(new WaveChannel32(AuxPlayerAudioFileReader));
            }
        }

        public TimeSpan GetMainPlayerPosition()
        {
            return MainPlayer.GetPositionTimeSpan();
        }

        public void PlayPlayers()
        {
            MainPlayer.Play();
            AuxPlayer.Play();
            WasPaused = false;
            PlaybackStateChanged?.Invoke();
        }

        public void PausePlayers()
        {
            MainPlayer.Pause();
            AuxPlayer.Pause();
            PlaybackStateChanged?.Invoke();
        }

        public void StopPlayers()
        {
            MainPlayer.Stop();
            AuxPlayer.Stop();
            WasPaused = false;
            MainPlayerAudioFileReader?.Dispose();
            AuxPlayerAudioFileReader?.Dispose();
            PlaybackStateChanged?.Invoke();
        }
    }
}
