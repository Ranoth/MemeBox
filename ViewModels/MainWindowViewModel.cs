using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemeBox.Commands;
using MemeBox.Models;
using MemeBox.Stores;
using MemeBox.Views;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using WPFUtilsBox.GlobalKeyboardHooker;
using WPFUtilsBox.HotKeyer;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxImage = System.Windows.Forms.MessageBoxIcon;
using MessageBoxButton = System.Windows.Forms.MessageBoxButtons;
using MessageBoxResult = System.Windows.Forms.DialogResult;
using WPFUtilsBox.EasyXml;
using System.Runtime.CompilerServices;

namespace MemeBox.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly NavigationStore navigationStore = new();
        private readonly SettingsStore settingsStore = new();
        private readonly PlayersStore playersStore;
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
            navigationStore.CurrentViewModelChanged += () => CurrentViewModel = navigationStore.CurrentViewModel;

            playersStore = new PlayersStore(settingsStore);

            navigationStore.CurrentViewModel = new SoundBoardViewModel(settingsStore, playersStore);

            SetStopButton();

            settingsStore.Settings.HotKeyChanged += () =>
            {
                SetStopButton();
            };

            InitCommands();
        }

        private void SetStopButton()
        {
            if (settingsStore.Settings.HotKey.Key == Key.None) StopButtonName = "Stop Playback";
            else StopButtonName = "Stop Playback -> " + settingsStore.Settings.HotKey.Key;

            try
            {
                XmlBroker.XmlDataWriter(settingsStore.Settings, settingsStore.SettingsFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.ToString());
            }


        }

        private void InitCommands()
        {
            ToSoundBoardCommand = new NavigateCommand(() => new SoundBoardViewModel(settingsStore, playersStore), navigationStore);
            ToUserControl1Command = new NavigateCommand(() => new UserControl1ViewModel(), navigationStore);
        }

        //private T ManageModels<T>() where T : ViewModelBase, new()
        //{
        //    var finder = navigationStore.ViewModelsSaved.Find(x => x.GetType() == typeof(T));
        //    if (finder != null) return (T)finder;
        //    else
        //    {
        //        var tempT = (T)Activator.CreateInstance(typeof(T), settingsStore, navigationStore, playersStore);
        //        navigationStore.ViewModelsSaved.Add(tempT);
        //        return tempT;
        //    }
        //}

        [RelayCommand(CanExecute = nameof(CanOpenSettingsWindow))]
        private void OpenSettingsWindow()
        {
            SettingsWindow = new SettingsWindow(settingsStore, playersStore, this);
            SettingsWindow.Show();
        }

        private bool CanOpenSettingsWindow() => SettingsWindow == null;

        [RelayCommand]
        private void StopPlayback()
        {
            playersStore.MainPlayer.Pause();
            playersStore.AuxPlayer.Pause();
        }

        //public void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    try
        //    {
        //        var crvm = navigationStore.CurrentViewModel;
        //        if (crvm.GetType() == typeof(SoundBoardViewModel))
        //        {
        //            var sound = settingsStore.UserSounds.SingleOrDefault(x => x.KeyBind == e.Key);

        //            if (sound != null)
        //            {
        //                if ((crvm as SoundBoardViewModel).CanPlaySound(sound.Name)) (crvm as SoundBoardViewModel).PlaySound(sound.Name);
        //            }
        //        }
        //    }
        //    catch (InvalidOperationException)
        //    {
        //        MessageBox.Show("Cannot use duplicate keybinds");
        //        settingsStore.UserSounds.LastOrDefault(x => x.KeyBind == e.Key).KeyBind = Key.None;
        //    }
        //}
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
            keyBindChanging = true;
            var keyBindDialog = new KeysBindsWindow(settingsStore, true);
            keyBindDialog.ShowDialog();
            keyBindChanging = false;
        }

        [RelayCommand]
        private void ClearKeyBind()
        {
            if (settingsStore.Settings.HotKey.Key == Key.None) return;

            if (MessageBox.Show($"Do you truly wish to clear the stop button's bound key ?",
                               "Clear Keybind",
                                              MessageBoxButton.YesNo,
                                                             MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                settingsStore.Settings.HotKey = new HotKey(Key.None, ModifierKeys.None);
            }
        }
    }
}
