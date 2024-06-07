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

        public MCTS(Node root, bool topPlayer)
        {
            Root = root;
            TopPlayer = topPlayer;
        }

        public void RunSimulation(int simulationNr)
        {
            for (int i = 0; i < simulationNr; i++) // Liczba symulacji
            {
                var node = Selection();
                int result = Simulation(node);
                Backpropagation(node, result);
            }
        }

        public Node Selection()
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
            var availableMoves = node.GetAllPossibleMoves();
            foreach (var move in availableMoves)
            {
                var gameClone = node.State.Clone();
                gameClone.MakeMove(move.Board, move.BallPosition);
                var newNode = new Node(gameClone, node);
                node.Children.Add(newNode);
            }
            return node.Children.First();
        }

        public int Simulation(Node node)
        {
            // Losowe symulacje, aż do zakończenia gry
            var gameClone = node.State.Clone();
            var topPlayer = TopPlayer;

            while (!node.IsTerminal)
            {
                var availableMoves = node.GetAllPossibleMoves();
                var randomMove = availableMoves[new Random().Next(availableMoves.Count)];
                gameClone.MakeMove(randomMove.Board, randomMove.BallPosition);
                topPlayer = !topPlayer;
                node = new Node(gameClone, node);
            }

            return node.Value * (topPlayer ? -1 : 1);
        }

        public void Backpropagation(Node? node, int result)
        {
            // Aktualizacja wyników węzłów w górę drzewa
            while (node != null)
            {
                node.Visits++;
                node.Wins += result;
                node = node.Parent;
            }
        }

        public Game GetBestMove()
        {
            // Zwraca najlepszy ruch na podstawie statystyk MCTS
            var bestChild = Root.Children.OrderByDescending(c => c.Visits).First();
            //return Root.SelectBestChild().State;
            return bestChild.State;
        }
    }
}
