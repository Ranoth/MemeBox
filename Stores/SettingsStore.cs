using EasyXml;
using GlobalKeyboardHooker;
using MemeBox.Models;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;

namespace MemeBox.Stores
{
    public class SettingsStore
    {
        private Settings settings = new();
        public string SettingsFilePath { get; set; } = "settings.xml";
        public string UserSoundsFilePath { get; set; } = "UserSounds.xml";
        public List<WaveOutCapabilities> AudioOutCapabilities { get; set; } = new();

        public Settings Settings
        {
            get => settings;
            set
            {
                settings = value;
                SettingsChanged?.Invoke();
            }
        }

        public BindingList<Sound> UserSounds { get; set; } = new();

        public event Action? SettingsChanged;
        public SettingsStore()
        {
            ReadXmlSounds();
            ReadXmlSettings();
        }

        private void ReadXmlSounds()
        {

            if (!File.Exists(UserSoundsFilePath))
            {
                var xml = new XElement("UserSounds");
                new XDocument(xml).Save(UserSoundsFilePath);
            }
            else
            {
                var xDoc = XDocument.Load(UserSoundsFilePath);
                Sound dummySound = new();

                var soundList = (from sounds in xDoc.Descendants("UserSounds").Elements("UserSound")
                                 select new Sound
                                 {
                                     Name = sounds.Attribute(nameof(dummySound.Name)).Value.ToString(),
                                     Path = sounds.Attribute(nameof(dummySound.Path)).Value.ToString(),
                                     HotKey = new HotKey((Key)Enum.Parse(typeof(Key), sounds.Element("KeyBind").Attribute("Key").Value.ToString()),
                                                           ((ModifierKeys)Enum.Parse(typeof(ModifierKeys), sounds.Element("KeyBind").Attribute("Modifiers").Value.ToString())))
                                 }).ToList();

                UserSounds = new BindingList<Sound>(soundList);
            }
        }

        private void ReadXmlSettings()
        {
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                AudioOutCapabilities.Add(WaveOut.GetCapabilities(i));
            }

            if (!File.Exists(SettingsFilePath))
            {
                var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                var waveOut = AudioOutCapabilities.Find(x => device.FriendlyName.Contains(x.ProductName));

                Settings = new Settings { SetOut = waveOut.ProductName };
                XmlBroker.XmlDataWriter(Settings, SettingsFilePath);
            }
            else
            {
                Settings = XmlBroker.XmlDataReader<Settings>(SettingsFilePath);
            }
        }
    }
}
