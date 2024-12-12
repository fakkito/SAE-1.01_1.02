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
    /// Logique d'interaction pour WindowCredits.xaml
    /// </summary>
    public partial class WindowCredits : Window
    {
        public WindowCredits()
        {
            InitializeComponent();
        }

        private void retourCredit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainJeu = new MainWindow();
            mainJeu.Show();
            this.Close();
        }
    }
}
