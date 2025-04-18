﻿using CommunityToolkit.Mvvm.Messaging;
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
            AuxPlayer.Volume = settingsStore.Settings.AdjustedVolumeAux;

            MainPlayer.DeviceNumber = settingsStore.AudioOutCapabilities
                 .IndexOf(settingsStore.AudioOutCapabilities
                 .FirstOrDefault(x => x.ProductName == settingsStore.Settings.SetOut));
            MainPlayer.Volume = settingsStore.Settings.AdjustedVolumeMain;

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
            if (MainPlayerAudioFileReader == null || AuxPlayerAudioFileReader == null) return;
            MainPlayer.Play();
            if (settingsStore.Settings.SetOutAux != "None") AuxPlayer.Play();
            WasPaused = false;
            PlaybackStateChanged?.Invoke();
        }

        public void PausePlayers()
        {
            if (MainPlayerAudioFileReader == null || AuxPlayerAudioFileReader == null) return;
            if (MainPlayer == null || AuxPlayer == null) return;
            MainPlayer.Pause();
            if (settingsStore.Settings.SetOutAux != "None") AuxPlayer.Pause();
            PlaybackStateChanged?.Invoke();
        }

        public void StopPlayers()
        {
            MainPlayer.Stop();
            if (settingsStore.Settings.SetOutAux != "None") AuxPlayer.Stop();
            WasPaused = false;
            MainPlayerAudioFileReader?.Dispose();
            if (settingsStore.Settings.SetOutAux != "None") AuxPlayerAudioFileReader?.Dispose();
            PlaybackStateChanged?.Invoke();
        }

        public void InvokePlaybackStateChanged()
        {
            PlaybackStateChanged?.Invoke();
        }
    }
}
