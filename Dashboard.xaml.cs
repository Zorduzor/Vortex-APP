using System;
using System.Windows;
using System.Windows.Controls;

namespace ARDesignAPP
{
    /// <summary>
    /// Logique d'interaction pour Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        /// <summary>
        /// Initialise une nouvelle instance de la classe Dashboard.
        /// </summary>
        public Dashboard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Propriété d'événement qui se déclenche lorsque la fenêtre est fermée.
        /// </summary>
        public Action<object, object> Closed { get; internal set; }

        /// <summary>
        /// Méthode interne pour afficher la fenêtre.
        /// </summary>
        internal new void Show()
        {
            this.Visibility = Visibility.Visible;
        }


        /// <summary>
        /// Gère l'événement 'Click' sur les boutons de la fenêtre.
        /// Navigue vers la page clients.xaml si le bouton clientsButton est cliqué,
        /// ou vers la page Vehicle.xaml si le bouton vehiculesButton est cliqué.
        /// </summary>
        /// <param name="sender">L'objet qui a déclenché l'événement.</param>
        /// <param name="e">Les données de l'événement.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Name == "clientsButton")
                {
                    MainFrame.Navigate(new Uri("clients.xaml", UriKind.Relative));
                }
                else if (button.Name == "vehiculesButton")
                {
                    MainFrame.Navigate(new Uri("Vehicle.xaml", UriKind.Relative));
                }
            }
        }
    }
}
