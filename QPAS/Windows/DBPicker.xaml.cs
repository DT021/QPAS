﻿// -----------------------------------------------------------------------
// <copyright file="DBPicker.xaml.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MySql.Data.MySqlClient;

namespace QPAS
{
    /// <summary>
    /// Interaction logic for DBPicker.xaml
    /// </summary>
    public partial class DBPicker : MetroWindow
    {
        public DBPicker()
        {
            InitializeComponent();

            //Hide the tab header
            foreach (TabItem t in Tabs.Items)
            {
                t.Visibility = Visibility.Collapsed;
            }

            MySqlHostTextBox.Text = Properties.Settings.Default.mySqlHost;
            MySqlUsernameTextBox.Text = Properties.Settings.Default.mySqlUsername;
            MySqlPasswordTextBox.Password = "asdf";

            WindowsAuthenticationRadioBtn.IsChecked = Properties.Settings.Default.sqlServerUseWindowsAuthentication;
            SqlServerAuthenticationRadioBtn.IsChecked = !Properties.Settings.Default.sqlServerUseWindowsAuthentication;

            SqlServerHostTextBox.Text = Properties.Settings.Default.sqlServerHost;
            SqlServerUsernameTextBox.Text =
                string.IsNullOrEmpty(Properties.Settings.Default.sqlServerUsername)
                    ? Properties.Settings.Default.sqlServerUsername
                    : "localhost\\SQLEXPRESS";
            SqlServerPasswordTextBox.Password = "asdf";
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            //depending on the choice, go to the appropriate database selection screen
            if (MySqlRadiobtn.IsChecked.HasValue && MySqlRadiobtn.IsChecked.Value)
            {
                Tabs.SelectedIndex = 1;
            }
            else if (SqlServerRadioBtn.IsChecked.HasValue && SqlServerRadioBtn.IsChecked.Value)
            {
                Tabs.SelectedIndex = 2;
            }
            else
            {
                Tabs.SelectedIndex = 3; //sqlite
            }
        }

        private void MySqlTestConnectionBtn_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnection connection = DBUtils.CreateMySqlConnection(
                server: MySqlHostTextBox.Text,
                username: MySqlUsernameTextBox.Text,
                password: MySqlPasswordTextBox.Password,
                noDB: true);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed with error: " + ex.Message);
                return;
            }

            MessageBox.Show("Connection succeeded.");
            connection.Close();
        }

        private void MySqlOKBtn_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnection connection = DBUtils.CreateMySqlConnection(
                server: MySqlHostTextBox.Text,
                username: MySqlUsernameTextBox.Text,
                password: MySqlPasswordTextBox.Password,
                noDB: true);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed with error: " + ex.Message);
                return;
            }
            connection.Close();

            Properties.Settings.Default.mySqlHost = MySqlHostTextBox.Text;
            Properties.Settings.Default.mySqlUsername = MySqlUsernameTextBox.Text;
            Properties.Settings.Default.mySqlPassword = DBUtils.Protect(MySqlPasswordTextBox.Password);
            Properties.Settings.Default.databaseType = "MySql";

            Properties.Settings.Default.Save();

            Close();
        }

        private void SqlServerOKBtn_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection connection = DBUtils.CreateSqlServerConnection(
                server: SqlServerHostTextBox.Text,
                username: SqlServerUsernameTextBox.Text,
                password: SqlServerPasswordTextBox.Password,
                noDB: true);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed with error: " + ex.Message);
                return;
            }

            if (WindowsAuthenticationRadioBtn.IsChecked != null)
                Properties.Settings.Default.sqlServerUseWindowsAuthentication = WindowsAuthenticationRadioBtn.IsChecked.Value;
            Properties.Settings.Default.sqlServerHost = SqlServerHostTextBox.Text;
            Properties.Settings.Default.sqlServerUsername = SqlServerUsernameTextBox.Text;
            Properties.Settings.Default.sqlServerPassword = DBUtils.Protect(SqlServerPasswordTextBox.Password);
            Properties.Settings.Default.databaseType = "SqlServer";

            Properties.Settings.Default.Save();

            Close();
        }

        private void SqlServerTestConnectionBtn_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection connection = DBUtils.CreateSqlServerConnection(
                server: SqlServerHostTextBox.Text,
                username: SqlServerUsernameTextBox.Text,
                password: SqlServerPasswordTextBox.Password,
                noDB: true);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed with error: " + ex.Message);
                return;
            }

            MessageBox.Show("Connection succeeded.");
            connection.Close();
        }
    }
}