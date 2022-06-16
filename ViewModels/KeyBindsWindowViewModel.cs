using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemeBox.Models;
using MemeBox.Stores;
using MemeBox.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace MemeBox.ViewModels
{
    public partial class KeyBindsWindowViewModel : ViewModelBase
    {
        private SettingsStore settingsStore;
        private Sound soundToUpdate;
        private KeysBindsWindow view;

        public KeyBind KeyToBind { get; set; }
        public string NameSoundToUpdate
        {
            get => soundToUpdate.Name;
        }
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

            KeyToBind = new KeyBind(key, mod);
            SetBind();
        }
        public void SetBind()
        {
            var sound = settingsStore.UserSounds.SingleOrDefault(x =>
            {
                if (x.KeyBind.Key == KeyToBind.Key && x.KeyBind.Modifiers == KeyToBind.Modifiers) return true;
                else return false;
            });
            if (sound == null)
            {
                soundToUpdate.KeyBind = KeyToBind;
                view.Close();
            }
            else if (System.Windows.Forms.MessageBox.Show("This key has already been bound, clear old KeyBind to assign it to the new sound ?", "", MessageBoxButtons.YesNo) ==
                    DialogResult.Yes)
            {

                sound.KeyBind = new KeyBind(Key.None, ModifierKeys.None);
                soundToUpdate.KeyBind = KeyToBind;
                view.Close();
            }
        }

        [RelayCommand]
        private void ClearBind()
        {
            if (soundToUpdate.KeyBind.Key != Key.None)
            {
                if (System.Windows.Forms.MessageBox.Show("Do you truly wish to clear this sound's bound key ?", String.Empty, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    soundToUpdate.KeyBind = new KeyBind(Key.None, ModifierKeys.None);
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
