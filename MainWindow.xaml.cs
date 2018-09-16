using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Mag_ACClientLauncher.ServerManagement;
using Mag_ACClientLauncher.Win32;

namespace Mag_ACClientLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            PopulateServerLists();

            // todo restore previously selected server guid
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // todo save selected server guid

            Properties.Settings.Default.Save();

            base.OnClosing(e);
        }

        private void PopulateServerLists()
        {
            var serversList = ServerManager.ServerList;

            PopulateServerList(cboLauncherServerList, serversList);

            PopulateServerList(cboBulkLauncherServerList, serversList);

            cmdEditServer.IsEnabled = (serversList.Count > 0);
            cmdDeleteServer.IsEnabled = (serversList.Count > 0);

            cmdLaunch.IsEnabled = (serversList.Count > 0);
            cmdBulkLaunch.IsEnabled = (serversList.Count > 0);
        }

        private void PopulateServerList(ComboBox comboBox, ICollection<Server> servers)
        {
            comboBox.Items.Clear();

            foreach (var server in servers)
                comboBox.Items.Add(server);

            comboBox.SelectedIndex = comboBox.Items.Count - 1;
        }

        private void SelectServer(Guid id)
        {
            foreach (var item in cboLauncherServerList.Items)
            {
                if (item is Server serverModel && serverModel.Id == id)
                {
                    cboLauncherServerList.SelectedItem = item;
                    break;
                }
            }

            foreach (var item in cboBulkLauncherServerList.Items)
            {
                if (item is Server serverModel && serverModel.Id == id)
                {
                    cboBulkLauncherServerList.SelectedItem = item;
                    break;
                }
            }
        }

        private bool DoLaunch(Server server, string account, string password)
        {
            var acClientExeLocation = Properties.Settings.Default.ACClientLocation;
            var decalInjectLocation = Properties.Settings.Default.DecalInjectLocation;

            if (String.IsNullOrEmpty(acClientExeLocation) || !File.Exists(acClientExeLocation))
            {
                MessageBox.Show("AC Client location doesn't exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (Properties.Settings.Default.InjectDecal && (String.IsNullOrEmpty(decalInjectLocation) || !File.Exists(decalInjectLocation)))
            {
                MessageBox.Show("Decal location doesn't exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (String.IsNullOrEmpty(server.Address))
            {
                MessageBox.Show("No host provided.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (String.IsNullOrEmpty(account) || String.IsNullOrEmpty(password))
            {
                MessageBox.Show("Invalid account or password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            string arguments;

            if (server.ServerType == ServerType.ACE)
                arguments = "-h " + server.Address + " -p " + server.Port + " -a " + account + " -v " + password;
            else
            {
                MessageBox.Show($"Launching for server type {server.ServerType} has not been implemented.");
                return false;
            }
            

            if (!Properties.Settings.Default.InjectDecal)
            {
                var startInfo = new ProcessStartInfo();
                startInfo.FileName = acClientExeLocation;
                startInfo.Arguments = arguments;
                startInfo.CreateNoWindow = true;
                startInfo.WorkingDirectory = Path.GetDirectoryName(acClientExeLocation) ?? throw new InvalidOperationException();

                Process.Start(startInfo);
            }
            else
            {
                return Injector.RunSuspendedCommaInjectCommaAndResume(acClientExeLocation, arguments, decalInjectLocation, "DecalStartup");
            }

            return true;
        }


        // ================================
        // ========== LAUNCH TAB ==========
        // ================================

        private void cmdAddServer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddServer();
            dialog.Owner = this;

            var result = dialog.ShowDialog();

            if (result == true)
                PopulateServerLists();
        }

        private void cmdEditServer_Click(object sender, RoutedEventArgs e)
        {
            if (cboLauncherServerList.SelectedItem is Server server)
            {
                var dialog = new AddServer(server);
                dialog.Owner = this;

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    PopulateServerLists();
                    SelectServer(server.Id);
                }
            }
        }

        private void cmdDeleteServer_Click(object sender, RoutedEventArgs e)
        {
            if (cboLauncherServerList.SelectedItem is Server server)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the server:{Environment.NewLine}{server}?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    ServerManager.DeleteServerById(server.Id);
                    PopulateServerLists();
                }
            }
        }

        private async void cmdLaunch_Click(object sender, RoutedEventArgs e)
        {
            cmdLaunch.IsEnabled = false;

            try
            {
                if (cboLauncherServerList.SelectedItem is Server server)
                {
                    if (!DoLaunch(server, "user", "password"))
                        return;

                    await Task.Delay(1000); // Prevent accidental double clicks
                }
            }
            finally
            {
                cmdLaunch.IsEnabled = true;
            }
        }


        // =====================================
        // ========== BULK LAUNCH TAB ==========
        // =====================================

        private async void cmdBulkLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (cmdBulkLaunch.Content.ToString() == "Cancel")
            {
                cmdBulkLaunch.Content = "Bulk Launch";
                return;
            }

            cmdBulkLaunch.Content = "Cancel";

            try
            {
                if (cboLauncherServerList.SelectedItem is Server server)
                    await DoBulkLaunch(Properties.Settings.Default.BulkLaunchQuantity, Properties.Settings.Default.BulkLaunchStartIndex, Properties.Settings.Default.BulkLaunchUserNamePrefix, TimeSpan.FromSeconds(Properties.Settings.Default.BulkLaunchInterval), server);
            }
            finally
            {
                cmdBulkLaunch.Content = "Bulk Launch";
            }
        }

        private async Task DoBulkLaunch(int launchQuantity, int startIndex, string userNamePrefix, TimeSpan interval, Server server)
        {
            for (int i = startIndex; i < (startIndex + launchQuantity); i++)
            {
                if (cmdBulkLaunch.Content.ToString() == "Bulk Launch")
                    return;

                var userName = userNamePrefix + i.ToString("0000");

                txtBulkLaunchStatus.Text += $"{DateTime.Now}: Launching user {userName}, connection {(i - startIndex) + 1} of {launchQuantity}" + Environment.NewLine;
                txtBulkLaunchStatus.ScrollToEnd();

                if (!DoLaunch(server, userNamePrefix + i.ToString("0000"), "password"))
                {
                    txtBulkLaunchStatus.Text += $"{DateTime.Now}: Launching user {userName}, connection {(i - startIndex) + 1} of {launchQuantity} FAILED" + Environment.NewLine;
                    txtBulkLaunchStatus.ScrollToEnd();
                    break;
                }

                if (cmdBulkLaunch.Content.ToString() == "Bulk Launch")
                    return;

                await Task.Delay(interval);
            }
        }


        // =================================
        // ========== OPTIONS TAB ==========
        // =================================

        private void btnACClientLocation_Click(object sender, RoutedEventArgs e)
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
                txtACClientLocation.Text = dialog.FileName;
        }

        private void btnDecalLocation_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.InitialDirectory = "C:\\Program Files (x86)\\Decal 3.0";

            // Set filter for file extension and default file extension
            dialog.DefaultExt = ".exe";
            dialog.Filter = "Libraries (dll)|*.dll|All files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method
            var result = dialog.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
                txtDecalInjectLocation.Text = dialog.FileName;
        }
    }
}
