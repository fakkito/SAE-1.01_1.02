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
    /// Logique d'interaction pour WindowMenu.xaml
    /// </summary>
    public partial class WindowMenu : Window
    {
        public WindowMenu()
        {
            InitializeComponent();
        }


        private void bouttonCredit_Click(object sender, RoutedEventArgs e)
        {
            WindowCredits creditsJeu = new WindowCredits();
            this.Close();
            creditsJeu.ShowDialog();
        }

        private void bouttonQuitter_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Application.Current.Shutdown();
        }
        private void bouttonJouer_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
