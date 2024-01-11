using System.Windows.Input;
using WPFUtilsBox.HotKeyer;

namespace MemeBox.Models
{
    public class Settings
    {
        public string SetOut { get; set; } = "None";
        public string SetOutAux { get; set; } = "None";
        public float Volume { get; set; } = 0.5f;
        private HotKey hotKey = new(Key.None, ModifierKeys.None);
        public HotKey HotKey
        {
            get => hotKey;
            set
            {
                hotKey = value;
                HotKeyChanged?.Invoke();
            }
        }
        public event Action? HotKeyChanged;
    }
}
