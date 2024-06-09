using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PaperSoccer
{
    public class MCTSRAVE : MCTS
    {

        public MCTSRAVE(Node root, bool topPlayer, Players player) : base(root, topPlayer, player)
        {

            
        }


        public override void RunSimulation(int simulationNr)
        {
            for (int i = 0; i < simulationNr; i++) // Liczba symulacji
            {
                var node = Selection();
                var winner = Simulation(node);
                Backpropagation(node, winner.Item1,winner.Item3,winner.Item2);
            }
        }

        public (Players, double[,], double[,]) Simulation(Node node)
        {
            // Losowe symulacje, aż do zakończenia gry
            var gameClone = node.State.Clone();

            var moveList = new List<Vertex>();
            var raveVisitsAdd = new double[9, 13];
            var raveWinsAdd = new double[9, 13];

            while (!gameClone.IsGameOver)
            {
                var availableMoves = GetPossibleMovexMCTS(gameClone);
                var randomMove = availableMoves[new Random().Next(availableMoves.Count)];

                if (!gameClone.CanBounceFrom(randomMove.X, randomMove.Y))
                {
                    if (gameClone.CurrentPlayer == Players.Player1)
                    {
                        gameClone.CurrentPlayer = Players.Player2;
                    }
                    else
                    {
                        gameClone.CurrentPlayer = Players.Player1;
                    }
                    gameClone.PlayerMove = !gameClone.PlayerMove;
                }

                gameClone.MakeMoveV2(randomMove);

                // 
                moveList.Add(randomMove);
                raveVisitsAdd[randomMove.X, randomMove.Y]++;
            }

            if (gameClone.Winner == this.TreeForPlayer) 
            {
                for (var i = 0; i < moveList.Count; i++) 
                {
                    var m = moveList[i];
                    raveWinsAdd[m.X, m.Y]++;
                }
            }

            return (gameClone.Winner, raveVisitsAdd, raveWinsAdd);
        }

        public void Backpropagation(Node? node, Players winner, double[,] raveWins, double[,] raveVisits)
        {

            int rows = raveWins.GetLength(0);
            int cols = raveWins.GetLength(1);
            // Aktualizacja wyników węzłów w górę drzewa
            while (node != null)
            {
                node.Visits++;

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        node.RAVEVisitCount[i, j] += raveVisits[i, j];
                        if (this.TreeForPlayer == winner)
                        {
                            node.RAVEVisitCount[i, j] += raveWins[i, j];
                        }
                        // Access raveWins[i, j] and do something with it
                    }
                }

                if (this.TreeForPlayer == winner) 
                {
                    node.Wins++;
                }
                
                node = node.Parent;
            }
        }

        public override Node Selection()
        {
            var node = Root;
            while (!node.IsTerminal)
            {
                if (node.Children.Count == 0)
                {
                    return Expansion(node);
                }
                else if (node.Children.Any(c => !c.Visited)) // jest jeszcze nieodwiedzone dziecko
                {
                    return node.Children.First(c => !c.Visited);
                }
                else
                {
                    node = SelectBestRaveChild(node);
                }
            }
            return node;
        }


        private Node SelectBestRaveChild(Node node) 
        {
            double bestUCT = UCT.CalculateRave_UCT(node.Children[0], node.Visits,node);
            int bestUCTindex = 0;


            for (int i = 1; i < node.Children.Count; i++)
            {
                var uctValue = UCT.CalculateRave_UCT(node.Children[i], node.Visits,node);
                if (uctValue > bestUCT)
                {
                    bestUCT = uctValue;
                    bestUCTindex = i;
                }
            }

            return node.Children[bestUCTindex];
        }
    }
}
