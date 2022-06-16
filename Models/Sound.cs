using CommunityToolkit.Mvvm.ComponentModel;
using GlobalKeyboardHooker;
using MemeBox.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace MemeBox.Models
{
    public partial class Sound : ObservableObject
    {
        private string name;
        private HotKey hotKey = new HotKey(Key.None, ModifierKeys.None);
        [ObservableProperty]
        private string nameBind;

        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
                SetNameBind();
            }
        }

        public string Path { get; set; }
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
