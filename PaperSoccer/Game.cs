using System.Transactions;

namespace PaperSoccer
{
    public class Game
    {
        public int MaxX { get; } = 9;
        public int MaxY { get; } = 13;
        public bool PlayerMove { get; set; }
        public Vertex[,] Board { get; private set; }
        public (int X, int Y) BallPosition { get; private set; }
        public Vertex BallPositionVertex { get => Board[BallPosition.X, BallPosition.Y]; }
        public bool IsGameOver { get => BallPositionVertex.IsGoal || BallPositionVertex.NoMoves; }
        public bool PlayerGoal => BallPosition.X >= 3 && BallPosition.X <= 5 && BallPosition.Y == 0;

        public Strategy Player1 { get; set; }
        public Strategy Player2 { get; set; }

        public Players CurrentPlayer { get; set; }


        public Players Winner 
        {
            get 
            {
                if (BallPositionVertex.IsGoal) 
                {
                    if (BallPosition.X >= 3 && BallPosition.X <= 5 && BallPosition.Y == 0)
                    {
                        return Players.Player1;
                    }
                    else 
                    {
                        return Players.Player2;
                    }
                }

                if (BallPositionVertex.NoMoves) 
                {
                    if (CurrentPlayer == Players.Player1)
                    {
                        return Players.Player2;
                    }
                    else 
                    {
                        return Players.Player1;
                    }
                }

                throw new InvalidOperationException();
            }
        }

        public Game(Strategy p1, Strategy p2)
        {
            Player1 = p1;
            Player2 = p2;
            PlayerMove = true;
            CurrentPlayer = Players.Player1;

            BallPosition = (MaxX / 2, MaxY / 2);
            Board = new Vertex[MaxX, MaxY];

            // tworzenie Vertexów
            for(int i = 0; i < Board.GetLength(0); i ++)
            {
                for(int j = 0; j < Board.GetLength(1); j++)
                {
                    if (NotLegitPlace(i, j))
                        continue;

                    Board[i, j] = new Vertex(i, j);
                }
            }

            // przypisanie sąsiadów
            for (int i = 0; i < Board.GetLength(0); i++)
            {
                for (int j = 0; j < Board.GetLength(1); j++)
                {
                    if (NotLegitPlace(i, j))
                        continue;

                    Board[i, j].Neighbors = GetNeighbors(i, j);
                }
            }
        }

        public Game(Vertex[,] board, (int, int) ballPos)
        {
            Board = board;
            BallPosition = ballPos;
        }

        /// <summary>
        /// metoda przeznaczona do realizacji ruchu (zakładamy, że są poprawne)
        /// </summary>
        /// <param name="moves">lista współrzędnych vertexów po których gracz przeszedł w tym ruchu</param>
        public void MakeMove(List<(int X, int Y)> moves)
        {
            foreach(var move in moves)
            {
                var sp = BallPosition; // startPosition
                var startVertex = BallPositionVertex;
                var vertexToGoTo = Board[move.X, move.Y];

                // remove both edges (a -> b and b -> a)
                Board[sp.X, sp.Y].Neighbors.Remove(vertexToGoTo);
                Board[move.X, move.Y].Neighbors.Remove(startVertex);

                BallPosition = move;
            }
        }

        public void MakeMoveV2(Vertex move)
        { 
            Board[BallPositionVertex.X, BallPositionVertex.Y].Neighbors.Remove(move);
            Board[move.X, move.Y].Neighbors.Remove(BallPositionVertex);

            BallPosition = (move.X,move.Y);
        }

        public void MakeMove((int X, int Y) move)
        {
            var sp = BallPosition; // startPosition
            var startVertex = BallPositionVertex;
            var vertexToGoTo = Board[move.X, move.Y];

            // remove both edges (a -> b and b -> a)
            Board[sp.X, sp.Y].Neighbors.Remove(vertexToGoTo);
            Board[move.X, move.Y].Neighbors.Remove(startVertex);

            BallPosition = move;
        }

        /// <summary>
        /// czasami łatwiej zwrócić nowy stan planszy niż listę ruchów
        /// </summary>
        /// <param name="newBoard"></param>
        /// <param name="newBallPos"></param>
        public void MakeMove(Vertex[,] newBoard, (int, int) newBallPos)
        {
            Board = newBoard;
            BallPosition = newBallPos;
        }

