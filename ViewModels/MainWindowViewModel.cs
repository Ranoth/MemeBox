using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemeBox.Commands;
using MemeBox.Models;
using MemeBox.Services;
using MemeBox.Stores;
using MemeBox.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;

namespace MemeBox.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly NavigationStore navigationStore = new();
        private readonly SettingsStore settingsStore = new();
        private readonly PlayersStore playersStore;

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

            InitCommands();
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
                var key = new KeyBind(KeyInterop.KeyFromVirtualKey((int)e.KeyboardData.Key), Keyboard.Modifiers);

                try
                {
                    var crvm = navigationStore.CurrentViewModel;
                    if (crvm.GetType() == typeof(SoundBoardViewModel))
                    {
                        var sound = settingsStore.UserSounds.SingleOrDefault(x =>
                        {
                            if (x.KeyBind.Key == key.Key && x.KeyBind.Modifiers == key.Modifiers) return true;
                            else return false;
                        });

                        if (sound != null && !(crvm as SoundBoardViewModel).KeyBindChanging)
                        {
                            if ((crvm as SoundBoardViewModel).CanPlaySound(sound.Name)) (crvm as SoundBoardViewModel).PlaySound(sound.Name);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Cannot use duplicate keybinds");
                    settingsStore.UserSounds.LastOrDefault(x => x.KeyBind == key).KeyBind = new KeyBind(Key.None, ModifierKeys.None);
                }
            }
        }
    }
}
