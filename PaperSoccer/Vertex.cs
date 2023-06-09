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
        /// od 0 do 7 (0 to lewa banda; 3-4-5 to bramka; 7 to prawa banda)
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// od 0 do 11 (0 - górna bramka; 11 - dolna bramka; poza bramką dostępne są tylko od 1 do 10)
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// lista sąsiadów (krawędzie) - jeśli ruch został wykonany to należy usunąć sąsiada
        /// </summary>
        public List<Vertex> Neighbors { get; set; }

        public bool IsGoal
        {
            get => (X == 3 || X == 4 || X == 5) && (Y == 0 || Y == 11); 
        }

        public bool NoMoves
        {
            get => Neighbors.Count == 0;
        }


        /// <summary>
        /// używane do stwierdzania czy się 'odbijamy' czyli czy po dostawieniu krawędzi
        /// nasza tura się kończy czy też rysujemy kolejną
        /// </summary>
        public bool WasVisited
        {
            get => Neighbors.Count != 8;
        }
    }
}
