using System;
using System.Windows;

namespace Mag_ACClientLauncher.ServerManagement
{
    /// <summary>
    /// Interaction logic for AddServer.xaml
    /// </summary>
    public partial class AddServer : Window
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
