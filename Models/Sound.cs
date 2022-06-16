using CommunityToolkit.Mvvm.ComponentModel;
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
        private KeyBind keyBind = new KeyBind(Key.None, ModifierKeys.None);
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
        public KeyBind KeyBind
        {
            get => keyBind;
            set
            {
                SetProperty(ref keyBind, value);
                SetNameBind();
            }
        }

        private void SetNameBind()
        {
            string bind = KeyBind.ToString();

            if (bind == "None") bind = String.Empty;
            else if (bind != String.Empty) bind = bind.Insert(0, " -> ");

            NameBind = $"{Name} {bind}";
        }
    }
}
