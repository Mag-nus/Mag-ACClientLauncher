using System;
using System.Collections.Generic;
using System.Linq;

namespace Mag_ACClientLauncher.ServerManagement
{
    static class ServerManager
    {
        public static readonly List<Server> ServerList = new List<Server>();

        static ServerManager()
        {
            try
            {
                /*
                string folder = GetServerDataFolder();
                var persister = new GameManagement.ServerPersister(folder);
                var publishedGDLServers = persister.GetPublishedGDLServerList();
                var publishedAceServers = persister.GetPublishedACEServerList();
                var userServers = persister.ReadUserServers();

                var servers = new List<GameManagement.ServerPersister.ServerData>();
                servers.AddRange(publishedGDLServers);
                servers.AddRange(publishedAceServers);
                servers.AddRange(userServers);

                var distinctServers = servers.Distinct().ToList();

                foreach (var sdata in distinctServers)
                    AddOrUpdateServer(sdata);
                */
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

        private static void SaveServerListToDisk()
        {
            /*var userServers = ServerList.Where(s => s.ServerSource != ServerModel.ServerSourceEnum.Published);

            var persister = new GameManagement.ServerPersister(GetServerDataFolder());
            persister.WriteServerListToFile(userServers);*/
        }
    }
}
