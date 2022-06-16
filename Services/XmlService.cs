using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MemeBox.Services
{
    public static class XmlService
    {
        public static void XmlDataWriter(object obj, string filename)
        {
            var sr = new XmlSerializer(obj.GetType());
            var writer = new StreamWriter(filename);
            sr.Serialize(writer, obj);
            writer.Close();
        }

        public static T XmlDataReader<T>(string filename) where T : class, new()
        {
            var data = new T();
            if (File.Exists(filename))
            {
                var xs = new XmlSerializer(typeof(T));
                var reader = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                data = (T)xs.Deserialize(reader);
                reader.Close();
            }
            return data;
        }
    }
}
