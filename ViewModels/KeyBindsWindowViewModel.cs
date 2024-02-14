using CommunityToolkit.Mvvm.Input;
using MemeBox.Models;
using MemeBox.Stores;
using MemeBox.Views;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxImage = System.Windows.Forms.MessageBoxIcon;
using MessageBoxButton = System.Windows.Forms.MessageBoxButtons;
using MessageBoxResult = System.Windows.Forms.DialogResult;
using WPFUtilsBox.HotKeyer;
using WPFUtilsBox.EasyXml;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MemeBox.ViewModels
{
    public partial class KeyBindsWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private SettingsStore settingsStore;
        private Sound soundToUpdate;
        private KeysBindsWindow view;
        private string buttonName;

        public HotKey? KeyToBind { get; set; } = new HotKey(Key.None, ModifierKeys.None);

        public KeyBindsWindowViewModel(SettingsStore settingsStore, KeysBindsWindow view, Sound soundToUpdate)
        {
            this.settingsStore = settingsStore;
            this.view = view;
            this.soundToUpdate = soundToUpdate;
        }
        public KeyBindsWindowViewModel(SettingsStore settingsStore, KeysBindsWindow view, string buttonName)
        {
            this.settingsStore = settingsStore;
            this.view = view;
            this.buttonName = buttonName;
        }

        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            var key = KeyBinder.GatherHotKey(e);
            if (key != null) KeyToBind = key;
            if (buttonName != null) SetBindPlaybackButton(buttonName);
            else SetBindSound();
        }
        public void SetBindSound()
        {
            if (KeyToBind.Key == Key.None)
            {
                MessageBox.Show("Please choose a key along with the modifier");
                return;
            }

            var sound = settingsStore.UserSounds.SingleOrDefault(x =>
            {
                if (x.HotKey.Key == KeyToBind.Key && x.HotKey.Modifiers == KeyToBind.Modifiers) return true;
                else return false;
            });

            if (sound == null)
            {
                if (settingsStore.Settings.PauseButtonHotKey.Key == KeyToBind.Key && settingsStore.Settings.PauseButtonHotKey.Modifiers == KeyToBind.Modifiers)
                {
                    MessageBox.Show($"This key has already been bound to the pause button, please choose another key");
                    return;
                }
                else if ((settingsStore.Settings.ResumeButtonHotKey.Key == KeyToBind.Key && settingsStore.Settings.ResumeButtonHotKey.Modifiers == KeyToBind.Modifiers))
                {
                    MessageBox.Show($"This key has already been bound to the resume button, please choose another key");
                    return;
                }
                soundToUpdate.HotKey = KeyToBind;
                view.Close();
            }
            else if (sound.Name != soundToUpdate.Name && MessageBox.Show($"This key has already been bound, clear old keybind from {sound.Name} to assign it to {soundToUpdate.Name} ?",
                     "Reassign Keybind",
                     MessageBoxButton.YesNo,
                     MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                sound.HotKey = new HotKey(Key.None, ModifierKeys.None);
                soundToUpdate.HotKey = KeyToBind;
                view.Close();
            }
            else if (sound.Name == soundToUpdate.Name)
            {
                soundToUpdate.HotKey = KeyToBind;
                view.Close();
            }
        }

        public void SetBindPlaybackButton(string buttonName)
        {
            if (KeyToBind.Key == Key.None)
            {
                MessageBox.Show("Please choose a key along with the modifier");
                return;
            }

            var sound = settingsStore.UserSounds.SingleOrDefault(x =>
            {
                if (x.HotKey.Key == KeyToBind.Key && x.HotKey.Modifiers == KeyToBind.Modifiers) return true;
                else return false;
            });

            if (sound != null)
            {
                MessageBox.Show($"This key has already been bound to {sound.Name}, please choose another key");
                return;
            }

            if (buttonName?.StartsWith("Pause") ?? false)
            {
                if ((settingsStore.Settings.ResumeButtonHotKey.Key == KeyToBind.Key && settingsStore.Settings.ResumeButtonHotKey.Modifiers == KeyToBind.Modifiers))
                {
                    MessageBox.Show($"This key has already been bound to the resume button, please choose another key");
                    return;
                }
                settingsStore.Settings.PauseButtonHotKey = KeyToBind;
            }
            else if (buttonName?.StartsWith("Resume") ?? false)
            {
                if (settingsStore.Settings.PauseButtonHotKey.Key == KeyToBind.Key && settingsStore.Settings.PauseButtonHotKey.Modifiers == KeyToBind.Modifiers)
                {
                    MessageBox.Show($"This key has already been bound to the pause button, please choose another key");
                    return;
                }
                settingsStore.Settings.ResumeButtonHotKey = KeyToBind;
            }

            try
            {
                XmlBroker.XmlDataWriter(settingsStore.Settings, settingsStore.SettingsFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.ToString());
            }
            view.Close();
        }

        [RelayCommand]
        private void ClearBind()
        {
            if (buttonName == string.Empty && soundToUpdate.HotKey.Key != Key.None)
            {
                if (MessageBox.Show($"Do you truly wish to clear {soundToUpdate.Name}'s bound key ?",
                    "Clear Keybind",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    soundToUpdate.HotKey = new HotKey(Key.None, ModifierKeys.None);
                    view.Close();
                }
            }
            else if (buttonName != string.Empty && settingsStore.Settings.PauseButtonHotKey.Key != Key.None)
            {
                if (MessageBox.Show($"Do you truly wish to clear the stop button's bound key ?",
                                       "Clear Keybind", MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    settingsStore.Settings.PauseButtonHotKey = new HotKey(Key.None, ModifierKeys.None);
                    view.Close();
                }
            }
            else if (buttonName != string.Empty && settingsStore.Settings.ResumeButtonHotKey.Key != Key.None)
            {
                if (MessageBox.Show($"Do you truly wish to clear the resume button's bound key ?",
                                                          "Clear Keybind", MessageBoxButton.YesNo,
                                                                                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    settingsStore.Settings.ResumeButtonHotKey = new HotKey(Key.None, ModifierKeys.None);
                    view.Close();
                }
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            view.Close();
        }
    }
}
