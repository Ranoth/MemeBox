using CommunityToolkit.Mvvm.Input;
using MemeBox.Commands;
using MemeBox.Models;
using MemeBox.Stores;
using MemeBox.Views;
using System.ComponentModel;
using NAudio.Wave;
using System.IO;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxImage = System.Windows.Forms.MessageBoxIcon;
using MessageBoxButton = System.Windows.Forms.MessageBoxButtons;
using MessageBoxResult = System.Windows.Forms.DialogResult;
using WPFUtilsBox.HotKeyer;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using NAudio.Utils;
using System.Windows.Threading;
using System;
using System.Diagnostics.Tracing;
using System.Windows;

namespace MemeBox.ViewModels
{
    public partial class SoundBoardViewModel : ViewModelBase
    {
        private SettingsStore settingsStore;
        private PlayersStore playersStore;
        private Sound? removedSound;
        private string? searchText;
        private AudioFileReader? playingSoundFileReader;

        [ObservableProperty]
        private BindingList<Sound> displayedSounds;
        [ObservableProperty]
        private bool allowDrop = true;

        public bool KeyBindChanging { get; set; } = false;
        public PlaySoundCommand PlaySoundCommand { get; set; }
        public BindingList<Sound> Sounds { get; private set; } = new();
        public string? SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                SetProperty(ref searchText, value);
                SearchSounds(SearchText);
            }
        }
        public SoundBoardViewModel(SettingsStore settingsStore, PlayersStore playersStore)
        {
            this.settingsStore = settingsStore;
            this.playersStore = playersStore;
            RegisterMessengers();

            Sounds = settingsStore.UserSounds;
            DisplayedSounds = new(Sounds);

            PlaySoundCommand = new PlaySoundCommand(PlaySound, CanPlaySound);

            UpdateProgress();
        }

        [RelayCommand]
        private void DropFile(DragEventArgs args)
        {
            string?[] filePath = (string?[])args.Data.GetData("FileNameW");
            Sound sound = new Sound { Name = Path.GetFileNameWithoutExtension(filePath[0]), Path = filePath[0] };
            Sounds.Add(sound);
            CanPlaySound(sound.Name);
        }
        private async void UpdateProgress()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if (playingSoundFileReader != null && playersStore.MainPlayer.PlaybackState == PlaybackState.Playing)
                    {
                        var progress = (int)(playersStore.MainPlayer?.GetPositionTimeSpan().TotalSeconds / playingSoundFileReader.TotalTime.TotalSeconds * 1000);

                        var sound = Sounds.FirstOrDefault(x => x.Name == Path.GetFileNameWithoutExtension(playingSoundFileReader.FileName));
                        if (sound != null && sound.Progress != progress && progress < 1000) sound.SetProgress(settingsStore, progress);

                        if (progress > 999 || playersStore.MainPlayer.PlaybackState != PlaybackState.Playing) sound.SetProgress(settingsStore, 0);
                    }
                    Thread.Sleep(50);
                }
            });
        }

        private void SearchSounds(string? target)
        {
            if (target == null || target == string.Empty)
            {
                DisplayedSounds = new BindingList<Sound>(Sounds);
                return;
            }
            DisplayedSounds = new BindingList<Sound>(Sounds.Where(x => x.Name.ToLower().Contains(target.ToLower())).ToList());
        }

        private void RegisterMessengers()
        {
            settingsStore.SettingsWindowOpenChanged += () => AllowDrop = settingsStore.AllowDrop;
            settingsStore.UserSounds.ListChanged += (s, e) =>
            {
                UpdateUserSoundsXml(s, e);
                SearchSounds(SearchText);
            };
        }

        public void PlaySound(object soundName)
        {
            var sound = Sounds.FirstOrDefault(x => x.Name == (string)soundName);

            try
            {
                playingSoundFileReader = new AudioFileReader(sound.Path);
                foreach (var item in Sounds.Where(x => x.Name != Path.GetFileNameWithoutExtension(playingSoundFileReader.FileName))) item.SetProgress(settingsStore, 0);

                playersStore.MainPlayer.Pause();
                playersStore.MainPlayer = InitPlayer(playersStore.MainPlayer, sound);
                playersStore.MainPlayer.Play();

                playersStore.AuxPlayer.Pause();
                playersStore.AuxPlayer = InitPlayer(playersStore.AuxPlayer, sound);
                if (playersStore.AuxPlayer.DeviceNumber != -1) playersStore.AuxPlayer.Play();
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(COMException))
                {
                    MessageBox.Show("File format unsupported by the application, please try any audio file format supported by " +
                                    "Windows Media Foundation");
                    sound.Path = string.Empty;
                }
            }
        }

        public bool CanPlaySound(object? soundName)
        {
            try
            {
                var sound = Sounds.SingleOrDefault(x => x.Name == ((string)soundName));
                new AudioFileReader(sound?.Path ?? String.Empty);
                if (File.Exists(sound?.Path ?? String.Empty)) return true;
                else return false;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(InvalidOperationException))
                {
                    AllowDrop = false;
                    RemoveSound((string)soundName);
                    System.Windows.MessageBox.Show($"Cannot add duplicates ({soundName})");
                    AllowDrop = true;
                    return true;
                }
                else if (ex.GetType() == typeof(NullReferenceException)) return false;
                else if (ex.GetType() == typeof(COMException))
                {
                    AllowDrop = false;
                    RemoveSound((string)soundName);
                    System.Windows.MessageBox.Show("File format unsupported by the application, please try any audio file format supported by " +
                                                   "Windows Media Foundation");
                    AllowDrop = true;
                    return false;
                }
                else
                {
                    //System.Windows.MessageBox.Show(ex.Message);
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

            switch (changeType)
            {
                case ListChangedType.ItemChanged:
                    {
                        var sound = list[e.NewIndex];
                        var xQuery = from elements in xDoc.Descendants("UserSounds").Elements("UserSound")
                                     where elements.Attribute(nameof(sound.Name)).Value == sound.Name
                                     select elements;

                        foreach (var element in xQuery)
                        {
                            element.SetAttributeValue(nameof(sound.Name), sound.Name);
                            element.SetAttributeValue(nameof(sound.Path), sound.Path);
                            element.Element(nameof(sound.HotKey))
                                    .SetAttributeValue(nameof(sound.HotKey.Key), sound.HotKey.Key.ToString());
                            element.Element(nameof(sound.HotKey))
                                    .SetAttributeValue(nameof(sound.HotKey.Modifiers), sound.HotKey.Modifiers.ToString());
                        }

                        xDoc.Save(settingsStore.UserSoundsFilePath);
                        break;
                    }

                case ListChangedType.ItemDeleted:
                    {
                        Sound _;
                        xDoc.Descendants("UserSounds").Elements("UserSound")
                            .FirstOrDefault(x => x.Attribute(nameof(_.Name)).Value == removedSound.Name).Remove();

                        xDoc.Save(settingsStore.UserSoundsFilePath);
                        break;
                    }

                case ListChangedType.ItemAdded:
                    {
                        var sound = list[e.NewIndex];
                        xDoc.Element("UserSounds").Add(
                            new XElement("UserSound",
                                new XAttribute(nameof(sound.Name), sound.Name),
                                new XAttribute(nameof(sound.Path), sound.Path),
                                    new XElement(nameof(sound.HotKey),
                                        new XAttribute(nameof(sound.HotKey.Key), sound.HotKey.Key.ToString()),
                                        new XAttribute(nameof(sound.HotKey.Modifiers), sound.HotKey.Modifiers.ToString()))));

                        xDoc.Save(settingsStore.UserSoundsFilePath);
                        break;
                    }
                case ListChangedType.Reset:
                    {
                        xDoc.Element("UserSounds").Remove();
                        xDoc.Add(new XElement("UserSounds"));
                        xDoc.Save(settingsStore.UserSoundsFilePath);
                        break;
                    }
            }
        }

        [RelayCommand]
        private void RemoveButton(string soundName)
        {
            AllowDrop = false;
            if (MessageBox.Show($"Do you truly wish to remove {soundName} ?", "Remove Button", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                RemoveSound(soundName);
                playersStore.MainPlayer.Pause();
                playersStore.AuxPlayer.Pause();
                var sound = settingsStore.UserSounds.FirstOrDefault(x => x.Progress != 0);
                if (sound != null) sound.SetProgress(settingsStore, 0);
            }
            AllowDrop = true;
        }
        private void RemoveSound(string soundName)
        {
            removedSound = Sounds.LastOrDefault(x => x.Name == soundName);
            Sounds.Remove(removedSound);
            removedSound = null;
        }

        [RelayCommand]
        private void SetKeyBind(string soundName)
        {
            KeyBindChanging = true;
            AllowDrop = false;
            var keyBindDialog = new KeysBindsWindow(settingsStore, Sounds.SingleOrDefault(x => x.Name == soundName));
            keyBindDialog.ShowDialog();
            KeyBindChanging = false;
            AllowDrop = true;
        }

        [RelayCommand]
        private void ClearKeyBind(string soundName)
        {
            AllowDrop = false;
            var sound = Sounds.SingleOrDefault(x => x.Name == soundName);
            if (sound.HotKey.Key == Key.None) return;
            if (MessageBox.Show($"Do you truly wish to clear {soundName}'s bound key ?",
                "Clear Keybind",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                sound.HotKey = new HotKey(Key.None, ModifierKeys.None);
            }
            AllowDrop = true;
        }

        [RelayCommand]
        private void AddSoundWindow()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new();
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() != MessageBoxResult.OK) { AllowDrop = true; return; }

            var sound = new Sound { Name = Path.GetFileNameWithoutExtension(openFileDialog.SafeFileName), Path = openFileDialog.FileName };
            Sounds.Add(sound);
        }
    }
}
