using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace ARDesignAPP
{
    /// <summary>
    /// Page de gestion des véhicules.
    /// Permet de gérer les informations liées aux véhicules et leurs clients associés.
    /// </summary>
    public partial class vehicles : Page
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructeur pour la page vehicles.
        /// Initialise la page, charge la configuration et associe les événements.
        /// </summary>
        public vehicles()
        {
            InitializeComponent();
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Association des événements pour la gestion des clients et véhicules.
            this.Loaded += Page_Loaded;
            ListviewClients.SelectionChanged += ListviewClients_SelectionChanged;
            ListviewClientsVehicles.SelectionChanged += ListviewClientsVehicles_SelectionChanged;
            newVehicleButton.Click += newVehicleButton_Click;
        }

        /// <summary>
        /// Appelle la méthode pour charger la liste des clients lors du chargement de la page.
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ChargerClients();
        }

        /// <summary>
        /// Charge la liste des clients depuis la base de données et les affiche dans ListviewClients.
        /// </summary>
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

            ListviewClients.ItemsSource = clients;// Mise à jour de l'interface avec la liste des clients.
        }

        /// <summary>
        /// Gère le changement de sélection dans la liste des clients.
        /// Charge les véhicules associés au client sélectionné.
        /// </summary>
        private void ListviewClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListviewClients.SelectedItem is Client client)
            {
                ChargerVehiculesClient(client.Id);
            }
            else
            {
                ListviewClientsVehicles.ItemsSource = null;
                ViderTextBox();
            }
        }

        /// <summary>
        /// Charge les véhicules associés à un client spécifique depuis la base de données.
        /// </summary>
        private void ChargerVehiculesClient(int clientId)
        {
            string connectionString = _configuration.GetConnectionString("SqlServerConnection");
            List<Vehicule> vehicules = new List<Vehicule>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id_vehicule, marque, modele FROM Vehicules WHERE id_client = @IdClient";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IdClient", clientId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vehicules.Add(new Vehicule
                        {
                            Id = reader.GetInt32(0),
                            Marque = reader.GetString(1),
                            Modele = reader.GetString(2)
                        });
                    }
                }
            }

            ListviewClientsVehicles.ItemsSource = vehicules;
        }

        /// <summary>
        /// Gère le changement de sélection dans la liste des véhicules.
        /// Charge les détails du véhicule sélectionné pour modification ou affichage.
        /// </summary>
        private void ListviewClientsVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListviewClientsVehicles.SelectedItem is Vehicule vehicule)
            {
                ChargerDetailsVehicule(vehicule.Id);
            }
        }

        /// <summary>
        /// Gère l'événement 'Click' du bouton newVehicleButton.
        /// Ouvre un popup pour sélectionner un client et ajoute un nouveau véhicule pour ce client.
        /// </summary>
        private void newVehicleButton_Click(object sender, RoutedEventArgs e)
        {
            int? clientId = OuvrirSelectionClient();
            if (clientId.HasValue)
            {
                AjouterVehicule(clientId.Value);
            }
            else
            {
                MessageBox.Show("Aucun client sélectionné.");
            }
        }


        /// <summary>
        /// Méthode pour ajouter un véhicule avec les informations fournies par l'utilisateur.
        /// </summary>
        private void AjouterVehicule(int clientId)
        {
            // Récupérer les données du véhicule depuis les champs de texte
            string marque = marqueBox.Text;
            string modele = modeleBox.Text;
            string motorisation = motorisationBox.Text;
            // Ajouter la récupération des autres données similaires

            // Valider les données
            if (string.IsNullOrEmpty(marque) || string.IsNullOrEmpty(modele))
            {
                MessageBox.Show("Veuillez saisir la marque et le modèle du véhicule.");
                return;
            }

            // Insérer les données dans la base de données
            string connectionString = _configuration.GetConnectionString("SqlServerConnection");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            INSERT INTO Vehicules (marque, modele, motorisation, id_client)
            VALUES (@Marque, @Modele, @Motorisation, @ClientId)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Marque", marque);
                cmd.Parameters.AddWithValue("@Modele", modele);
                cmd.Parameters.AddWithValue("@Motorisation", motorisation);
                cmd.Parameters.AddWithValue("@ClientId", clientId);

                cmd.ExecuteNonQuery();
            }

            // Afficher un message de succès
            MessageBox.Show("Véhicule ajouté avec succès.");
        }


        /// <summary>
        /// Charge les détails d'un véhicule spécifique depuis la base de données.
        /// </summary>
        private void ChargerDetailsVehicule(int vehiculeId)
        {
            string connectionString = _configuration.GetConnectionString("SqlServerConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT marque, modele, motorisation, kilometrage, numero_homologation, cylindree, puissance_kw, numero_VIN, date_premiere_immatriculation, date_derniere_expertise, date_dernier_entretien, pieces_installees, cvorigina, coupleoriginal, orifile, cvremap, coupleremap, stage, tunefile, boiteavitesse, carburant, ecu, outil, fabricant, modeleecu, software, hardware, brandsoftware FROM Vehicules WHERE id_vehicule = @IdVehicule";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@IdVehicule", vehiculeId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        marqueBox.Text = reader["marque"].ToString();
                        modeleBox.Text = reader["modele"].ToString();
                        motorisationBox.Text = reader["motorisation"].ToString();
                        kilometrageBox.Text = reader["kilometrage"].ToString();
                        numero_homologationBox.Text = reader["numero_homologation"].ToString();
                        cylindreeBox.Text = reader["cylindree"].ToString();
                        puissance_kwBox.Text = reader["puissance_kw"].ToString();
                        numeroVINBox.Text = reader["numero_VIN"].ToString();
                        misencirculBox.Text = reader["date_premiere_immatriculation"].ToString();
                        expertiseBox.Text = reader["date_derniere_expertise"].ToString();
                        entretienBox.Text = reader["date_dernier_entretien"].ToString();
                        piecesinstallBox.Text = reader["pieces_installees"].ToString();
                        cvoriginalBox.Text = reader["cvorigina"].ToString();
                        coupleoriginalBox.Text = reader["coupleoriginal"].ToString();
                        orifileBox.Text = reader["orifile"].ToString();
                        cvtuneBox.Text = reader["cvremap"].ToString();
                        coupletuneBox.Text = reader["coupleremap"].ToString();
                        stageBox.Text = reader["stage"].ToString();
                        tunefileBox.Text = reader["tunefile"].ToString();
                        boiteavitesseBox.Text = reader["boiteavitesse"].ToString();
                        carburantBox.Text = reader["carburant"].ToString();
                        ecuBox.Text = reader["ecu"].ToString();
                        outilBox.Text = reader["outil"].ToString();
                        softwareBox.Text = reader["software"].ToString();
                        hardwareBox.Text = reader["hardware"].ToString();
                        brandsoftwareBox.Text = reader["brandsoftware"].ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Vide tous les champs de texte relatifs aux données du véhicule.
        /// </summary>
        private void ViderTextBox()
        {
            marqueBox.Text = string.Empty;
            modeleBox.Text = string.Empty;
            motorisationBox.Text = string.Empty;
            kilometrageBox.Text = string.Empty;
            numero_homologationBox.Text = string.Empty;
            cylindreeBox.Text = string.Empty;
            puissance_kwBox.Text = string.Empty;
            numeroVINBox.Text = string.Empty;
        }



        // Classe Client
        public class Client
        {
            public int Id { get; set; }
            public string Nom { get; set; }
            public string? Prenom { get; set; }
            public string? NomComplet => $"{Nom} {Prenom}";
        }

        // Classe Vehicule
        public class Vehicule
        {
            public int Id { get; set; }
            public string? Marque { get; set; }
            public string? Modele { get; set; }
        }

        /// <summary>
        /// Gestionnaire d'événements pour le clic sur le bouton ClientsButton.
        /// Redirige vers la page clients.xaml.
        /// </summary>
        private void ClientsButton_Click(object sender, RoutedEventArgs e)
        {
            // Redirige vers la page clients.xaml
            NavigationService?.Navigate(new Uri("clients.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Gestionnaire d'événements pour le clic sur le bouton de mise à jour d'un véhicule.
        /// </summary>
        private void majVehicleButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListviewClientsVehicles.SelectedItem is Vehicule vehicule)
            {
                MettreAJourVehicule(vehicule.Id);
            }
        }

        /// <summary>
        /// Met à jour les informations d'un véhicule spécifié dans la base de données.
        /// </summary>
        /// <param name="vehiculeId">Identifiant du véhicule à mettre à jour.</param>
        private void MettreAJourVehicule(int vehiculeId)
        {
            string connectionString = _configuration.GetConnectionString("SqlServerConnection");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            UPDATE Vehicules 
            SET marque = @Marque, 
                modele = @Modele, 
                motorisation = @Motorisation, 
                kilometrage = @Kilometrage, 
                numero_homologation = @NumeroHomologation, 
                cylindree = @Cylindree, 
                puissance_kw = @PuissanceKW, 
                numero_VIN = @NumeroVIN, 
                date_premiere_immatriculation = @DatePremiereImmatriculation, 
                date_derniere_expertise = @DateDerniereExpertise, 
                date_dernier_entretien = @DateDernierEntretien, 
                pieces_installees = @PiecesInstallees,
                cvorigina = @CVOrigina,
                coupleoriginal = @CoupleOriginal,
                orifile = @OriFile,
                cvremap = @CVRemap,
                coupleremap = @CoupleRemap,
                stage = @Stage,
                tunefile = @TuneFile,
                boiteavitesse = @BoiteAVitesse,
                carburant = @Carburant,
                ecu = @ECU,
                outil = @Outil,
                fabricant = @Fabricant,
                modeleecu = @ModeleECU,
                software = @Software,
                hardware = @Hardware,
                brandsoftware = @BrandSoftware
            WHERE id_vehicule = @IdVehicule";

                SqlCommand cmd = new SqlCommand(query, conn);
                // Ajout des paramètres
                cmd.Parameters.AddWithValue("@Marque", marqueBox.Text);
                cmd.Parameters.AddWithValue("@Modele", modeleBox.Text);
                cmd.Parameters.AddWithValue("@Motorisation", motorisationBox.Text);
                cmd.Parameters.AddWithValue("@Kilometrage", kilometrageBox.Text);
                cmd.Parameters.AddWithValue("@NumeroHomologation", numero_homologationBox.Text);
                cmd.Parameters.AddWithValue("@Cylindree", cylindreeBox.Text);
                cmd.Parameters.AddWithValue("@PuissanceKW", puissance_kwBox.Text);
                cmd.Parameters.AddWithValue("@NumeroVIN", numeroVINBox.Text);
                cmd.Parameters.AddWithValue("@DatePremiereImmatriculation", misencirculBox.Text);
                cmd.Parameters.AddWithValue("@DateDerniereExpertise", expertiseBox.Text);
                cmd.Parameters.AddWithValue("@DateDernierEntretien", entretienBox.Text);
                cmd.Parameters.AddWithValue("@PiecesInstallees", piecesinstallBox.Text);
                cmd.Parameters.AddWithValue("@CVOrigina", cvoriginalBox.Text);
                cmd.Parameters.AddWithValue("@CoupleOriginal", coupleoriginalBox.Text);
                cmd.Parameters.AddWithValue("@OriFile", orifileBox.Text);
                cmd.Parameters.AddWithValue("@CVRemap", cvtuneBox.Text);
                cmd.Parameters.AddWithValue("@CoupleRemap", coupletuneBox.Text);
                cmd.Parameters.AddWithValue("@Stage", stageBox.Text);
                cmd.Parameters.AddWithValue("@TuneFile", tunefileBox.Text);
                cmd.Parameters.AddWithValue("@BoiteAVitesse", boiteavitesseBox.Text);
                cmd.Parameters.AddWithValue("@Carburant", carburantBox.Text);
                cmd.Parameters.AddWithValue("@ECU", ecuBox.Text);
                cmd.Parameters.AddWithValue("@Outil", outilBox.Text);
                cmd.Parameters.AddWithValue("@Software", softwareBox.Text);
                cmd.Parameters.AddWithValue("@Hardware", hardwareBox.Text);
                cmd.Parameters.AddWithValue("@BrandSoftware", brandsoftwareBox.Text);
                cmd.Parameters.AddWithValue("@IdVehicule", vehiculeId);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gestionnaire d'événements pour le clic sur le bouton 'Orifile'.
        /// Déclenche la sélection d'un fichier et la mise à jour du champ correspondant.
        /// </summary>
        private void OrifileButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionnerFichier("orifile");
        }

        /// <summary>
        /// Gestionnaire d'événements pour le clic sur le bouton 'Tunefile'.
        /// Déclenche la sélection d'un fichier et la mise à jour du champ correspondant.
        /// </summary>
        private void TunefileButton_Click(object sender, RoutedEventArgs e)
        {
            SelectionnerFichier("tunefile");
        }

        /// <summary>
        /// Ouvre une boîte de dialogue pour sélectionner un fichier et met à jour le champ de texte correspondant.
        /// </summary>
        /// <param name="fileType">Type de fichier à sélectionner ('orifile' ou 'tunefile').</param>
        private void SelectionnerFichier(string fileType)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                if (fileType == "orifile")
                {
                    orifileBox.Text = selectedFilePath;
                }
                else if (fileType == "tunefile")
                {
                    tunefileBox.Text = selectedFilePath;
                }
            }
        }
        private void orifileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                orifileBox.Text = openFileDialog.FileName;
            }
        }


        private void tunefileButton_Click(object sender, RoutedEventArgs e)
        {
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    tunefileBox.Text = openFileDialog.FileName;
                }
            }
        }

        /// <summary>
        /// Efface le contenu de toutes les TextBox.
        /// </summary>
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Parcourir tous les contrôles enfants de la page
            foreach (var control in GetAllControls(this))
            {
                // Si le contrôle est une TextBox, vider son contenu
                if (control is TextBox textBox)
                {
                    textBox.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// Récupère tous les contrôles enfants de l'élément visuel spécifié.
        /// </summary>
        /// <param name="parent">L'élément visuel parent.</param>
        /// <returns>La liste de tous les contrôles enfants.</returns>
        private IEnumerable<Control> GetAllControls(DependencyObject parent)
        {
            // Vérifier si l'élément parent est nul
            if (parent == null) yield break;

            // Parcourir tous les enfants de l'élément parent
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // Vérifier si l'enfant est un contrôle
                if (child is Control control)
                {
                    // Retourner le contrôle
                    yield return control;
                }

                // Récursivement, obtenir tous les contrôles enfants des enfants actuels
                foreach (var descendant in GetAllControls(child))
                {
                    yield return descendant;
                }
            }
        }
        private int? OuvrirSelectionClient()
        {
            var dialog = new ClientSelectionDialog();
            // Définir la fenêtre parente du dialogue
            if (Application.Current.MainWindow != null)
            {
                dialog.Owner = Application.Current.MainWindow;
            }

            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                return dialog.SelectedClient.Id;
            }
            return null;
        }

        private void dashboardButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("Dashboard.xaml", UriKind.Relative));
        }
    }
}







