using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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

 

 

        public Node SelectBestChild()
        {
            double bestUCT = UCT.CalculateUCT(this.Children[0], this.Visits);
            int bestUCTindex = 0;


            for (int i = 1; i < this.Children.Count; i++)
            {
                var uctValue = UCT.CalculateUCT(this.Children[i], this.Visits);
                if (uctValue > bestUCT)
                {
                    bestUCT = uctValue;
                    bestUCTindex = i;
                }
            }

            return this.Children[bestUCTindex];
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
