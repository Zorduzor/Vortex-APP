using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ARDesignAPP
{
    /// <summary>
    /// Logique d'interaction pour ClientSelectionDialog.xaml
    /// </summary>
    public partial class ClientSelectionDialog : Window
    {
        private readonly IConfiguration _configuration;

        public Client SelectedClient { get; private set; }

        public ClientSelectionDialog()
        {
            InitializeComponent();
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            ChargerClients();

            // Centrer la fenêtre de dialogue
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void ChargerClients()
        {
            string connectionString = _configuration.GetConnectionString("SqlServerConnection");
            List<Client> clients = new List<Client>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id_client, nom, prenom FROM Clients";
                SqlCommand cmd = new SqlCommand(query, conn);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            Id = reader.GetInt32(0),
                            Nom = reader.GetString(1),
                            Prenom = reader.GetString(2)
                        });
                    }
                }
            }

            listViewClients.ItemsSource = clients;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedClient = listViewClients.SelectedItem as Client;
            if (SelectedClient != null)
            {
                this.DialogResult = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        public int? GetSelectedClientId()
        {
            if (listViewClients.SelectedItem is Client selectedClient)
            {
                return selectedClient.Id;
            }
            return null;
        }

    }

}