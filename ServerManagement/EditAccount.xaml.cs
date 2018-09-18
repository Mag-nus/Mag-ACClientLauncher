using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Mag_ACClientLauncher.ServerManagement
{
    /// <summary>
    /// Interaction logic for EditAccount.xaml
    /// </summary>
    public partial class EditAccount : Window
    {
        public readonly Account Account;

        public EditAccount(Account account)
        {
            InitializeComponent();

            Account = account;

            txtUserName.Text = Account.UserName;
            txtPassword.Text = Account.Password;

            if (account.Characters.Count > 1) txtCharacter1.Text = account.Characters[1];
            if (account.Characters.Count > 2) txtCharacter2.Text = account.Characters[2];
            if (account.Characters.Count > 3) txtCharacter3.Text = account.Characters[3];
            if (account.Characters.Count > 4) txtCharacter4.Text = account.Characters[4];
            if (account.Characters.Count > 5) txtCharacter5.Text = account.Characters[5];
            if (account.Characters.Count > 6) txtCharacter6.Text = account.Characters[6];
            if (account.Characters.Count > 7) txtCharacter7.Text = account.Characters[7];
            if (account.Characters.Count > 8) txtCharacter8.Text = account.Characters[8];
            if (account.Characters.Count > 9) txtCharacter9.Text = account.Characters[9];
            if (account.Characters.Count > 10) txtCharacter10.Text = account.Characters[10];
            if (account.Characters.Count > 11) txtCharacter11.Text = account.Characters[11];
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                    return;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex}");
            }
        }

        private bool ValidateInputs()
        {
            if (!ValidateInput(txtUserName, txtPassword)) return false;

            if (!String.IsNullOrEmpty(txtCharacter1.Text) && !IsValidCharacterName(txtCharacter1.Text)) return false;
            if (!String.IsNullOrEmpty(txtCharacter2.Text) && !IsValidCharacterName(txtCharacter2.Text)) return false;
            if (!String.IsNullOrEmpty(txtCharacter3.Text) && !IsValidCharacterName(txtCharacter3.Text)) return false;
            if (!String.IsNullOrEmpty(txtCharacter4.Text) && !IsValidCharacterName(txtCharacter4.Text)) return false;
            if (!String.IsNullOrEmpty(txtCharacter5.Text) && !IsValidCharacterName(txtCharacter5.Text)) return false;
            if (!String.IsNullOrEmpty(txtCharacter6.Text) && !IsValidCharacterName(txtCharacter6.Text)) return false;
            if (!String.IsNullOrEmpty(txtCharacter7.Text) && !IsValidCharacterName(txtCharacter7.Text)) return false;
            if (!String.IsNullOrEmpty(txtCharacter8.Text) && !IsValidCharacterName(txtCharacter8.Text)) return false;
            if (!String.IsNullOrEmpty(txtCharacter9.Text) && !IsValidCharacterName(txtCharacter9.Text)) return false;
            if (!String.IsNullOrEmpty(txtCharacter10.Text) && !IsValidCharacterName(txtCharacter10.Text)) return false;
            if (!String.IsNullOrEmpty(txtCharacter11.Text) && !IsValidCharacterName(txtCharacter11.Text)) return false;

            Account.UserName = txtUserName.Text;
            Account.Password = txtPassword.Text;

            Account.Characters.Clear();

            Account.Characters.Add("");

            if (!String.IsNullOrEmpty(txtCharacter1.Text)) Account.Characters.Add(txtCharacter1.Text);
            if (!String.IsNullOrEmpty(txtCharacter2.Text)) Account.Characters.Add(txtCharacter2.Text);
            if (!String.IsNullOrEmpty(txtCharacter3.Text)) Account.Characters.Add(txtCharacter3.Text);
            if (!String.IsNullOrEmpty(txtCharacter4.Text)) Account.Characters.Add(txtCharacter4.Text);
            if (!String.IsNullOrEmpty(txtCharacter5.Text)) Account.Characters.Add(txtCharacter5.Text);
            if (!String.IsNullOrEmpty(txtCharacter6.Text)) Account.Characters.Add(txtCharacter6.Text);
            if (!String.IsNullOrEmpty(txtCharacter7.Text)) Account.Characters.Add(txtCharacter7.Text);
            if (!String.IsNullOrEmpty(txtCharacter8.Text)) Account.Characters.Add(txtCharacter8.Text);
            if (!String.IsNullOrEmpty(txtCharacter9.Text)) Account.Characters.Add(txtCharacter9.Text);
            if (!String.IsNullOrEmpty(txtCharacter10.Text)) Account.Characters.Add(txtCharacter10.Text);
            if (!String.IsNullOrEmpty(txtCharacter11.Text)) Account.Characters.Add(txtCharacter11.Text);

            return true;
        }

        private bool ValidateInput(TextBox userName, TextBox password)
        {
            if (!String.IsNullOrEmpty(userName.Text))
            {
                if (!AddAccounts.IsValidUserName(userName.Text) || !AddAccounts.IsValidPassword(userName.Text, password.Text))
                    return false;
            }

            return true;
        }

        internal static bool IsValidCharacterName(string text)
        {
            var temp = text.Replace("'", "a");
            temp = temp.Replace(" ", "a");
            temp = temp.Replace("-", "a");

            if (!temp.All(Char.IsLetter))
            {
                MessageBox.Show($"Character '{text}' contains an invalid character.", "Invalid character");
                return false;
            }

            return true;
        }
    }
}
