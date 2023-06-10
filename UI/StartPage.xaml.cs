using PaperSoccer;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UI
{
    /// <summary>
    /// Logika interakcji dla klasy StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            StartGame(Enum.GetValues<Strategy>()[p1.SelectedIndex], Enum.GetValues<Strategy>()[p2.SelectedIndex + 1]);
        }

        private void StartGame(Strategy player1, Strategy player2)
        {
            this.NavigationService.Navigate(new GameBoard(player1, player2));
        }
    }
}
