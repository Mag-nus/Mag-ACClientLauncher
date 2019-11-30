using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
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
        private const string DefaultPublicServerListUri = "https://raw.githubusercontent.com/acresources/serverslist/master/Servers.xml";

        private static readonly HttpClient httpClient = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();

            Title += " 1.5"; // TODO: !!!!! ATTENTION ===== Update line 55 in AssemblyInfo.cs ===== ATTENTION !!!!!

            if (Properties.Settings.Default.WindowPositionLeft > 0 && Properties.Settings.Default.WindowPositionTop > 0)
            {
                Left = Properties.Settings.Default.WindowPositionLeft;
                Top = Properties.Settings.Default.WindowPositionTop;
            }

            if (Properties.Settings.Default.WindowSizeWidth >= MinWidth && Properties.Settings.Default.WindowSizeHeight >= MinHeight)
            {
                Width = Properties.Settings.Default.WindowSizeWidth;
                Height = Properties.Settings.Default.WindowSizeHeight;
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

            ReloadServerBrowserListView();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.WindowPositionLeft = Left;
            Properties.Settings.Default.WindowPositionTop = Top;

            Properties.Settings.Default.WindowSizeWidth = Width;
            Properties.Settings.Default.WindowSizeHeight = Height;

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

            cmdEditServer.IsEnabled = (serversList.Count > 0);
            cmdDeleteServer.IsEnabled = (serversList.Count > 0);

            cmdAddAccounts.IsEnabled = (serversList.Count > 0);

            cmdLaunchChecked.IsEnabled = (serversList.Count > 0);
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

            if (server.EmuType == EmuType.ACE)
                arguments = "-h " + server.Address + " -p " + server.Port + " -a " + account.UserName + " -v " + account.Password;
            else if (server.EmuType == EmuType.GDL)
                arguments = "-h " + server.Address + " -p " + server.Port + " -a " + account.UserName + ":" + account.Password;
            else
            {
                MessageBox.Show($"Launching for server type {server.EmuType} has not been implemented.");
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
                SelectServer(server.Id);
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

                    ReloadServerBrowserListView();
                }
            }
        }

        private void cmdDeleteServer_Click(object sender, RoutedEventArgs e)
        {
            if (cboLauncherServerList.SelectedItem is Server server)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the server:{Environment.NewLine}{server}{Environment.NewLine}{Environment.NewLine}This will also delete any associated accounts!", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    ServerManager.DeleteServerById(server.Id);
                    PopulateServerLists();

                    ReloadServerBrowserListView();
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

        private CancellationTokenSource cmdLaunchAllCTS;

        private async void cmdLaunchChecked_Click(object sender, RoutedEventArgs e)
        {
            cmdLaunchChecked.IsEnabled = false;
            cmdCancelLaunchChecked.IsEnabled = true;

            cmdLaunchAllCTS = new CancellationTokenSource();

            try
            {
                if (cboLauncherServerList.SelectedItem is Server server)
                {
                    foreach (var account in server.Accounts)
                    {
                        if (!cmdCancelLaunchChecked.IsEnabled)
                            return;

                        if (account.Launch)
                        {
                            if (!DoLaunch(server, account))
                                return;

                            if (!cmdCancelLaunchChecked.IsEnabled)
                                return;

                            if (Properties.Settings.Default.IntervalBetweenLaunches > 0)
                                await Task.Delay(Properties.Settings.Default.IntervalBetweenLaunches * 1000, cmdLaunchAllCTS.Token);
                        }
                    }

                    if (!cmdCancelLaunchChecked.IsEnabled)
                        return;

                    await Task.Delay(1000, cmdLaunchAllCTS.Token); // Prevent accidental double clicks
                }
            }
            catch (TaskCanceledException)
            {
                // do nothing
            }
            finally
            {
                cmdCancelLaunchChecked.IsEnabled = false;
                cmdLaunchChecked.IsEnabled = true;
            }
        }

        private void cmdCancelLaunchChecked_Click(object sender, RoutedEventArgs e)
        {
            cmdCancelLaunchChecked.IsEnabled = false;
            cmdLaunchAllCTS.Cancel();
        }


        // =================================
        // ======= SERVER BROWSER TAB ======
        // =================================

        private void ReloadServerBrowserListView()
        {
            var lastUpdated = PublicServerManager.GetLastUpdated();
            lblReloadTime.Content = lastUpdated == DateTime.MinValue ? "" : "Downloaded: " + lastUpdated;

            lstPublicServers.Items.Clear();

            foreach (var server in PublicServerManager.ServerList)
            {
                var localDefinedServer = ServerManager.FindByGuid(server.id);

                if (localDefinedServer == null)
                    server.Action = "Import";
                else
                {
                    if (localDefinedServer.Name != server.name || localDefinedServer.Address != server.server_host || localDefinedServer.Port != server.server_port)
                        server.Action = "Update";
                    else
                        server.Action = null;
                }

                lstPublicServers.Items.Add(server);
            }
        }

        private async void BtnDownloadServerBrowserList_Click(object sender, RoutedEventArgs e)
        {
            btnDownloadServerBrowserList.IsEnabled = false;

            try
            {
                var publicServerListUrl = Properties.Settings.Default.PublicServerListUrl;

                if (String.IsNullOrWhiteSpace(publicServerListUrl) || !Uri.TryCreate(publicServerListUrl, UriKind.Absolute, out var uri))
                {
                    MessageBox.Show("Server list is not a valid Uri:" + Environment.NewLine + publicServerListUrl, "BtnDownloadServerBrowserList_Click", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (await PublicServerManager.UpdateFromPublicServerList(httpClient, uri))
                    ReloadServerBrowserListView();
            }
            finally
            {
                btnDownloadServerBrowserList.IsEnabled = true;
            }
        }

        private void cmdServerBrowserAction_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var server = button.DataContext as ServerItem;

            if (server.Action == "Import")
            {
                if (!ServerManager.TryImport(server))
                {
                    MessageBox.Show("TryImport failed.", "cmdServerBrowserAction_Click", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else if (server.Action == "Update")
            {
                // todo should we add confirmation?

                if (!ServerManager.TryUpdate(server))
                {
                    MessageBox.Show("TryUpdate failed.", "cmdServerBrowserAction_Click", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            //else
            //    MessageBox.Show($"Unknown action: {server.Action}", "cmdServerBrowserAction_Click", MessageBoxButton.OK, MessageBoxImage.Error);

            server.Action = null;

            lstPublicServers.Items.Refresh();

            PopulateServerLists();
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
                Properties.Settings.Default.ACClientLocation = dialog.FileName;
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
                Properties.Settings.Default.DecalInjectLocation = dialog.FileName;
        }

        private void btnPublicServerListDefault_Click(object sender, RoutedEventArgs e)
        {
            var dialogResult = MessageBox.Show("This will reset your public server list to:" + Environment.NewLine + DefaultPublicServerListUri, "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dialogResult == MessageBoxResult.Yes)
                Properties.Settings.Default.PublicServerListUrl = DefaultPublicServerListUri;
        }
    }
}
