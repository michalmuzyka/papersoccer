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

        public List<List<(int X, int Y)>> GetAllPossibleMoves()
        {
            var res = new List<List<(int X, int Y)>>();

            // na razie tylko ruchy o długości 1
            var ballVertex = State.BallPositionVertex;
            res = ballVertex.Neighbors.Select(v => new List<(int, int)>() { (v.X, v.Y) }).ToList();

            return res;
        }

        public Node SelectBestChild()
        {
            // Wybierz najlepsze dziecko na podstawie współczynnika UCT (Upper Confidence Bound for Trees)
            return Children.OrderByDescending(c => c.Wins / (c.Visits == 0 ? -1 : c.Visits) + Math.Sqrt(2 * Math.Log(Visits) / (c.Visits == 0 ? -1 : c.Visits))).First();
        }

        
    }
}
