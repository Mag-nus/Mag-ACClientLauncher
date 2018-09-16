using System;
using System.Linq;
using System.Windows;

namespace Mag_ACClientLauncher.ServerManagement
{
    /// <summary>
    /// Interaction logic for AddServer.xaml
    /// </summary>
    public partial class AddServer
    {
        public readonly Server Server;

        public AddServer()
        {
            InitializeComponent();

            Server = new Server();
            Server.Id = Guid.NewGuid();
        }

        public AddServer(Server server)
        {
            InitializeComponent();

            Server = server;

            Title = "Edit A Server";
            btnAdd.Content = "Save";

            rdACEServer.IsChecked = (Server.ServerType == ServerType.ACE);
            rdGDLServer.IsChecked = (Server.ServerType == ServerType.GDL);
            txtServerName.Text = Server.Name;
            txtServerAddress.Text = Server.Address;
            txtServerPort.Text = Server.Port.ToString();
            cmbDefaultRodat.SelectedValue = Server.ReadOnlyDat ? "true" : "false";
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                if (btnAdd.Content.ToString() == "Add")
                    ServerManager.AddNewServer(Server);
                else if (btnAdd.Content.ToString() != "Save")
                {
                    MessageBox.Show("Unexpected content value for btnAdd");
                    return;
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex}");
            }
        }

        private bool ValidateInput()
        {
            if (String.IsNullOrEmpty(txtServerName.Text))
            {
                MessageBox.Show("Server Name required");
                txtServerName.Focus();
                return false;
            }

            if (ServerManager.ServerList.Any(s => s.Name == txtServerName.Text))
            {
                MessageBox.Show("Server Name already exists");
                txtServerName.Focus();
                return false;
            }

            if (String.IsNullOrEmpty(txtServerAddress.Text))
            {
                MessageBox.Show("Server Address required");
                txtServerAddress.Focus();
                return false;
            }

            if (String.IsNullOrEmpty(txtServerPort.Text))
            {
                MessageBox.Show("Server Port required");
                txtServerPort.Focus();
                return false;
            }

            if (!ushort.TryParse(txtServerPort.Text, out var port))
            {
                MessageBox.Show("Server Port must be numeric");
                txtServerPort.Focus();
                return false;
            }

            if (ServerManager.ServerList.Any(s => s.Address == txtServerAddress.Text && s.Port == port))
            {
                MessageBox.Show("Server Address already exists");
                txtServerName.Focus();
                return false;
            }

            if (cmbDefaultRodat.SelectedValue == null)
            {
                MessageBox.Show("Rodat selection required");
                cmbDefaultRodat.Focus();
                return false;
            }

            if (rdACEServer.IsChecked != null && rdACEServer.IsChecked.Value) Server.ServerType = ServerType.ACE;
            if (rdGDLServer.IsChecked != null && rdGDLServer.IsChecked.Value) Server.ServerType = ServerType.GDL;
            Server.Name = txtServerName.Text;
            Server.Address = txtServerAddress.Text;
            Server.Port = port;
            Server.ReadOnlyDat = (cmbDefaultRodat.SelectedValue.ToString() == "true");

            return true;
        }
    }
}
