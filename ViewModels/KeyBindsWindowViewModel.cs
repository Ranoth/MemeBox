using CommunityToolkit.Mvvm.Input;
using MemeBox.Models;
using MemeBox.Stores;
using MemeBox.Views;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;
using MessageBoxImage = System.Windows.Forms.MessageBoxIcon;
using MessageBoxButton = System.Windows.Forms.MessageBoxButtons;
using MessageBoxResult = System.Windows.Forms.DialogResult;
using WPFUtilsBox.GlobalKeyboardHooker;
using WPFUtilsBox.HotKeyer;

namespace MemeBox.ViewModels
{
    public partial class KeyBindsWindowViewModel : ViewModelBase
    {
        private SettingsStore settingsStore;
        private Sound soundToUpdate;
        private KeysBindsWindow view;

        public HotKey? KeyToBind { get; set; }
        public KeyBindsWindowViewModel(SettingsStore settingsStore, Sound soundToUpdate, KeysBindsWindow view)
        {
            this.settingsStore = settingsStore;
            this.soundToUpdate = soundToUpdate;
            this.view = view;
        }

        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            var key = KeyBinder.GatherHotKey(e);
            if (key != null) KeyToBind = key;
            SetBind();
        }
        public void SetBind()
        {
            var sound = settingsStore.UserSounds.SingleOrDefault(x =>
            {
                if (x.HotKey.Key == KeyToBind.Key && x.HotKey.Modifiers == KeyToBind.Modifiers) return true;
                else return false;
            });
            if (sound == null)
            {
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

        [RelayCommand]
        private void ClearBind()
        {
            if (soundToUpdate.HotKey.Key != Key.None)
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
        }

        [RelayCommand]
        private void Cancel()
        {
            view.Close();
        }
    }
}
