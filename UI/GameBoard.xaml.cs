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
using System.Xml.Linq;

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
        private const int SIMULATION_MCTS = 100;

        int update = 0;
        int waitForClick = 0;
        int mouseX = 0;
        int mouseY = 0;

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



                // czekanie sekundy jesli gra komputer vs komputer
                if (game.Player1 != Strategy.Player && game.Player2 != Strategy.Player) {
                    await Task.Delay(1000);
                }
            }

            await VerifyGameStatus();
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


        private void UpdateMCTSTreesAfterMove()
        {
            var searchedBall = this.game.BallPosition;

            if (player1MCTS != null) 
            {
                var searchedGame = player1MCTS.Root.Children.Where(x => searchedBall == x.State.BallPosition).FirstOrDefault();

                if (searchedGame == null)
                {
                    // stworzenie nowego roota
                    var gameClone = player1MCTS.Root.State.Clone();
                    var newNode = new Node(gameClone, null);
                    player1MCTS.Root = newNode;
                }
                else 
                {
                    player1MCTS.Root = searchedGame;
                }

                player1MCTS.Root.Parent = null;
            }

            if (player2MCTS != null)
            {
                var searchedGame = player2MCTS.Root.Children.Where(x => searchedBall == x.State.BallPosition).FirstOrDefault();

                if (searchedGame == null)
                {
                    // stworzenie nowego roota
                    var gameClone = player2MCTS.Root.State.Clone();
                    var newNode = new Node(gameClone, null);
                    player2MCTS.Root = newNode;
                }
                else
                {
                    player2MCTS.Root = searchedGame;
                }

                player2MCTS.Root.Parent = null;
            }
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Interlocked.CompareExchange(ref waitForClick, 1, 1) == 1 && drawingManager.SelectedPossibleMove != null)
            {
                var move = Point.ToVertex(drawingManager.SelectedPossibleMove);

                Interlocked.Exchange(ref mouseX, move.x);
                Interlocked.Exchange(ref mouseY, move.y);
                Interlocked.Exchange(ref waitForClick, 2);
            }
        }


        private Vertex WaitForMouseClick() 
        {
            Interlocked.CompareExchange(ref waitForClick, 1, 0);

            if (Interlocked.CompareExchange(ref waitForClick, 0, 2) == 2)
            {
                var x = Interlocked.Exchange(ref mouseX, 0);
                var y = Interlocked.Exchange(ref mouseY, 0);

                return game.Board[x, y];
            }

            return null;
        }

        private async Task MakeMoveV2() 
        {
            switch (game.CurrentPlayer) 
            {
                case Players.Player1:
                    MovePlayer1();
                    break;
                case Players.Player2:
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
                    move = WaitForMouseClick();
                    break;
                case Strategy.MCTS_PUCT:
                case Strategy.MCTS_RAVE:
                case Strategy.MCTS:
                    player1MCTS.RunSimulation(SIMULATION_MCTS);
                    move = player1MCTS.GetBestChildV2();
                    break;
                case Strategy.Heuristics:
                    move = game.GetMoveHerestic();
                    break;
            }

            if (move != null)
            {
                // should switch players
                if (!game.CanBounceFrom(move.X, move.Y))
                {
                    if (game.CurrentPlayer == Players.Player1)
                    {
                        game.CurrentPlayer = Players.Player2;
                    }
                    else
                    {
                        game.CurrentPlayer = Players.Player1;
                    }

                    game.PlayerMove = !game.PlayerMove;
                }


                // make move
                game.MakeMoveV2(move);

                // poprawianie drzewa mcts
                UpdateMCTSTreesAfterMove();
            }

        }

        private async void MovePlayer2() 
        {

            Vertex move = null;
            switch (game.Player2)
            {
                case Strategy.Player:
                    move = WaitForMouseClick();
                    break;
                case Strategy.MCTS_PUCT:
                case Strategy.MCTS_RAVE:
                case Strategy.MCTS:
                    player2MCTS.RunSimulation(SIMULATION_MCTS);
                    move = player2MCTS.GetBestChildV2();
                    break;
                case Strategy.Heuristics:
                    move = game.GetMoveHerestic();
                    break;
            }

            if (move != null)
            {
                // should switch players
                if (!game.CanBounceFrom(move.X, move.Y))
                {
                    if (game.CurrentPlayer == Players.Player1)
                    {
                        game.CurrentPlayer = Players.Player2;
                    }
                    else
                    {
                        game.CurrentPlayer = Players.Player1;
                    }
                    game.PlayerMove = !game.PlayerMove;
                }

                game.MakeMoveV2(move);

                // poprawianie drzewa mcts
                UpdateMCTSTreesAfterMove();
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

        private async Task VerifyGameStatus()
        {
            if (game.IsGameOver)
                drawingManager.GameFinished(game.PlayerGoal);
            else if (game.GetPossibleMoves().Count == 0)
                drawingManager.GameFinished(game.PlayerMove);
            else
                Interlocked.Exchange(ref update, 1);
        }

        private void GameBoard_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            Mouse.Capture(this, CaptureMode.SubTree);
            var pointToWindow = Mouse.GetPosition(MainCanvas);

            if (Interlocked.CompareExchange(ref update, 0, 1) == 1)
                drawingManager.Update();

            drawingManager.DrawBoard(new Point((int)pointToWindow.X, (int)pointToWindow.Y));
            Mouse.Capture(null);
        }
    }
}
