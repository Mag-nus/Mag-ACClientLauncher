using System;

namespace Mag_ACClientLauncher.ServerManagement
{
    public class Server
    {
        public Guid Id;

        public string Name;

        public ServerType ServerType;

        public string Address;
        public ushort Port;

        public bool ReadOnlyDat;

        public override string ToString()
        {
            return $"{Name} - {ServerType} {Address}:{Port}";
        }
    }
}
