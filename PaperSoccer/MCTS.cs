using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperSoccer
{
    public class MCTS
    {
        public Node Root { get; set; }
        public bool TopPlayer { get; set; } // czy ten grający na górze

        public Players TreeForPlayer { get; set; }

        public MCTS(Node root, bool topPlayer, Players player)
        {
            Root = root;
            TopPlayer = topPlayer;
            TreeForPlayer = player;
        }

        public virtual void RunSimulation(int simulationNr)
        {
            for (int i = 0; i < simulationNr; i++) // Liczba symulacji
            {
                var node = Selection();
                var winner = Simulation(node);
                Backpropagation(node, winner);
            }
        }

        public virtual Node Selection()
        {
            // Wybierz węzeł do rozszerzenia (wybierz najlepsze dziecko, dopóki węzeł nie jest liściem)
            var node = Root;
            while (!node.IsTerminal)
            {
                if (node.Children.Count == 0)
                {
                    return Expansion(node);
                }
                else if(node.Children.Any(c => !c.Visited)) // jest jeszcze nieodwiedzone dziecko
                {
                    return node.Children.First(c => !c.Visited);
                }
                else
                {
                    node = node.SelectBestChild();
                }
            }
            return node;
        }


        public Node Expansion(Node node)
        {
            // Rozszerzenie węzła przez dodanie wszystkich możliwych ruchów jako nowe dzieci
            var availableMoves = GetPossibleMovexMCTS(node.State);

            foreach (var move in availableMoves)
            {
                var gameClone = node.State.Clone();
                if (!gameClone.CanBounceFrom(move.X, move.Y))
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

                gameClone.MakeMoveV2(move);
                var newNode = new Node(gameClone, node);
                node.Children.Add(newNode);
            }
            return node.Children.First();
        }

        public virtual Players Simulation(Node node)
        {
            // Losowe symulacje, aż do zakończenia gry
            var gameClone = node.State.Clone();
            var topPlayer = TopPlayer;

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

                
            }
            return gameClone.Winner;
        }


        protected List<Vertex> GetPossibleMovexMCTS(Game game) 
        {
            var retList = new List<Vertex>();
            var ball = game.BallPositionVertex;


            if (ball.Neighbors.Count == 0) 
            {
                return retList;
            }

            foreach ( var neighbor in ball.Neighbors ) 
            {
                retList.Add( neighbor );
            }
            return retList;

        }

        public virtual void Backpropagation(Node? node, Players winner)
        {
            // Aktualizacja wyników węzłów w górę drzewa
            while (node != null)
            {
                node.Visits++;
                //if (node.State.CurrentPlayer == winner) 
                //{
                //    node.Wins++;
                //}

                if (this.TreeForPlayer == winner) 
                {
                    node.Wins++;
                }
                
                node = node.Parent;
            }
        }

        public Vertex GetBestChildV2() 
        {
            if(Root.Children.Count == 0) return null;

            var bestChild = Root.Children[0];
            double bestWinRate = 0;
            double winRate = 0;
            foreach (var child in Root.Children)
            {
                //if (Root.State.CurrentPlayer == child.State.CurrentPlayer)
                //{
                //    winRate = (double)child.Wins / child.Visits;
                //}
                //else 
                //{
                //    winRate = 1 -  (double)child.Wins / child.Visits;
                //}
                winRate = (double)child.Wins / child.Visits;

                if (winRate > bestWinRate)
                {
                    bestWinRate = winRate;
                    bestChild = child;
                }
            }

            return bestChild.State.BallPositionVertex;
        }
    }
}