        public void AImove()
        {
            Strategy s = Player2;
            if (PlayerMove) //player 1
                s = Player1;

            
            switch (s)
            {
                case Strategy.Heuristics:
                    MakeMove(GetRandomMoves());
                    break;
                case Strategy.MCTS:
                    var mcts = new MCTS(new Node(this, null), true);
                    //mcts.RunSimulation();
                    var bestMove = mcts.GetBestMove();
                    MakeMove(bestMove.Board, bestMove.BallPosition);
                    break;
                case Strategy.MCTS_RAVE:
                    break;
                case Strategy.MCTS_PUCT: 
                    break;
                default:
                    break;
            }
        }

        
        /// <summary>
        /// Zwraca dozwolone ruchy z obecnej pozycji
        /// </summary>
        /// <returns></returns>
        public List<Vertex> GetPossibleMoves()
        {
            return Board[BallPosition.X, BallPosition.Y].Neighbors;
        }

        /// <summary>
        /// Metoda przeznaczona do wyznaczania losowych ruchów komputera (do testowania UI)
        /// </summary>
        /// <returns></returns>
        public List<(int X, int Y)> GetRandomMoves()
        {
            var rng = new Random();
            var moves = new List<(int X, int Y)>();

            var currentPos = BallPosition;
            var currentVertex = BallPositionVertex;

            while(true)
            {
                var (previousX, previousY) = moves.LastOrDefault();

                // trzeba uważać żeby się nie cofnąć
                var nextVertex = currentVertex.Neighbors.Where(v => v.X != previousX && v.Y != previousY)
                    .OrderBy(v => rng.Next()).First();

                var visited = nextVertex.WasVisited;
                var noMoves = nextVertex.NoMoves;
                moves.Add((nextVertex.X, nextVertex.Y));

                if (!visited || noMoves) return moves;
            }
        }

        /// <summary>
        /// Metoda służąca do stwierdzenia, czy na UI należy narysować kreskę pomiędzy dwoma współrzędnymi
        /// </summary>
        /// <returns></returns>
        public bool ShouldDraw(int x1, int y1, int x2, int y2)
        {
            var v1 = Board[x1, y1];
            var v2 = Board[x2, y2];

            var allPossibleNeighbors = GetNeighbors(x1, y1);
            var currentNeighbors = v1.Neighbors;

            return allPossibleNeighbors.Any(v => (v.X == x2 && v.Y == y2)) && // czy teoretycznie powinna być krawędź
                !currentNeighbors.Any(v => (v.X == x2 && v.Y == y2)); // i czy została zużyta
        }

        /// <summary>
        /// Sprawdza czy można odbić się od punktu siatki
        /// </summary>
        public bool CanBounceFrom(int x, int y)
        {
            if (PositionIsOnEdge(x, y))
                return true;

            var v1 = Board[x, y];

            var allPossibleNeighbors = GetNeighbors(x, y);
            var currentNeighbors = v1.Neighbors;

            return allPossibleNeighbors.Count() != currentNeighbors.Count();
        }

        //jak po ruchu wylądujemy poza planszą to jesteśmy na krawędzi
        private bool PositionIsOnEdge(int x, int y)
        {
            return NotLegitPlace(x - 1, y) ||
                   NotLegitPlace(x, y - 1) ||
                   NotLegitPlace(x - 1, y - 1) ||
                   NotLegitPlace(x + 1, y) ||
                   NotLegitPlace(x, y + 1) ||
                   NotLegitPlace(x + 1, y + 1) ||
                   NotLegitPlace(x - 1, y + 1) ||
                   NotLegitPlace(x + 1, y - 1);
        }

        public Game Clone()
        {
            var newBoard = new Vertex[MaxX, MaxY];
            var dictionary = new Dictionary<Vertex, Vertex>();

            for (int i = 0; i < Board.GetLength(0); i++)
            {
                for (int j = 0; j < Board.GetLength(1); j++)
                {
                    if (NotLegitPlace(i, j))
                        continue;

                    var v = new Vertex(i, j);
                    dictionary[Board[i, j]] = v;
                    newBoard[i, j] = v;
                }
            }

            for (int i = 0; i < Board.GetLength(0); i++)
            {
                for (int j = 0; j < Board.GetLength(1); j++)
                {
                    if (NotLegitPlace(i, j))
                        continue;

                    newBoard[i, j].Neighbors = Board[i, j].Neighbors.Select(v => dictionary[v]).ToList();
                }
            }

            return new Game(newBoard, BallPosition);
        }

