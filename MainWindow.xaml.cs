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

            if (Properties.Settings.Default.WindowPositionLeft != 0 && Properties.Settings.Default.WindowPositionTop != 0)
            {
                Left = Properties.Settings.Default.WindowPositionLeft;
                Top = Properties.Settings.Default.WindowPositionTop;
            }

            PopulateServerLists();

            if (!String.IsNullOrEmpty(Properties.Settings.Default.SelectedServer))
            {
                try
                {
                    var guid = new Guid(Properties.Settings.Default.SelectedServer);

                    SelectServer(guid);
                }
                catch { /* ignored */ }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.WindowPositionLeft = Left;
            Properties.Settings.Default.WindowPositionTop = Top;

            if (cboLauncherServerList.SelectedItem is Server server)
                Properties.Settings.Default.SelectedServer = server.Id.ToString();

            Properties.Settings.Default.Save();

            ServerManager.SaveServerListToDisk();

            base.OnClosing(e);
        }

        private void PopulateServerLists()
        {
            var serversList = ServerManager.ServerList;

            PopulateServerList(cboLauncherServerList, serversList);

            PopulateServerList(cboBulkLauncherServerList, serversList);

            cmdEditServer.IsEnabled = (serversList.Count > 0);
            cmdDeleteServer.IsEnabled = (serversList.Count > 0);

            cmdAddAccounts.IsEnabled = (serversList.Count > 0);

            cmdLaunchAll.IsEnabled = (serversList.Count > 0);
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
                if (item is Server server && server.Id == id)
                {
                    cboLauncherServerList.SelectedItem = item;

                    SetListAccountsSource(server.Accounts);

                    break;
                }
            }

            foreach (var item in cboBulkLauncherServerList.Items)
            {
                if (item is Server server && server.Id == id)
                {
                    cboBulkLauncherServerList.SelectedItem = item;

                    break;
                }
            }
        }

        private void SetListAccountsSource(IList<Account> accounts)
        {
            lstAccounts.ItemsSource = accounts;

            cmdEditAccount.IsEnabled = (accounts.Count > 0);
            cmdDeleteAccount.IsEnabled = (accounts.Count > 0);
        }

        private bool DoLaunch(Server server, Account account)
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

            if (String.IsNullOrEmpty(account.UserName) || String.IsNullOrEmpty(account.Password))
            {
                MessageBox.Show("Invalid account or password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            string arguments;

            if (server.ServerType == ServerType.ACE)
                arguments = "-h " + server.Address + " -p " + server.Port + " -a " + account.UserName + " -v " + account.Password;
            else if (server.ServerType == ServerType.GDL)
                arguments = "-h " + server.Address + " -p " + server.Port + " -a " + account.UserName + ":" + account.Password;
            else
            {
                MessageBox.Show($"Launching for server type {server.ServerType} has not been implemented.");
                return false;
            }

            // I'm not 100% sure if this is right. ThwargLauncher uses "-rodat on" and "-rodat off", but the client seems to want "-rodat"
            if (server.ReadOnlyDat)
                arguments += " -rodat";


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

        private void cboLauncherServerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboLauncherServerList.SelectedItem is Server server)
                SetListAccountsSource(server.Accounts);
        }

        private void cmdAddServer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddServer();
            dialog.Owner = this;

            var result = dialog.ShowDialog();

            if (result == true)
            {
                ServerManager.AddNewServer(dialog.Server);
                PopulateServerLists();
            }
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
                    ServerManager.SaveServerListToDisk();
                    PopulateServerLists();
                    SelectServer(server.Id);
                }
            }
        }

        private void cmdDeleteServer_Click(object sender, RoutedEventArgs e)
        {
            if (cboLauncherServerList.SelectedItem is Server server)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the server: {server}{Environment.NewLine}{Environment.NewLine}This will also delete any associated accounts!", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    ServerManager.DeleteServerById(server.Id);
                    PopulateServerLists();
                }
            }
        }

        private void cmdAddAccounts_Click(object sender, RoutedEventArgs e)
        {
            if (cboLauncherServerList.SelectedItem is Server server)
            {
                var dialog = new AddAccounts();
                dialog.Owner = this;

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    foreach (var account in dialog.Accounts)
                        server.Accounts.Add(account);

                    cmdEditAccount.IsEnabled = (server.Accounts.Count > 0);
                    cmdDeleteAccount.IsEnabled = (server.Accounts.Count > 0);
                }
            }
        }

        private void cmdEditAccount_Click(object sender, RoutedEventArgs e)
        {
            if (lstAccounts.SelectedItem == null)
                MessageBox.Show("No account selected");
            else
            {
                var account = lstAccounts.SelectedItem as Account;

                var dialog = new EditAccount(account);
                dialog.Owner = this;

                var result = dialog.ShowDialog();

                if (result == true)
                    lstAccounts.Items.Refresh();
            }
        }

        private void cmdDeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (cboLauncherServerList.SelectedItem is Server server)
            {
                if (lstAccounts.SelectedItem == null)
                    MessageBox.Show("No account selected");
                else
                {
                    var account = lstAccounts.SelectedItem as Account;

                    var result = MessageBox.Show($"Are you sure you want to delete the account: {account}", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                        server.Accounts.Remove(account);
                }
            }
        }

        private async void cmdLaunch_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button == null)
                return;

            button.IsEnabled = false;

            try
            {
                if (cboLauncherServerList.SelectedItem is Server server)
                {
                    var index = lstAccounts.Items.IndexOf(button.DataContext);

                    var account = server.Accounts[index];

                    if (!DoLaunch(server, account))
                        return;

                    await Task.Delay(1000); // Prevent accidental double clicks
                }
            }
            finally
            {
                button.IsEnabled = true;
            }
        }

        private async void cmdLaunchAll_Click(object sender, RoutedEventArgs e)
        {
            cmdLaunchAll.IsEnabled = false;

            try
            {
                if (cboLauncherServerList.SelectedItem is Server server)
                {
                    foreach (var account in server.Accounts)
                    {
                        if (account.Launch)
                        {
                            if (!DoLaunch(server, account))
                                return;

                            await Task.Delay(1000);
                        }
                    }
                }
            }
            finally
            {
                cmdLaunchAll.IsEnabled = true;
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
                    await DoBulkLaunch(Properties.Settings.Default.BulkLaunchQuantity, Properties.Settings.Default.BulkLaunchStartIndex, Properties.Settings.Default.BulkLaunchUserNamePrefix, TimeSpan.FromSeconds(Properties.Settings.Default.IntervalBetweenLaunches), server);
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

                var userName = userNamePrefix + i.ToString("00000");

                txtBulkLaunchStatus.Text += $"{DateTime.Now}: Launching user {userName}, connection {(i - startIndex) + 1} of {launchQuantity}" + Environment.NewLine;
                txtBulkLaunchStatus.ScrollToEnd();

                var account = new Account {UserName = userName, Password = "password"};

                if (!DoLaunch(server, account))
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
