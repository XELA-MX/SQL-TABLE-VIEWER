using System.Data.SqlClient;
using System.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using System.Data;


namespace SqlTableViewer
{


    public class DataItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string connStr;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (serverNameBox.Text == "")
            {
                MessageBox.Show("Please enter your server name!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (databaseBox.Text == "")
            {
                MessageBox.Show("Please enter the database you are gonna be using!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (usernameBox.Text == "")
            {
                MessageBox.Show("Please enter your username for database login!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (passwordBox.Text == "")
            {
                MessageBox.Show("Please enter your password for database login!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            /*
             

                        EN ESTE PUNTO SE HA REVISADO SI LAS DATABOXES TIENE  INFORMACIÓN
             
             
             */
            // DESACTIVAR TODO
            usernameBox.IsEnabled = false;
            passwordBox.IsEnabled = false;
            serverNameBox.IsEnabled = false;
            databaseBox.IsEnabled = false;
            encryptCheck.IsEnabled = false;
            trustCertificateCheck.IsEnabled = false;
            rememberInfo.IsEnabled = false;

            // ESTABLECER VARIABLES
            string serverName = "Server=" + serverNameBox.Text + ";";
            string databaseName = "Database=" + databaseBox.Text + ";";
            string encryption;
            if (encryptCheck.IsChecked == true)
            {
                encryption = "Encrypt=true;";

            }
            else
            {
                encryption = "Encrypt=false;";
            }

            string serverCertificate;
            if (trustCertificateCheck.IsChecked == true)
            {
                serverCertificate = "TrustServerCertificate=true;";
            }
            else
            {
                serverCertificate = "TrustServerCertificate=false;";
            }

            string username = "User Id=" + usernameBox.Text + ";";
            string password = "Password=" + passwordBox.Text + ";";

            // FORMAR EL CONNECTION STRING
            connStr = serverName + databaseName + encryption + serverCertificate + username + password;

            //TEST CONNECTION
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    title.Text = "Connection Settings - ✅ Connected to " + serverName;
                    title.FontSize = 15;
                    title.Foreground = System.Windows.Media.Brushes.Green;
                    string numeralEncrypt = "0";

                    if (encryptCheck.IsChecked == true)
                    {
                        numeralEncrypt = "1";
                    }

                    string numeralCertificate = "0";

                    if (trustCertificateCheck.IsChecked == true)
                    {
                        numeralCertificate = "1";
                    }


                    if (rememberInfo.IsChecked == true)
                    {

                        //CREAR JSON CON TODOS LOS DATOS
                        List<DataItem> dataItems = new List<DataItem>
                        {
                            //ESTABLECER LO ESENCIAL
                            new DataItem {Key = "ServerName" , Value = serverNameBox.Text},
                            new DataItem {Key = "Db" , Value = databaseBox.Text},
                            new DataItem {Key="Encryption" , Value = numeralEncrypt},
                            new DataItem {Key = "Certificate" , Value=numeralCertificate},
                            new DataItem {Key = "Username" , Value=usernameBox.Text},
                            new DataItem {Key = "Password" , Value=passwordBox.Text}
                        };
                        // CREAR ARCHIVO JSON
                        string jsonString = JsonConvert.SerializeObject(dataItems, Formatting.Indented);
                        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string STVfolder = Path.Combine(documentsPath, "STV");
                        if (!System.IO.Directory.Exists(STVfolder))
                        {
                            System.IO.Directory.CreateDirectory(STVfolder);
                        }

                        string filePath = System.IO.Path.Combine(STVfolder, "info.json");
                        if (File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }

                        File.WriteAllText(filePath, jsonString);

                    }

                    //ADD TO TASBLE COMBO BOX
                    DataTable dataTable = conn.GetSchema("Tables");
                    foreach (DataRow row in dataTable.Rows)
                    {
                        tableComboBox.Items.Add(row[2].ToString());
                    }

                    connSettings.Visibility = Visibility.Hidden;
                    connectedStackPanel.Visibility = Visibility.Visible;
                    conn.Close();
                }
                catch (SqlException ex)
                {
                    //ENABLE EVERYTHING
                    serverNameBox.IsEnabled = true;
                    databaseBox.IsEnabled = true;
                    usernameBox.IsEnabled = true;
                    passwordBox.IsEnabled = true;
                    encryptCheck.IsEnabled = true;
                    trustCertificateCheck.IsEnabled = true;
                    //SHOW MESSAGE BOX
                    MessageBox.Show(ex.Message.ToString(), "CONNECTION ERROR");
                }

            }

        }



        private void Window_Initialized(object sender, EventArgs e)
        {
            if (connSettings.Visibility == Visibility.Hidden)
            {
                connSettings.Visibility = Visibility.Visible;
                connectedStackPanel.Visibility = Visibility.Hidden;
            }

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string STVfolder = Path.Combine(documentsPath, "STV");
            string filePath = System.IO.Path.Combine(STVfolder, "info.json");

            if (File.Exists(filePath))
            {
                rememberInfo.IsChecked = true;

                try
                {
                    string jsonString = File.ReadAllText(filePath);
                    List<DataItem> dataItems = JsonConvert.DeserializeObject<List<DataItem>>(jsonString);
                    int cont = 0;
                    foreach (var item in dataItems)
                    {
                        if (cont == 0)
                        {
                            serverNameBox.Text = item.Value;
                        }
                        else if (cont == 1)
                        {
                            databaseBox.Text = item.Value;
                        }
                        else if (cont == 2)
                        {
                            if (item.Value != "0")
                            {
                                encryptCheck.IsChecked = true;
                            }
                        }
                        else if (cont == 3)
                        {
                            if (item.Value != "0")
                            {
                                trustCertificateCheck.IsChecked = true;
                            }
                        }
                        else if (cont == 4)
                        {
                            usernameBox.Text = item.Value;
                        }
                        else if (cont == 5)
                        {
                            passwordBox.Text = item.Value;
                        }
                        cont++;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void tableComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string selectedTable = tableComboBox.SelectedItem.ToString();
            //SECURITY CHECK TO SEE IF ITS NOTHING
            if (selectedTable == "")
            {
                return;
            }

            //CLEAR THE GRID
            DataTable dt = new DataTable();
            // Asignar el nuevo DataTable al DataGrid
            dataGrid.ItemsSource = dt.DefaultView;
            dataGrid.Columns.Clear();

            //OPEN CONNECTION
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM " + selectedTable;
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable ds = new DataTable();
                    adapter.Fill(ds);

                    foreach (DataColumn column in ds.Columns)
                    {
                        dataGrid.Columns.Add(new System.Windows.Controls.DataGridTextColumn
                        {
                            Header = column.ColumnName,
                            Binding = new System.Windows.Data.Binding(column.ColumnName)
                        });
                    }

                    dataGrid.ItemsSource = ds.DefaultView;

                    conn.Close();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "CONNECTION ERROR");
                    connectedStackPanel.Visibility = Visibility.Hidden;
                    connSettings.Visibility = Visibility.Visible;
                    return;
                }
            }
        }
    }
}