        // jest kilka miejsc w których nie powinno być Vertexa (rogi planszy) no i poza planszą ofc
        private bool NotLegitPlace(int x, int y)
        {
            return
                x < 3 && (y == 0 || y == MaxY - 1) ||
                x > 5 && (y == 0 || y == MaxY - 1) ||
                x < 0 || x >= MaxX || y < 0 || y >= MaxY;
        }

        public List<Vertex> GetNeighbors(int x, int y)
        {
            List<Vertex> neighbors = new List<Vertex>();

            for(int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == dy && dx == 0) // nie dodajemy krawędzi do samego siebie
                        continue;

                    if (NotLegitPlace(x + dx, y + dy)) // jesteśmy poza planszą
                        continue;

                    if ((x == 0 || x == MaxX - 1) && dx == 0) // pionowe bandy
                        continue;

                    if ((y == 1 || y == MaxY - 2) && !(x == 3 || x == 4 || x == 5) && dy == 0) // poziome bandy (bez słupków)
                        continue;

                    if ((y == 1 || y == MaxY - 2) && x == 3 && dy == 0 && dx != 1) // lewe słupki - nie można w lewo
                        continue;

                    if ((y == 1 || y == MaxY - 2) && x == 5 && dy == 0 && dx != -1) // prawe słupki - nie można w prawo
                        continue;

                    // teraz robimy, żeby ze słupka nie dało się do narożnika bramki
                    if (y == 1  && (x == 3 || x == 5) && dy == -1 && dx == 0) // górne słupki - nie można w górę
                        continue;

                    if (y == 11 && (x == 3 || x == 5) && dy ==  1 && dx == 0) // dolne słupki - nie można w dół
                        continue;

                    //strzał z boku w lewy róg górnej bramki
                    if (y == 1 && x == 2 && dy == -1 && dx == 1) 
                        continue;

                    //strzał z boku w prawy róg górnej bramki
                    if (y == 1 && x == 6 && dy == -1 && dx == -1)
                        continue;


                    //strzał z boku w lewy róg dolnej bramki
                    if (y == 11 && x == 2 && dy == 1 && dx == 1)
                        continue;

                    //strzał z boku w prawy róg dolnej bramki
                    if (y == 11 && x == 6 && dy == 1 && dx == -1)
                        continue;

                    neighbors.Add(Board[x+dx, y+dy]);
                }
            }

            return neighbors;
        }




        public Vertex GetMoveHerestic() 
        {
            Random random = new Random();
            Vertex nextMove;
            
            if (this.CurrentPlayer == Players.Player1)
            {
                double max = this.BallPositionVertex.Neighbors.Max(p => p.Y);

                for (int i = 0; i < 3; i++)
                {
                    var current = max - i;

                    var possibleMoves = this.BallPositionVertex.Neighbors.Where(p => p.Y == current).ToList();
                    var movesWithAdditionalMove = possibleMoves.Where(p => CanBounceFrom(p.X, p.Y)).ToList();

                    if (movesWithAdditionalMove != null && movesWithAdditionalMove.Count() > 0)
                    {
                        return movesWithAdditionalMove[random.Next(movesWithAdditionalMove.Count())];
                    }
                    else if (possibleMoves != null && possibleMoves.Count() > 0)
                    {
                        return possibleMoves[random.Next(possibleMoves.Count())];
                    }
                }
                return null;

            }
            else 
            {
                double min = this.BallPositionVertex.Neighbors.Min(p => p.Y);

                for (int i = 0; i < 3; i++)
                {
                    var current = min + i;

                    var possibleMoves = this.BallPositionVertex.Neighbors.Where(p => p.Y == current).ToList();
                    var movesWithAdditionalMove = possibleMoves.Where(p => CanBounceFrom(p.X, p.Y)).ToList();

                    if (movesWithAdditionalMove != null && movesWithAdditionalMove.Count() > 0)
                    {
                        return movesWithAdditionalMove[random.Next(movesWithAdditionalMove.Count())];
                    }
                    else if (possibleMoves != null && possibleMoves.Count() > 0)
                    {
                        return possibleMoves[random.Next(possibleMoves.Count())];
                    }
                }

                return null;
            }
        }

    }
}