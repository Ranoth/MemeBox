using CommunityToolkit.Mvvm.ComponentModel;
using MemeBox.Stores;
using NAudio.Wave;
using System.Windows.Input;
using WPFUtilsBox.HotKeyer;

namespace MemeBox.Models
{
    public partial class Sound : ObservableObject
    {
        private string? name;
        private HotKey hotKey = new HotKey(Key.None, ModifierKeys.None);
        [ObservableProperty]
        private string? nameBind;
        private int? progress;
        public string? Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
                SetNameBind();
            }
        }

        public string? Path { get; set; }
        public HotKey HotKey
        {
            get => hotKey;
            set
            {
                SetProperty(ref hotKey, value);
                SetNameBind();
            }
        }
        public int Progress
        {
            get => progress ?? 0;
            set
            {
                SetProperty(ref progress, value);
            }
        }

        //public void Progress = (SettingsStore settingsStore, int progress)
        //{
        //    settingsStore.UserSounds.RaiseListChangedEvents = false;
        //    Progress = progress;
        //    settingsStore.UserSounds.RaiseListChangedEvents = true;
        //}

        private void SetNameBind()
        {
            string bind = HotKey.ToString();

            if (bind == "None") bind = String.Empty;
            else if (bind != String.Empty) bind = bind.Insert(0, " -> ");

            NameBind = $"{Name} {bind}";
        }
    }
}
