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

        public string ACClientLocationOverride;
        public bool ReadOnlyDat;

        /// <summary>
        /// 0 = No Change
        /// 1 = Yes
        /// 2 = No
        /// </summary>
        public byte InjectDecalOverride;

        public readonly ObservableCollection<Account> Accounts = new ObservableCollection<Account>();

        public override string ToString()
        {
            return $"{Name} - {EmuType}, {Address}:{Port}";
        }
    }
}
