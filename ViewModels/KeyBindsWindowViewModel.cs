using CommunityToolkit.Mvvm.Input;
using GlobalKeyboardHooker;
using MemeBox.Models;
using MemeBox.Stores;
using MemeBox.Views;
using System.Windows.Forms;
using System.Windows.Input;

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

        public void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var key = e.Key; var mod = Keyboard.Modifiers;
            if (key == Key.System)
                key = e.SystemKey;

            if (key == Key.LeftCtrl ||
                key == Key.LeftAlt ||
                key == Key.RightCtrl ||
                key == Key.RightAlt ||
                key == Key.LeftShift ||
                key == Key.RightShift ||
                key == Key.LWin ||
                key == Key.RWin ||
                key == Key.Clear ||
                key == Key.OemClear ||
                key == Key.Apps)
            {
                return;
            }

            KeyToBind = new HotKey(key, mod);
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
            else if (MessageBox.Show("This key has already been bound, clear old KeyBind to assign it to the new sound ?", "", MessageBoxButtons.YesNo) ==
                    DialogResult.Yes)
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
                if (MessageBox.Show("Do you truly wish to clear this sound's bound key ?", String.Empty, MessageBoxButtons.YesNo) == DialogResult.Yes)
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
