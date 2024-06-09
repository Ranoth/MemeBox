using MemeBox.Models;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Xml.Linq;
using WPFUtilsBox.EasyXml;
using WPFUtilsBox.HotKeyer;

namespace MemeBox.Stores
{
    public class SettingsStore
    {
        public Settings Settings = new();
        public string SettingsFilePath { get; set; } = "settings.xml";
        public string UserSoundsFilePath { get; set; } = "UserSounds.xml";
        public List<WaveOutCapabilities> AudioOutCapabilities { get; set; } = new();
        private bool allowDrop = false;
        public Action? SettingsWindowOpenChanged;
        public BindingList<Sound> UserSounds { get; set; } = new();

        public bool AllowDrop
        {
            get => allowDrop;
            set
            {
                allowDrop = value;
                SettingsWindowOpenChanged?.Invoke();
            }
        }   
        public SettingsStore()
        {
            ReadXmlSounds();
            ReadXmlSettings();
        }

        private void ReadXmlSounds()
        {

            if (!File.Exists(UserSoundsFilePath))
            {
                var xml = new XElement(nameof(UserSounds));
                new XDocument(xml).Save(UserSoundsFilePath);
            }
            else
            {
                var xDoc = XDocument.Load(UserSoundsFilePath);
                Sound _;

                var soundList = (from sounds in xDoc.Descendants("UserSounds").Elements("UserSound")
                                 select new Sound
                                 {
                                     Name = sounds?.Attribute(nameof(_.Name)).Value.ToString(),
                                     Path = sounds.Attribute(nameof(_.Path)).Value.ToString(),
                                     HotKey = new HotKey((Key)Enum.Parse(typeof(Key), sounds.Element(nameof(_.HotKey)).Attribute(nameof(_.HotKey.Key)).Value.ToString()),
                                                         (ModifierKeys)Enum.Parse(typeof(ModifierKeys), sounds.Element(nameof(_.HotKey)).Attribute(nameof(_.HotKey.Modifiers)).Value.ToString()))
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
                var waveOut = AudioOutCapabilities.FirstOrDefault(x => device.FriendlyName.Contains(x.ProductName));

                Settings = new Settings { SetOut = waveOut.ProductName };
                XmlBroker.XmlDataWriter(Settings, SettingsFilePath);
            }
            else
            {
                Settings = XmlBroker.XmlDataReader<Settings>(SettingsFilePath);
            }
        }

        public void InvokeSelectedPlayerChanged()
        {
            SelectedPlayerChanged?.Invoke();
        }
    }
}
