using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperSoccer
{
    public class Node
    {
        public int Wins { get; set; }
        public int Visits { get; set; }
        public Game State { get; set; }
        public List<Node> Children { get; set; }
        public Node? Parent { get; set; }
        public bool Visited { get => Visits != 0; }

        public double puct_weight { get; set; }

        public Node(Game game, Node? parent = null)
        {
            State = game;
            Children = new List<Node>();
            Parent = parent;
        }

        public bool IsTerminal
        {
            get => State.IsGameOver;
        }

        public int Value
        {
            get
            {
                if (!IsTerminal)
                    throw new InvalidOperationException("powinno sie to wywolywac na zakonczonym");

                return State.BallPositionVertex.Value;
            }
        }

        public List<Game> GetAllPossibleMoves()
        {
            var movesFromHere = new List<Game>();

            // na razie tylko ruchy o długości 1
            var ballVertex = State.BallPositionVertex;
            //res = ballVertex.Neighbors.Select(v => new List<(int, int)>() { (v.X, v.Y) }).ToList();

            if(ballVertex.Neighbors.Count == 0)
            {
                return movesFromHere;
            }
            // WSZYSTKIE RUCHY
            // klonujemy Game tyle ile mamy sąsiadów

            foreach (var neighbor in ballVertex.Neighbors)
            {
                var moveToConsider = neighbor.Tuple;
                var newMoves = ConsiderMove(State, moveToConsider, 1);
                movesFromHere.AddRange(newMoves);
            }

            return movesFromHere;
        }

        public List<Game> ConsiderMove(Game startState, (int X, int Y) move, int depth)
        {
            var clone = startState.Clone();
            clone.MakeMove(move);

            var bounce = clone.BallPositionVertex.WasVisited2;
            var noMoves = clone.BallPositionVertex.NoMoves;

            if (!bounce || noMoves)
            {
                return new List<Game>() { clone };
            }

            var movesFromHere = new List<Game>();

            foreach(var neighbor in clone.BallPositionVertex.Neighbors)
            {
                var moveToConsider = neighbor.Tuple;

                // nie sprawdzaj zbyt głęboko
                if(depth >= 2 && neighbor.WasVisited)
                {
                    movesFromHere.Add(clone);
                }

                var newMoves = ConsiderMove(clone, moveToConsider, depth + 1);
                movesFromHere.AddRange(newMoves);
            }
            return movesFromHere;
        }

        public Node SelectBestChild()
        {
            // Wybierz najlepsze dziecko na podstawie współczynnika UCT (Upper Confidence Bound for Trees)

            //return UCT.CalculateUCT

            return Children.OrderByDescending(c => c.Wins / c.Visits + Math.Sqrt(2 * Math.Log(Visits) / c.Visits)).First();
        }


        public void CalculateChildrenNodesWeights() 
        {
            int visitSum = 0;

            foreach (var child in Children)
            {
                visitSum += child.Visits;
            }


            foreach (var child in Children) 
            {
                child.puct_weight = child.Visits / visitSum;
            }
        }

    }
}
