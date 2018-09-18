using System.Collections.Generic;

namespace Mag_ACClientLauncher.ServerManagement
{
    public class Account
    {
        public bool Launch { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public List<string> Characters { get; } = new List<string> { "" };

        public int SelectedCharacterIndex { get; set; }

        public override string ToString()
        {
            return UserName;
        }
    }
}
