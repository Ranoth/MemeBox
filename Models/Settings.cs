using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WPFUtilsBox.HotKeyer;

namespace MemeBox.Models
{
    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string setOut = "None";
        public string SetOut
        {
            get => setOut;
            set
            {
                setOut = value;
                OnPropertyChanged();
            }
        }

        private string setOutAux = "None";
        public string SetOutAux
        {
            get => setOutAux;
            set
            {
                setOutAux = value;
                OnPropertyChanged();
            }
        }

        private float volumeMain = 0.5f;
        public float VolumeMain
        {
            get => volumeMain;
            set
            {
                volumeMain = value;
                OnPropertyChanged();
            }
        }

        private float volumeAux = 0.5f;
        public float VolumeAux
        {
            get => volumeAux;
            set
            {
                volumeAux = value;
                OnPropertyChanged();
            }
        }

        private HotKey hotKey = new(Key.None, ModifierKeys.None);
        public HotKey HotKey
        {
            get => hotKey;
            set
            {
                hotKey = value;
                // OnPropertyChanged();
                HotKeyChanged?.Invoke();
            }
        }

        public event Action? HotKeyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}