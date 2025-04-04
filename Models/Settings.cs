using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms.Design;
using System.Windows.Input;
using System.Xml.Serialization;
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
                // Apply logarithmic scaling
                AdjustedVolumeMain = (float)Math.Pow(volumeMain, 2);
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
                // Apply logarithmic scaling
                AdjustedVolumeAux = (float)Math.Pow(volumeAux, 2);
            }
        }

        // These properties store the adjusted (logarithmic) volume values
        [XmlIgnore]
        public float AdjustedVolumeMain { get; private set; } = 0.25f; // Default to 0.5^2
        [XmlIgnore]
        public float AdjustedVolumeAux { get; private set; } = 0.25f;  // Default to 0.5^2

        private HotKey pauseButtonHotKey = new(Key.None, ModifierKeys.None);
        public HotKey PauseButtonHotKey
        {
            get => pauseButtonHotKey;
            set
            {
                pauseButtonHotKey = value;
                OnPropertyChanged();
                PauseButtonHotKeyChanged?.Invoke();
            }
        }
        public event Action? PauseButtonHotKeyChanged;

        private HotKey resumeButtonHotKey = new HotKey(Key.None, ModifierKeys.None);
        public HotKey ResumeButtonHotKey
        {
            get => resumeButtonHotKey;
            set
            {
                resumeButtonHotKey = value;
                OnPropertyChanged();
                ResumeButtonHotKeyChanged?.Invoke();
            }
        }
        public event Action? ResumeButtonHotKeyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string SetButtonName(HotKey? hotKey, string buttonName)
        {
            if ((hotKey?.Key ?? Key.None) == Key.None) return $"{buttonName} Playback";
            else return $"{buttonName} Playback -> {hotKey}";
        }
    }
}