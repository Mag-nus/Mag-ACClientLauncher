using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Mag_ACClientLauncher.ServerManagement
{
    /// <summary>
    /// Interaction logic for AddAccounts.xaml
    /// </summary>
    public partial class AddAccounts : Window
    {
        public readonly List<Account> Accounts = new List<Account>();

        public AddAccounts()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
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
            if (!ValidateInput(txtUserName1, txtPassword1)) return false;
            if (!ValidateInput(txtUserName2, txtPassword2)) return false;
            if (!ValidateInput(txtUserName3, txtPassword3)) return false;
            if (!ValidateInput(txtUserName4, txtPassword4)) return false;
            if (!ValidateInput(txtUserName5, txtPassword5)) return false;
            if (!ValidateInput(txtUserName6, txtPassword6)) return false;

            Accounts.Clear();

            if (!String.IsNullOrEmpty(txtUserName1.Text)) Accounts.Add(new Account { UserName = txtUserName1.Text, Password = txtPassword1.Text });
            if (!String.IsNullOrEmpty(txtUserName2.Text)) Accounts.Add(new Account { UserName = txtUserName2.Text, Password = txtPassword2.Text });
            if (!String.IsNullOrEmpty(txtUserName3.Text)) Accounts.Add(new Account { UserName = txtUserName3.Text, Password = txtPassword3.Text });
            if (!String.IsNullOrEmpty(txtUserName4.Text)) Accounts.Add(new Account { UserName = txtUserName4.Text, Password = txtPassword4.Text });
            if (!String.IsNullOrEmpty(txtUserName5.Text)) Accounts.Add(new Account { UserName = txtUserName5.Text, Password = txtPassword5.Text });
            if (!String.IsNullOrEmpty(txtUserName6.Text)) Accounts.Add(new Account { UserName = txtUserName6.Text, Password = txtPassword6.Text });

            return true;
        }

        private bool ValidateInput(TextBox userName, TextBox password)
        {
            if (!String.IsNullOrEmpty(userName.Text))
            {
                if (!IsValidUserName(userName.Text) || !IsValidPassword(userName.Text, password.Text))
                    return false;
            }

            return true;
        }

        internal static bool IsValidUserName(string text)
        {
            if (text.Contains("!") ||
                text.Contains("@") ||
                text.Contains("#") ||
                text.Contains("$") ||
                text.Contains("%") ||
                text.Contains("^") ||
                text.Contains("&") ||
                text.Contains("*") ||
                text.Contains("(") ||
                text.Contains(")") ||
                text.Contains("=") ||
                text.Contains(".") ||
                text.Contains(",") ||
                text.Contains("<") ||
                text.Contains(">") ||
                text.Contains("?") ||
                text.Contains(";") ||
                text.Contains(":") ||
                text.Contains(" ") ||
                String.IsNullOrWhiteSpace(text)
            )
            {
                MessageBox.Show($"Name '{text}' contains an invalid character. Please do not use !@#$%^&*()=.,<>?;:", "Invalid name");
                return false;
            }

            return true;
        }

        internal static bool IsValidPassword(string name, string password)
        {
            if (String.IsNullOrWhiteSpace(password) || password.Contains(" "))
            {
                MessageBox.Show($"Password for account '{name}' may not contain a space.", "Invalid password.");
                return false;
            }

            return true;
        }
    }
}
