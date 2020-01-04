using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Mag_ACClientLauncher.ServerManagement
{
    static class PublicServerManager
    {
        public static List<ServerItem> ServerList { get; private set; } = new List<ServerItem>();

        public static readonly string ServerListFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Mag-ACClientLauncher\\PublicServerList.xml";

        static PublicServerManager()
        {
            try
            {
                if (!File.Exists(ServerListFileName))
                    return;

                using (var reader = new StreamReader(ServerListFileName))
                    ServerList = Deserialize(reader);
            }
            catch
            {
                // ignored
            }
        }

        public static List<ServerItem> Deserialize(TextReader textReader)
        {
            var xs = new XmlSerializer(typeof(List<ServerItem>));

            return (List<ServerItem>)xs.Deserialize(textReader);
        }

        public static DateTime GetLastUpdated()
        {
            if (!File.Exists(ServerListFileName))
                return DateTime.MinValue;

            var fileInfo = new FileInfo(ServerListFileName);

            return fileInfo.LastWriteTime;
        }

        public static async Task<bool> UpdateFromPublicServerList(HttpClient httpClient, Uri uri)
        {
            try
            {
                string responseBody = await httpClient.GetStringAsync(uri);

                if (String.IsNullOrWhiteSpace(responseBody))
                    return false;

                using (var reader = new StringReader(responseBody))
                    ServerList = Deserialize(reader);

                var directoryName = Path.GetDirectoryName(ServerListFileName);

                if (!Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);

                File.WriteAllText(ServerListFileName, responseBody);

                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }
    }
}
