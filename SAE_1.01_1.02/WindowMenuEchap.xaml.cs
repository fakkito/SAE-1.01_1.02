using System;
using System.Collections.Generic;
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

namespace SAE_1._01_1._02
{
    /// <summary>
    /// Logique d'interaction pour WindowMenuEchap.xaml
    /// </summary>
    public partial class WindowMenuEchap : Window
    {
        public WindowMenuEchap()
        {
            InitializeComponent();

           
        }

        private void ReprendreButton_Click(object sender, RoutedEventArgs e)
        {

            
            this.Close(); // Ferme la fenêtre et retourne au jeu
           
            
        }

        private void Quitter_Click(object sender, RoutedEventArgs e)
        {

            Application.Current.Shutdown(); //  pour fermer l'application
        }

        //private void EchapQuitter(KeyEventArgs e)
        //{
        //    if (e.Key == Key.Escape)
        //    {
        //        this.Close ();
        //    }
        //}


    }
}
