namespace PaperSoccer
{
    public class Game
    {
        public int MaxX { get; } = 8;
        public int MaxY { get; } = 12;
        public bool PlayerMove { get; private set; }
        public Vertex[,] Board { get; private set; }
        public (int X, int Y) BallPosition { get; private set; }
        public Vertex BallPositionVertex { get => Board[BallPosition.X, BallPosition.Y]; }
        public bool IsGameOver { get => BallPositionVertex.IsGoal; }

        public Game()
        {
            BallPosition = (MaxX / 2, MaxY / 2);
            Board = new Vertex[MaxX,MaxY];

            // tworzenie Vertexów
            for(int i = 0; i < Board.GetLength(0); i ++)
            {
                for(int j = 0; j < Board.GetLength(1); j++)
                {
                    if (NotLegitPlace(i, j))
                        continue;

                    Board[i, j] = new Vertex { X = i, Y = j };
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
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
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

                    if ((y == 1 || y == MaxY - 2) && x == 5 && dy == 0 && (dx != -1)) // prawe słupki - nie można w prawo
                        continue;

                    // teraz robimy, żeby ze słupka nie dało się do narożnika bramki
                    if (y == 1 && (x == 3 || x == 5) && dy == -1 && dx == 0) // górne słupki - nie można w górę
                        continue;

                    if ((y == 1 || y == MaxY - 2) && x == 5 && dy == 0 && (dx != -1)) // prawe słupki - nie można w
                        continue;

                    neighbors.Add(Board[x+dx, y+dy]);
                }
            }

            return neighbors;
        }
    }
}