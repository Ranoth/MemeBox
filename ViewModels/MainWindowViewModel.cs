﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemeBox.Commands;
using MemeBox.Stores;
using MemeBox.Views;
using System.Windows.Input;
using WPFUtilsBox.GlobalKeyboardHooker;
using WPFUtilsBox.HotKeyer;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxImage = System.Windows.Forms.MessageBoxIcon;
using MessageBoxButton = System.Windows.Forms.MessageBoxButtons;
using MessageBoxResult = System.Windows.Forms.DialogResult;
using WPFUtilsBox.EasyXml;
using AutoUpdaterDotNET;
using System.Collections.Specialized;
using MemeBox.Models;
using System.ComponentModel;
using NAudio.Utils;

namespace MemeBox.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly NavigationStore navigationStore = new();
        private readonly SettingsStore settingsStore = new();
        private PlayersStore playersStore;
        [ObservableProperty]
        private string stopButtonName;

        private bool keyBindChanging = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenSettingsWindowCommand))]
        private SettingsWindow settingsWindow;
        [ObservableProperty]
        private ViewModelBase currentViewModel;

        public NavigateCommand? ToSoundBoardCommand { get; private set; }
        public NavigateCommand? ToUserControl1Command { get; private set; }

        public MainWindowViewModel()
        {
            AutoUpdater.Start("https://getmemebox.ranoth.com/app.xml");

            navigationStore.CurrentViewModelChanged += () => CurrentViewModel = navigationStore.CurrentViewModel;

            playersStore = new PlayersStore(settingsStore);

            navigationStore.CurrentViewModel = new SoundBoardViewModel(settingsStore, playersStore);

            SetStopButton();

            settingsStore.Settings.HotKeyChanged += () => SetStopButton();
            settingsStore.UserSounds.ListChanged += (s, e) =>
            {
                UnbindAllButtonsCommand.NotifyCanExecuteChanged();
                RemoveAllSoundsCommand.NotifyCanExecuteChanged();
            };
            settingsStore.Settings.HotKeyChanged += () => UnbindAllButtonsCommand.NotifyCanExecuteChanged();

            InitCommands();
        }
        private void SetStopButton()
        {
            if ((settingsStore.Settings.HotKey?.Key ?? Key.None) == Key.None) StopButtonName = "Stop Playback";
            else StopButtonName = "Stop Playback -> " + settingsStore.Settings.HotKey;
        }

        private void InitCommands()
        {
            ToSoundBoardCommand = new NavigateCommand(() => new SoundBoardViewModel(settingsStore, playersStore), navigationStore);
            ToUserControl1Command = new NavigateCommand(() => new UserControl1ViewModel(), navigationStore);
        }

        [RelayCommand(CanExecute = nameof(CanOpenSettingsWindow))]
        private void OpenSettingsWindow()
        {
            settingsStore.AllowDrop = false;
            SettingsWindow = new SettingsWindow(settingsStore, playersStore, this);
            SettingsWindow.ShowDialog();
            settingsStore.AllowDrop = true;
        }

        private bool CanOpenSettingsWindow() => SettingsWindow == null;

        [RelayCommand]
        private void StopPlayback()
        {
            playersStore.MainPlayer.Pause();
            playersStore.AuxPlayer.Pause();
            var sound = settingsStore.UserSounds.FirstOrDefault(x => x.Progress != 0);
            if (sound != null) sound.SetProgress(settingsStore, 0);
        }
        public void OnKeyDownGlobal(object sender, GlobalKeyboardHookEventArgs e)
        {
            if (!e.KeyboardState.HasFlag(GlobalKeyboardHook.KeyboardState.KeyUp))
            {
                var key = new HotKey(KeyInterop.KeyFromVirtualKey((int)e.KeyboardData.Key), Keyboard.Modifiers);

                try
                {
                    var crvm = navigationStore.CurrentViewModel;
                    if (crvm.GetType() == typeof(SoundBoardViewModel))
                    {
                        var sound = settingsStore.UserSounds.SingleOrDefault(x =>
                        {
                            if (x.HotKey.Key == key.Key && x.HotKey.Modifiers == key.Modifiers) return true;
                            else return false;
                        });

                        if (sound != null && !(crvm as SoundBoardViewModel).KeyBindChanging)
                        {
                            if ((crvm as SoundBoardViewModel).CanPlaySound(sound.Name)) (crvm as SoundBoardViewModel).PlaySound(sound.Name);
                        }
                        else if (sound == null && key.Key == settingsStore.Settings.HotKey.Key
                            && key.Modifiers == settingsStore.Settings.HotKey.Modifiers
                            && !(crvm as SoundBoardViewModel).KeyBindChanging)
                        {
                            StopPlayback();
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Cannot use duplicate keybinds");
                    settingsStore.UserSounds.LastOrDefault(x => x.HotKey == key).HotKey = new HotKey(Key.None, ModifierKeys.None);
                }
            }
        }

        [RelayCommand]
        private void SetKeyBind()
        {
            settingsStore.AllowDrop = false;
            keyBindChanging = true;
            var keyBindDialog = new KeysBindsWindow(settingsStore, true);
            keyBindDialog.ShowDialog();
            keyBindChanging = false;
            settingsStore.AllowDrop = true;
        }

        [RelayCommand]
        private void ClearKeyBind()
        {
            settingsStore.AllowDrop = false;
            if (settingsStore.Settings.HotKey.Key == Key.None) { settingsStore.AllowDrop = true; return; };

            if (MessageBox.Show($"Do you truly wish to clear the stop button's bound key ?",
                                "Clear Keybind",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                settingsStore.Settings.HotKey = new HotKey(Key.None, ModifierKeys.None);
            }
            settingsStore.AllowDrop = true;
        }

        [RelayCommand(CanExecute = nameof(CanExecuteUnbindAllButtons))]
        private void UnbindAllButtons()
        {
            settingsStore.AllowDrop = false;
            if (MessageBox.Show($"Do you truly wish to unbind all buttons ?",
                                "Unbind All Buttons",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                foreach (var sound in settingsStore.UserSounds) sound.HotKey = new HotKey(Key.None, ModifierKeys.None);
                settingsStore.Settings.HotKey = new HotKey(Key.None, ModifierKeys.None);
                try
                {
                    XmlBroker.XmlDataWriter(settingsStore.Settings, settingsStore.SettingsFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error : " + ex.ToString());
                }
                UnbindAllButtonsCommand.NotifyCanExecuteChanged();
                settingsStore.AllowDrop = true;
            }
        }
        private bool CanExecuteUnbindAllButtons()
        {
            if (settingsStore.Settings.HotKey.Key == Key.None && settingsStore.UserSounds.All(x => x.HotKey.Key == Key.None)) return false;
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanExecuteRemoveAllSounds))]
        private void RemoveAllSounds()
        {
            settingsStore.AllowDrop = false;
            if (MessageBox.Show($"Do you truly wish to remove all sounds ?",
                                               "Remove All Sounds",
                                               MessageBoxButton.YesNo,
                                               MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                playersStore.MainPlayer.Pause();
                playersStore.AuxPlayer.Pause();
                settingsStore.UserSounds.Clear();
                RemoveAllSoundsCommand.NotifyCanExecuteChanged();
            }
            settingsStore.AllowDrop = true;
        }

        private bool CanExecuteRemoveAllSounds()
        {
            if (settingsStore.UserSounds.Count == 0) return false;
            return true;
        }
    }
}
