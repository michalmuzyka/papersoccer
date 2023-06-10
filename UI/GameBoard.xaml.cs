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
using System.Windows.Threading;

namespace UI
{
    /// <summary>
    /// Logika interakcji dla klasy GameBoard.xaml
    /// </summary>
    public partial class GameBoard : Page
    {
        DrawingManager drawingManager;
        Game game;

        public GameBoard()
        {
            InitializeComponent();
            game = new Game();
            drawingManager = new DrawingManager(MainCanvas, game);

            this.Loaded += GameBoard_Loaded;
            MainCanvas.MouseUp += MainCanvas_MouseUp;
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            drawingManager.MakeMove();
        }

        private void GameBoard_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            Mouse.Capture(this);
            var pointToWindow = Mouse.GetPosition(MainCanvas);
            drawingManager.DrawBoard(new Point((int)pointToWindow.X, (int)pointToWindow.Y));
            Mouse.Capture(null);
        }
    }
}
