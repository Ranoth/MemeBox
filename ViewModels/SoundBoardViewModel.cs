using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MemeBox.Commands;
using MemeBox.Models;
using MemeBox.Stores;
using MemeBox.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using NAudio.Wave;
using System.IO;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Forms;
using GlobalKeyboardHooker;

namespace MemeBox.ViewModels
{
    public partial class SoundBoardViewModel : ViewModelBase
    {
        private SettingsStore settingsStore;
        private PlayersStore playersStore;
        private Sound removedSound;

        public bool KeyBindChanging { get; set; } = false;
        public PlaySoundCommand PlaySoundCommand { get; set; }
        public BindingList<Sound> Sounds { get; private set; } = new();

        public SoundBoardViewModel(SettingsStore settingsStore, PlayersStore playersStore)
        {
            this.settingsStore = settingsStore;
            this.playersStore = playersStore;
            UpdateSoundsList();
            RegisterMessengers();

            PlaySoundCommand = new PlaySoundCommand(PlaySound, CanPlaySound);
        }

        private void RegisterMessengers()
        {
            settingsStore.UserSounds.ListChanged += (s, e) =>
            {
                UpdateSoundsList();
                UpdateUserSoundsXml(s, e);
            };
        }
        private void UpdateSoundsList()
        {
            Sounds = settingsStore.UserSounds;
        }

        public void PlaySound(object soundName)
        {
            var sound = Sounds.FirstOrDefault(x => x.Name == (string)soundName);

            try
            {
                playersStore.MainPlayer.Pause();
                playersStore.MainPlayer = InitPlayer(playersStore.MainPlayer, sound);
                playersStore.MainPlayer.Play();

                playersStore.AuxPlayer.Pause();
                playersStore.AuxPlayer = InitPlayer(playersStore.AuxPlayer, sound);
                if (playersStore.AuxPlayer.DeviceNumber != -1) playersStore.AuxPlayer.Play();
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(COMException)) System.Windows.MessageBox.Show("File format unsupported by the application, please try any audio file format supported by " +
                "Windows Media Foundation");
            }
        }

        public bool CanPlaySound(object? soundName)
        {
            try
            {
                var sound = Sounds.SingleOrDefault(x => x.Name == ((string)soundName));
                if (File.Exists(sound?.Path ?? String.Empty))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(InvalidOperationException))
                {
                    RemoveSound((string)soundName);
                    System.Windows.MessageBox.Show($"Cannot add duplicates ({soundName})");
                    return true;
                }
                else
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    return false;
                }
            }
        }


        private WaveOut InitPlayer(WaveOut player, Sound sound)
        {
            if (player.DeviceNumber != -1) player.Init(new WaveChannel32(new AudioFileReader(sound.Path)));
            return player;
        }

        private void UpdateUserSoundsXml(object? sender, ListChangedEventArgs e)
        {
            var list = sender as BindingList<Sound>;
            var changeType = e.ListChangedType;
            var xDoc = XDocument.Load(settingsStore.UserSoundsFilePath);
            var sound = list[e.NewIndex];

            if (changeType == ListChangedType.ItemChanged)
            {
                var xQuery = from elements in xDoc.Descendants("UserSounds").Elements("UserSound")
                             where elements.Attribute(nameof(sound.Name)).Value == sound.Name
                             select elements;

                foreach (var element in xQuery)
                {
                    element.SetAttributeValue(nameof(sound.Name), sound.Name);
                    element.SetAttributeValue(nameof(sound.Path), sound.Path);
                    element.Element(nameof(sound.HotKey)).SetAttributeValue(nameof(sound.HotKey.Key), sound.HotKey.Key.ToString());
                    element.Element(nameof(sound.HotKey)).SetAttributeValue(nameof(sound.HotKey.Modifiers), sound.HotKey.Modifiers.ToString());
                }

                xDoc.Save(settingsStore.UserSoundsFilePath);
            }
            else if (changeType == ListChangedType.ItemDeleted)
            {
                xDoc.Descendants("UserSounds").Elements("UserSound")
                .Where(x => x.Attribute(nameof(sound.Name)).Value == removedSound.Name).FirstOrDefault().Remove();

                xDoc.Save(settingsStore.UserSoundsFilePath);
            }
            else if (changeType == ListChangedType.ItemAdded)
            {
                xDoc.Element("UserSounds").Add(
                    new XElement("UserSound",
                        new XAttribute(nameof(sound.Name), sound.Name),
                        new XAttribute(nameof(sound.Path), sound.Path),
                            new XElement(nameof(sound.HotKey),
                                new XAttribute(nameof(sound.HotKey.Key), sound.HotKey.Key.ToString()),
                                new XAttribute(nameof(sound.HotKey.Modifiers), sound.HotKey.Modifiers.ToString()))));

                xDoc.Save(settingsStore.UserSoundsFilePath);
            }
        }

        [RelayCommand]
        private void RemoveButton(string soundName)
        {
            if (System.Windows.Forms.MessageBox.Show("Do you truly wish to remove this button ?", String.Empty, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                RemoveSound(soundName);
            }
        }
        private void RemoveSound(string soundName)
        {
            var sound = Sounds.LastOrDefault(x => x.Name == soundName);

            removedSound = sound;
            settingsStore.UserSounds.Remove(sound);
            removedSound = null;
        }

        [RelayCommand]
        private void SetKeyBind(string soundName)
        {
            KeyBindChanging = true;
            var keyBindDialog = new KeysBindsWindow(settingsStore, Sounds.SingleOrDefault(x => x.Name == soundName));
            keyBindDialog.ShowDialog();
            KeyBindChanging = false;
        }

        [RelayCommand]
        private void ClearKeyBind(string soundName)
        {
            var sound = Sounds.SingleOrDefault(x => x.Name == soundName);
            if (sound.HotKey.Key != Key.None)
            {
                if (System.Windows.Forms.MessageBox.Show("Do you truly wish to clear this sound's bound key ?", String.Empty, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    sound.HotKey = new HotKey(Key.None, ModifierKeys.None);
                }
            }
        }

        [RelayCommand]
        private void AddSoundWindow()
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            var sound = new Sound { Name = openFileDialog.SafeFileName, Path = openFileDialog.FileName };
            sound.Name = sound.Name.Remove(sound.Name.LastIndexOf('.'));

            settingsStore.UserSounds.Add(sound);
        }
    }
}
