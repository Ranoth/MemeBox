using CommunityToolkit.Mvvm.ComponentModel;
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
using AutoUpdaterDotNET;
using CommunityToolkit.Mvvm.Messaging;
using MemeBox.Messages;
using NAudio.Wave;

namespace MemeBox.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly NavigationStore navigationStore = new();
        private readonly SettingsStore settingsStore = new();
        private PlayersStore playersStore;
        [ObservableProperty]
        private string pauseButtonName;
        [ObservableProperty]
        private string resumeButtonName;

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

            SetPauseButton();
            SetResumeButton();

            settingsStore.Settings.PauseButtonHotKeyChanged += () =>
            {
                SetPauseButton();
                UnbindAllButtonsCommand.NotifyCanExecuteChanged();
            };
            settingsStore.Settings.ResumeButtonHotKeyChanged += () =>
            {
                SetResumeButton();
                UnbindAllButtonsCommand.NotifyCanExecuteChanged();
            };

            settingsStore.UserSounds.ListChanged += (s, e) =>
            {
                UnbindAllButtonsCommand.NotifyCanExecuteChanged();
                RemoveAllSoundsCommand.NotifyCanExecuteChanged();
            };

            WeakReferenceMessenger.Default.Register<PlaybackStateChangedMessage>(this, (r, m) =>
            {
                ResumeSoundCommand.NotifyCanExecuteChanged();
                PausePlaybackCommand.NotifyCanExecuteChanged();
            });

            InitCommands();
        }

        private void SetResumeButton()
        {
            ResumeButtonName = settingsStore.Settings.SetButtonName(settingsStore.Settings.ResumeButtonHotKey, "Resume");
        }

        private void SetPauseButton()
        {
            PauseButtonName = settingsStore.Settings.SetButtonName(settingsStore.Settings.PauseButtonHotKey, "Pause");
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

        [RelayCommand(CanExecute = nameof(CanExecutePausePlayback))]
        private void PausePlayback()
        {
            playersStore.PausePlayers();
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
                        else if (sound == null && key.Key == settingsStore.Settings.PauseButtonHotKey.Key
                            && key.Modifiers == settingsStore.Settings.PauseButtonHotKey.Modifiers
                            && !(crvm as SoundBoardViewModel).KeyBindChanging)
                        {
                            PausePlayback();
                            ResumeSoundCommand.NotifyCanExecuteChanged();
                        }
                        else if (sound == null && key.Key == settingsStore.Settings.ResumeButtonHotKey.Key
                            && key.Modifiers == settingsStore.Settings.ResumeButtonHotKey.Modifiers
                            && !(crvm as SoundBoardViewModel).KeyBindChanging)
                        {
                            ResumeSound();
                            PausePlaybackCommand.NotifyCanExecuteChanged();
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
        private void SetKeyBind(string buttonName)
        {
            settingsStore.AllowDrop = false;
            keyBindChanging = true;
            var keyBindDialog = new KeysBindsWindow(settingsStore, buttonName);
            keyBindDialog.ShowDialog();
            keyBindChanging = false;
            settingsStore.AllowDrop = true;
        }

        [RelayCommand]
        private void ClearKeyBind(string buttonName)
        {
            settingsStore.AllowDrop = false;

            if (buttonName.StartsWith("Pause"))
            {
                if (settingsStore.Settings.PauseButtonHotKey.Key == Key.None) { settingsStore.AllowDrop = true; return; };

                if (MessageBox.Show($"Do you truly wish to clear the pause button's bound key ?",
                                    "Clear Keybind",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    settingsStore.Settings.PauseButtonHotKey = new HotKey(Key.None, ModifierKeys.None);
                }
            }
            else if (buttonName.StartsWith("Resume"))
            {
                if (settingsStore.Settings.ResumeButtonHotKey.Key == Key.None) { settingsStore.AllowDrop = true; return; };

                if (MessageBox.Show($"Do you truly wish to clear the resume button's bound key ?",
                                                       "Clear Keybind",
                                                       MessageBoxButton.YesNo,
                                                       MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    settingsStore.Settings.ResumeButtonHotKey = new HotKey(Key.None, ModifierKeys.None);
                }
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
                settingsStore.Settings.PauseButtonHotKey = new HotKey(Key.None, ModifierKeys.None);
                settingsStore.Settings.ResumeButtonHotKey = new HotKey(Key.None, ModifierKeys.None);

                UnbindAllButtonsCommand.NotifyCanExecuteChanged();
                settingsStore.AllowDrop = true;
            }
        }
        private bool CanExecuteUnbindAllButtons()
        {
            if ((settingsStore.Settings.PauseButtonHotKey.Key == Key.None || settingsStore.Settings.ResumeButtonHotKey.Key == Key.None)
                && settingsStore.UserSounds.All(x => x.HotKey.Key == Key.None)) return false;
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
                playersStore.StopPlayers();
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

        [RelayCommand(CanExecute = nameof(CanExecuteResumePlayback))]
        private void ResumeSound()
        {
            playersStore.ResumePlayers();
        }

        private bool CanExecuteResumePlayback()
        {
            if (playersStore.MainPlayer.PlaybackState == PlaybackState.Paused) return true;
            else if (playersStore.MainPlayer.PlaybackState == PlaybackState.Stopped) return false;
            return false;
        }

        private bool CanExecutePausePlayback()
        {
            if (playersStore.MainPlayer.PlaybackState == PlaybackState.Playing) return true;
            else if (playersStore.MainPlayer.PlaybackState == PlaybackState.Stopped) return false;
            return false;
        }
    }
}
