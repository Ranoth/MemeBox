using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MemeBox.Models
{
    public class KeyBind
    {
        public Key Key { get; set; }
        public ModifierKeys Modifiers { get; set; }

        public KeyBind(Key key, ModifierKeys modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public override string ToString()
        {
            string str = String.Empty;

            if (Modifiers.HasFlag(ModifierKeys.Control)) str += "Ctrl + ";
            if (Modifiers.HasFlag(ModifierKeys.Shift)) str += "Shift + ";
            if (Modifiers.HasFlag(ModifierKeys.Alt)) str += "Alt + ";
            if (Modifiers.HasFlag(ModifierKeys.Windows)) str += "Win + ";

            str += Key;

            return str;
        }
    }
}
