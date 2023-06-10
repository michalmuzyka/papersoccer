using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperSoccer
{
    public class Vertex
    {
        /// <summary>
        /// od 0 do 8 (0 to lewa banda; 3-4-5 to bramka; 8 to prawa banda)
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// od 0 do 12 (0 - górna bramka; 12 - dolna bramka; poza bramką dostępne są tylko od 1 do 11)
        /// </summary>
        public int Y { get; set; }

        public (int X, int Y) Tuple { get => (X, Y); }

        /// <summary>
        /// lista sąsiadów (krawędzie) - jeśli ruch został wykonany to należy usunąć sąsiada
        /// </summary>
        public List<Vertex> Neighbors { get; set; }

        public Vertex(int x, int y)
        {
            X = x; 
            Y = y;
            Neighbors = new();
        }
        public Vertex((int X, int Y) tuple)
        {
            X = tuple.X;
            Y = tuple.Y;
            Neighbors = new();
        }

        public bool IsGoal
        {
            get => (X == 3 || X == 4 || X == 5) && (Y == 0 || Y == 12); 
        }

        public bool NoMoves
        {
            get => Neighbors.Count == 0;
        }


        /// <summary>
        /// używane do stwierdzania czy się 'odbijamy' czyli czy po dostawieniu krawędzi
        /// nasza tura się kończy czy też rysujemy kolejną PRZED WYKONANIEM RUCHU
        /// </summary>
        public bool WasVisited
        {
            get => Neighbors.Count != 8;
        }

        /// <summary>
        /// używane do stwierdzania czy się 'odbijamy' czyli czy po dostawieniu krawędzi
        /// nasza tura się kończy czy też rysujemy kolejną PO WYKONANIU RUCHU
        /// </summary>
        public bool WasVisited2
        {
            get => Neighbors.Count != 7;
        }

        /// <summary>
        /// powinno byc sprawdzane tylko jeśli IsTerminal; to value dla gracza z góry; dla oponenta *= -1
        /// </summary>
        public int Value
        {
            get => Y == 0 ? -1 : Y == 12 ? 1 : 0;
        }
    }
}
