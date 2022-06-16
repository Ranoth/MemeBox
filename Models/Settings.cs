using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemeBox.Models
{
    public class Settings
    {
        public string SetOut { get; set; } = "None";
        public string SetOutAux { get; set; } = "None";
        public float Volume { get; set; } = 0.5f;
    }
}
