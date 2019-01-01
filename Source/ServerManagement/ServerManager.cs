using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Mag_ACClientLauncher.ServerManagement
{
    static class ServerManager
    {
        public static readonly List<Server> ServerList = new List<Server>();

        public static readonly string ServerListFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Mag-ACClientLauncher\\ServerList.xml";

        static ServerManager()
        {
            try
            {
                var xs = new XmlSerializer(typeof(List<Server>));

                using (var reader = new StreamReader(ServerListFileName))
                    ServerList = (List<Server>)xs.Deserialize(reader);
            }
            catch
            {
                // ignored
            }
        }

        public static void AddNewServer(Server server)
        {
            ServerList.Add(server);

            SaveServerListToDisk();
        }

        public static void DeleteServerById(Guid id)
        {
            var existing = ServerList.FirstOrDefault(s => s.Id == id);

            if (existing == null)
                return; 

            ServerList.Remove(existing);

            SaveServerListToDisk();
        }

        public static void SaveServerListToDisk()
        {
            var directoryName = Path.GetDirectoryName(ServerListFileName);

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            var xs = new XmlSerializer(typeof(List<Server>));

            using (var tw = new StreamWriter(ServerListFileName))
                xs.Serialize(tw, ServerList);
        }
    }
}
