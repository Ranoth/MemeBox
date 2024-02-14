using CommunityToolkit.Mvvm.Messaging;
using MemeBox.Messages;
using MemeBox.Models;
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
            if (MainPlayer.DeviceNumber != -1) MainPlayer.Init(new WaveChannel32(new AudioFileReader(sound.Path)));
            if (AuxPlayer.DeviceNumber != -1) AuxPlayer.Init(new WaveChannel32(new AudioFileReader(sound.Path)));
        }

        public void PlayPlayers()
        {
            MainPlayer.Play();
            AuxPlayer.Play();
            WeakReferenceMessenger.Default.Send(new PlaybackStateChangedMessage(MainPlayer.PlaybackState));
        }

        public void PausePlayers()
        {
            MainPlayer.Pause();
            AuxPlayer.Pause();
            WeakReferenceMessenger.Default.Send(new PlaybackStateChangedMessage(MainPlayer.PlaybackState));
        }

        public void ResumePlayers()
        {
            MainPlayer.Resume();
            AuxPlayer.Resume();
            WeakReferenceMessenger.Default.Send(new PlaybackStateChangedMessage(MainPlayer.PlaybackState));
        }

        public void StopPlayers()
        {
            MainPlayer.Stop();
            AuxPlayer.Stop();
            WeakReferenceMessenger.Default.Send(new PlaybackStateChangedMessage(MainPlayer.PlaybackState));
        }
    }
}
