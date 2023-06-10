using PaperSoccer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        public GameBoard(Strategy player1, Strategy player2)
        {
            InitializeComponent();
            game = new Game(player1, player2);
            drawingManager = new DrawingManager(MainCanvas, game);

            this.Loaded += GameBoard_Loaded;
            MainCanvas.MouseUp += MainCanvas_MouseUp;
            MakeMove();
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (drawingManager.SelectedPossibleMove != null) 
            { 
                var move = Point.ToVertex(drawingManager.SelectedPossibleMove);

                bool canBounceFromNewPos = game.CanBounceFrom(move.x, move.y);
                game.MakeMove(move);
                if (!canBounceFromNewPos)
                    game.PlayerMove = !game.PlayerMove;

                VerifyGameStatus();
                MakeMove();
            }
        }

        private async void MakeMove()
        {
            if ((game.PlayerMove && game.Player1 != Strategy.Player) || 
                (!game.PlayerMove && game.Player2 != Strategy.Player))
            {
                await Task.Delay(1000);
                game.AImove();
                game.PlayerMove = !game.PlayerMove;
                VerifyGameStatus();
                MakeMove();
            }
        }

        private void VerifyGameStatus()
        {
            if (game.IsGameOver)
                drawingManager.GameFinished(game.PlayerGoal);
            else if (game.GetPossibleMoves().Count == 0)
                drawingManager.GameFinished(game.PlayerMove);
            else
                drawingManager.Update();
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
