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

                // Backwards Compatability
                foreach (var server in ServerList)
                {
                    if (!String.IsNullOrWhiteSpace(server.ServerType))
                    {
                        if (server.ServerType == "ACE")
                            server.EmuType = EmuType.ACE;

                        server.ServerType = null;
                    }
                }
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

        public static Server FindByGuid(Guid id)
        {
            foreach (var server in ServerList)
            {
                if (server.Id == id)
                    return server;
            }

            return null;
        }

        public static bool TryImport(ServerItem serverItem)
        {
            if (FindByGuid(serverItem.id) != null)
                return false;

            var server = new Server
            {
                Id = serverItem.id,
                Name = serverItem.name,
                Address = serverItem.server_host,
                Port = serverItem.server_port,
            };

            if (serverItem.emu == "ACE")
                server.EmuType = EmuType.ACE;
            else if (serverItem.emu == "GDL")
                server.EmuType = EmuType.GDL;

            ServerList.Add(server);

            return true;
        }

        public static bool TryUpdate(ServerItem serverItem)
        {
            var server = FindByGuid(serverItem.id);

            if (server == null)
                return false;

            server.Id = serverItem.id;
            server.Name = serverItem.name;

            if (serverItem.emu == "ACE")
                server.EmuType = EmuType.ACE;
            else if (serverItem.type == "GDL")
                server.EmuType = EmuType.GDL;

            server.Address = serverItem.server_host;
            server.Port = serverItem.server_port;

            return true;
        }
    }
}
