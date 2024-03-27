using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;

namespace ARDesignAPP
{
    /// <summary>
    /// Logique d'interaction pour clients.xaml
    /// </summary>
    public partial class clients : Page
    {
        private readonly IConfiguration _configuration;

        public clients()
        {
            InitializeComponent();

            // Configuration pour accéder à la chaîne de connexion
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            this.Loaded += Page_Loaded;
            ListviewClients.SelectionChanged += ListviewClients_SelectionChanged;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ChargerClients();
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

            ListviewClients.ItemsSource = clients;
        }

        private void ListviewClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListviewClients.SelectedItem is Client client)
            {
                ChargerDetailsClient(client.Id);
            }
            else
            {
                ViderTextBoxClient();
            }
        }

        private void ChargerDetailsClient(int clientId)
        {
            string connectionString = _configuration.GetConnectionString("SqlServerConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT nom, prenom, adresse_postale, ville, numero_telephone, email FROM Clients WHERE id_client = @IdClient";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IdClient", clientId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        nomcompletBox.Text = $"{reader["prenom"]} {reader["nom"]}";
                        adresseBox.Text = reader["adresse_postale"].ToString();
                        villeBox.Text = reader["ville"].ToString();
                        telephoneBox.Text = reader["numero_telephone"].ToString();
                        emailBox.Text = reader["email"].ToString();
                    }
                }
            }
        }

        private void ViderTextBoxClient()
        {
            nomcompletBox.Text = string.Empty;
            adresseBox.Text = string.Empty;
            villeBox.Text = string.Empty;
            telephoneBox.Text = string.Empty;
            emailBox.Text = string.Empty;
        }

        // Classe Client
        public class Client
        {
            public int Id { get; set; }
            public string Nom { get; set; }
            public string? Prenom { get; set; }
            public string? NomComplet => $"{Nom} {Prenom}";
        }

        private void vehiclesButton_Click(object sender, RoutedEventArgs e)
        {
            // Utilisez la méthode Navigate de la NavigationService pour naviguer vers la page vehicle.xaml
            this.NavigationService.Navigate(new Uri("vehicle.xaml", UriKind.Relative));
        }

        private void newClientButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = _configuration.GetConnectionString("SqlServerConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Clients (prenom, nom, adresse_postale, ville, numero_telephone, email) VALUES (@Prenom, @Nom, @Adresse, @Ville, @Telephone, @Email)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Prenom", newPrenomBox.Text);
                cmd.Parameters.AddWithValue("@Nom", newNomBox.Text);
                cmd.Parameters.AddWithValue("@Adresse", newAdresseBox.Text);
                cmd.Parameters.AddWithValue("@Ville", newVilleBox.Text);
                cmd.Parameters.AddWithValue("@Telephone", newTelephoneBox.Text);
                cmd.Parameters.AddWithValue("@Email", newEmailBox.Text);

                cmd.ExecuteNonQuery();
            }

            ViderNewClientTextBoxes();
            ChargerClients(); // Recharger la liste des clients pour afficher le nouveau client
        }

        private void ViderNewClientTextBoxes()
        {
            newPrenomBox.Text = string.Empty;
            newNomBox.Text = string.Empty;
            newAdresseBox.Text = string.Empty;
            newVilleBox.Text = string.Empty;
            newTelephoneBox.Text = string.Empty;
            newEmailBox.Text = string.Empty;
        }

        private void majClientsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListviewClients.SelectedItem is Client selectedClient)
            {
                string connectionString = _configuration.GetConnectionString("SqlServerConnection");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Clients SET prenom = @Prenom, nom = @Nom, adresse_postale = @Adresse, ville = @Ville, numero_telephone = @Telephone, email = @Email WHERE id_client = @IdClient";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@IdClient", selectedClient.Id);
                    cmd.Parameters.AddWithValue("@Prenom", nomcompletBox.Text.Split(' ')[0]);
                    cmd.Parameters.AddWithValue("@Nom", nomcompletBox.Text.Split(' ')[1]);
                    cmd.Parameters.AddWithValue("@Adresse", adresseBox.Text);
                    cmd.Parameters.AddWithValue("@Ville", villeBox.Text);
                    cmd.Parameters.AddWithValue("@Telephone", telephoneBox.Text);
                    cmd.Parameters.AddWithValue("@Email", emailBox.Text);

                    cmd.ExecuteNonQuery();
                }

                ChargerClients(); // Recharger la liste des clients pour afficher les informations mises à jour
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListviewClients.SelectedItem is Client selectedClient)
            {
                // Débogage : Vérifier l'ID du client sélectionné
                Console.WriteLine("Client sélectionné pour suppression : " + selectedClient.Id);

                string connectionString = _configuration.GetConnectionString("SqlServerConnection");

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM Clients WHERE id_client = @IdClient";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@IdClient", selectedClient.Id);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        // Débogage : Vérifier si des lignes ont été affectées
                        Console.WriteLine("Lignes affectées par la suppression : " + rowsAffected);
                    }
                }
                catch (Exception ex)
                {
                    // Afficher le message d'erreur dans la console pour le débogage
                    Console.WriteLine("Erreur lors de la suppression : " + ex.Message);
                }

                ChargerClients(); // Recharger la liste des clients
            }
            else
            {
                // Afficher un message si aucun client n'est sélectionné
                Console.WriteLine("Aucun client n'est sélectionné pour la suppression.");
            }
        }

        private void dashboardButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("Dashboard.xaml", UriKind.Relative));
        }
    }
}
