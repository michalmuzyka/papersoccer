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
        MCTS player1MCTS = null;
        MCTS player2MCTS = null;


        private TaskCompletionSource<Vertex> mouseClickTaskCompletionSource;

        public GameBoard(Strategy player1, Strategy player2)
        {
            InitializeComponent();

            game = new Game(player1, player2);
            CreateMCTSTree(player1, player2);
            drawingManager = new DrawingManager(MainCanvas, game);

            this.Loaded += GameBoard_Loaded;
            MainCanvas.MouseUp += MainCanvas_MouseUp;
            //MakeMove();
            Task.Run(() => RunGame());

        }

        private async Task RunGame() 
        {
            while (!game.IsGameOver)
            {
                // potencjalne uruchomienie MCTS
                // następny ruch
                await MakeMoveV2();

                // weryfikacja gry
                await VerifyGameStatus();

                // poprawianie drzewa mcts
            }
        }

        private void CreateMCTSTree(Strategy player1, Strategy player2) 
        {
            switch (player1) 
            {
                case Strategy.MCTS:
                case Strategy.MCTS_PUCT:
                case Strategy.MCTS_RAVE:
                    player1MCTS = new MCTS(new Node(game, null), false);
                    break;
            }

            switch (player2)
            {
                case Strategy.MCTS:
                case Strategy.MCTS_PUCT:
                case Strategy.MCTS_RAVE:
                    player2MCTS = new MCTS(new Node(game, null), true);
                    break;
            }
        }

        //private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (drawingManager.SelectedPossibleMove != null) 
        //    { 
        //        var move = Point.ToVertex(drawingManager.SelectedPossibleMove);

        //        bool canBounceFromNewPos = game.CanBounceFrom(move.x, move.y);
        //        game.MakeMove(move);
        //        if (!canBounceFromNewPos)
        //            game.PlayerMove = !game.PlayerMove;

        //        VerifyGameStatus();
        //        MakeMove();
        //    }
        //}

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (drawingManager.SelectedPossibleMove != null && mouseClickTaskCompletionSource !=null)
            {
                var move = Point.ToVertex(drawingManager.SelectedPossibleMove);

                mouseClickTaskCompletionSource.SetResult(game.Board[move.x, move.y]);

                return; 

            }
        }


        private async Task<Vertex> WaitForMouseClick() 
        {
            mouseClickTaskCompletionSource = new TaskCompletionSource<Vertex>();
            Vertex selectedVertex = await mouseClickTaskCompletionSource.Task;
            mouseClickTaskCompletionSource = null;

            return selectedVertex;

        }

        private async Task MakeMoveV2() 
        {
            switch (game.CurrentPlayer) 
            {
                case CurentPlayer.Player1:
                    MovePlayer1();
                    break;
                case CurentPlayer.Player2:
                    MovePlayer2();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }


        private async void MovePlayer1() 
        {
            Vertex move = null;
            switch (game.Player1) 
            {
                case Strategy.Player:
                    move = await WaitForMouseClick();
                    break;
                case Strategy.MCTS_PUCT:
                case Strategy.MCTS_RAVE:
                case Strategy.MCTS:
                    break;
                case Strategy.Heuristics:
                    move = game.GetMoveHerestic();
                    break;
            }

            // should switch players
            if (!game.CanBounceFrom(move.X, move.Y)) 
            {
                if (game.CurrentPlayer == CurentPlayer.Player1)
                {
                    game.CurrentPlayer = CurentPlayer.Player2;
                }
                else 
                {
                    game.CurrentPlayer = CurentPlayer.Player1;
                }

                game.PlayerMove = !game.PlayerMove;
            }


            // make move
            game.MakeMoveV2(move);

        }

        private async void MovePlayer2() 
        {

            Vertex move = null;
            switch (game.Player2)
            {
                case Strategy.Player:
                    move = await WaitForMouseClick();
                    break;
                case Strategy.MCTS_PUCT:
                case Strategy.MCTS_RAVE:
                case Strategy.MCTS:
                    break;
                case Strategy.Heuristics:
                    move = game.GetMoveHerestic();
                    break;
            }

            // should switch players
            if (!game.CanBounceFrom(move.X, move.Y))
            {
                if (game.CurrentPlayer == CurentPlayer.Player1)
                {
                    game.CurrentPlayer = CurentPlayer.Player2;
                }
                else
                {
                    game.CurrentPlayer = CurentPlayer.Player1;
                }
                game.PlayerMove = !game.PlayerMove;
            }

            game.MakeMoveV2(move);
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

        private async Task VerifyGameStatus()
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
