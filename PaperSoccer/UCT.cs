using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperSoccer
{

    
    public static class UCT 
    {
        private static readonly double C = Math.Sqrt(2);

        public static double CalculateUCT(Node node, int parentVisitCount) 
        {
            if (node.Visits < 1) return double.MaxValue;

            return node.Wins / node.Visits + C * Math.Sqrt(Math.Log(parentVisitCount) / node.Visits);
        }


        public static double CalculatePuct_UCT(Node node, int parentVisitCount) 
        {
            return 0;
        }




        public static double CalculateRave_UCT(Node node, int parentVisitCount) 
        {
            return 0;
        }
    }
}