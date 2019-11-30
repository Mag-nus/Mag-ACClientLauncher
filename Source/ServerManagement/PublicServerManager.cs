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
        public static readonly List<ServerItem> ServerList = new List<ServerItem>();

        public static readonly string ServerListFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Mag-ACClientLauncher\\PublicServerList.xml";

        static PublicServerManager()
        {
            try
            {
                if (!File.Exists(ServerListFileName))
                    return;

                var xs = new XmlSerializer(typeof(List<ServerItem>));

                using (var reader = new StreamReader(ServerListFileName))
                    ServerList = (List<ServerItem>)xs.Deserialize(reader);
            }
            catch
            {
                // ignored
            }
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

                if (!ValidateContents(responseBody))
                    return false;

                File.WriteAllText(ServerListFileName, responseBody);

                return true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        public static bool ValidateContents(string input)
        {
            var xs = new XmlSerializer(typeof(List<ServerItem>));

            var tempServerList = new List<ServerItem>();

            using (var reader = new StringReader(input))
                tempServerList = (List<ServerItem>)xs.Deserialize(reader);

            return tempServerList.Count > 0;
        }
    }
}
