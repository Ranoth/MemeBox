using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using WPFUtilsBox.GlobalKeyboardHooker;
using WPFUtilsBox.HotKeyer;

namespace MemeBox.Models
{
    public partial class Sound : ObservableObject
    {
        private string? name;
        private HotKey hotKey = new HotKey(Key.None, ModifierKeys.None);
        [ObservableProperty]
        private string? nameBind;
        [ObservableProperty]
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

        private void SetNameBind()
        {
            string bind = HotKey.ToString();

            if (bind == "None") bind = String.Empty;
            else if (bind != String.Empty) bind = bind.Insert(0, " -> ");

            NameBind = $"{Name} {bind}";
        }
    }
}
