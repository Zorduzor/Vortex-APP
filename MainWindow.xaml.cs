using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Windows.Input;

namespace ARDesignAPP
{
    /// <summary>
    /// Logique d'interaction pour la fenêtre principale (MainWindow.xaml).
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initialise une nouvelle instance de la classe MainWindow.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Charge la configuration à partir du fichier appsettings.json
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Gérer l'appui sur la touche "Entrée" pour valider le bouton de connexion
            emailLoginBox.KeyDown += HandleKeyDown;
            passwordLoginBox.KeyDown += HandleKeyDown;
        }

        /// <summary>
        /// Gère l'événement KeyDown pour les zones de texte d'email et de mot de passe.
        /// Si la touche appuyée est "Entrée", déclenche la méthode de connexion (Login).
        /// </summary>
        /// <param name="sender">L'objet qui a déclenché l'événement.</param>
        /// <param name="e">Les données de l'événement de la touche enfoncée.</param>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login();
            }
        }

        /// <summary>
        /// Gère l'événement Click du bouton de connexion.
        /// Déclenche la méthode de connexion (Login).
        /// </summary>
        /// <param name="sender">L'objet qui a déclenché l'événement.</param>
        /// <param name="e">Les données de l'événement.</param>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        /// <summary>
        /// Tente de connecter l'utilisateur en vérifiant les informations d'identification
        /// par rapport à la base de données d'utilisateurs.
        /// </summary>
        private void Login()
        {
            string connectionString = _configuration.GetConnectionString("SqlServerConnection");

            string email = emailLoginBox.Text;
            string password = passwordLoginBox.Text;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM Utilisateurs WHERE email = @Email AND mot_de_passe = @Password";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Connexion réussie, ouvrir automatiquement la page Dashboard
                            this.Hide();
                            Dashboard dashboard = new Dashboard();
                            dashboard.Closed += (s, args) => this.Close(); // Fermer l'application lorsque la page Dashboard est fermée
                            dashboard.Show();
                        }
                        else
                        {
                            MessageBox.Show("Email ou mot de passe incorrect.");
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Erreur de connexion à la base de données : " + ex.Message);
                }
            }
        }
    }
}
