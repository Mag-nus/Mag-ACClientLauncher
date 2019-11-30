using System;
using System.Collections.ObjectModel;

namespace Mag_ACClientLauncher.ServerManagement
{
    public class Server
    {
        public Guid Id;

        public string Name;

        // Backwards compatability
        public string ServerType;

        public EmuType EmuType;

        public string Address;
        public ushort Port;

        public bool ReadOnlyDat;

        public readonly ObservableCollection<Account> Accounts = new ObservableCollection<Account>();

        public override string ToString()
        {
            return $"{Name} - {EmuType}, {Address}:{Port}";
        }
    }
}
