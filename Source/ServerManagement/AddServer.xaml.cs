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

            rdACEServer.IsChecked = (Server.EmuType == EmuType.ACE);
            rdGDLServer.IsChecked = (Server.EmuType == EmuType.GDL);
            txtServerName.Text = Server.Name;
            txtServerAddress.Text = Server.Address;
            txtServerPort.Text = Server.Port.ToString();
            txtACClientLocationOverride.Text = server.ACClientLocationOverride;
            cmbDefaultRodat.SelectedValue = Server.ReadOnlyDat ? "true" : "false";
            if (server.InjectDecalOverride == 0) cmbInjectDecalOverride.SelectedValue = "No Change";
            if (server.InjectDecalOverride == 1) cmbInjectDecalOverride.SelectedValue = "Yes";
            if (server.InjectDecalOverride == 2) cmbInjectDecalOverride.SelectedValue = "No";
        }

        private void BtnACClientLocationOverride_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.InitialDirectory = "C:\\Turbine\\Asheron's Call";

            // Set filter for file extension and default file extension
            dialog.DefaultExt = ".exe";
            dialog.Filter = "Executables (exe)|*.exe|All files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method
            var result = dialog.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
                txtACClientLocationOverride.Text = dialog.FileName;
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

            if (rdACEServer.IsChecked != null && rdACEServer.IsChecked.Value) Server.EmuType = EmuType.ACE;
            if (rdGDLServer.IsChecked != null && rdGDLServer.IsChecked.Value) Server.EmuType = EmuType.GDL;
            Server.Name = txtServerName.Text;
            Server.Address = txtServerAddress.Text;
            Server.Port = port;
            Server.ACClientLocationOverride = txtACClientLocationOverride.Text;
            Server.ReadOnlyDat = (cmbDefaultRodat.SelectedValue.ToString() == "true");
            if (cmbInjectDecalOverride.SelectedValue.ToString() == "No Change") Server.InjectDecalOverride = 0;
            if (cmbInjectDecalOverride.SelectedValue.ToString() == "Yes") Server.InjectDecalOverride = 1;
            if (cmbInjectDecalOverride.SelectedValue.ToString() == "No") Server.InjectDecalOverride = 2;

            return true;
        }
    }
}
