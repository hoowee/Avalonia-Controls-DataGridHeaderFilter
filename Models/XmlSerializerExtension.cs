using System;
using System.IO;
using System.Xml.Serialization;

namespace Avalonia.Controls.DataGridHeaderFilter.Models;

public class XmlSerializerExtension
{
    public static bool Serialize<T>(T obj, string fileName)
    {
        try
        {
            using var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            XmlSerializer xs = new XmlSerializer(typeof(T));
            xs.Serialize(fs, obj);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static T? Deserialize<T>(string fileName)
    {
        try
        {
            if (!File.Exists(fileName))
                return default;
            using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            XmlSerializer xs = new XmlSerializer(typeof(T));
            var obj = xs.Deserialize(fs);
            return obj is T t ? t : default;
        }
        catch (Exception)
        {
            return default;
        }
    }
}
