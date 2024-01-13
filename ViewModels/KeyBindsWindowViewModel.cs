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
        private bool isStopButton;

        public HotKey? KeyToBind { get; set; } = new HotKey(Key.None, ModifierKeys.None);

        public KeyBindsWindowViewModel(SettingsStore settingsStore, KeysBindsWindow view, Sound soundToUpdate)
        {
            this.settingsStore = settingsStore;
            this.view = view;
            this.soundToUpdate = soundToUpdate;
        }
        public KeyBindsWindowViewModel(SettingsStore settingsStore, KeysBindsWindow view, bool isStopButton)
        {
            this.settingsStore = settingsStore;
            this.view = view;
            this.isStopButton = isStopButton;
        }
        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            var key = KeyBinder.GatherHotKey(e);
            if (key != null) KeyToBind = key;
            if (isStopButton) SetBindStopButton();
            else SetBind();
        }
        public void SetBind()
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
                if (settingsStore.Settings.HotKey.Key == KeyToBind.Key && settingsStore.Settings.HotKey.Modifiers == KeyToBind.Modifiers)
                {
                    MessageBox.Show($"This key has already been bound to the stop button, please choose another key");
                    return;
                }
                soundToUpdate.HotKey = KeyToBind;
                view.Close();
            }
            else if (MessageBox.Show($"This key has already been bound, clear old keybind from {sound.Name} to assign it to {soundToUpdate.Name} ?",
                     "Reassign Keybind",
                     MessageBoxButton.YesNo,
                     MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                sound.HotKey = new HotKey(Key.None, ModifierKeys.None);
                soundToUpdate.HotKey = KeyToBind;
                view.Close();
            }
        }

        public void SetBindStopButton()
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

            settingsStore.Settings.HotKey = KeyToBind;

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
            if (isStopButton == false && soundToUpdate.HotKey.Key != Key.None)
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
            else if (isStopButton == true && settingsStore.Settings.HotKey.Key != Key.None)
            {
                if (MessageBox.Show($"Do you truly wish to clear the stop button's bound key ?",
                                       "Clear Keybind", MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    settingsStore.Settings.HotKey = new HotKey(Key.None, ModifierKeys.None);
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
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            view.Close();
        }
    }
}
