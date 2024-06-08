using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PaperSoccer;

public enum GameStatus
{
    OnGoing,
    Draw,
    Win
}

public enum Players 
{
    Player1,
    Player2
}



public enum Strategy
{
    [Display(Name = "Player")]
    Player,
    [Display(Name = "Heuristics")]
    Heuristics,
    [Display(Name = "MCTS")]
    MCTS,
    [Display(Name = "MCTS RAVE")]
    MCTS_RAVE,
    [Display(Name = "MCTS PUCT")]
    MCTS_PUCT,
}
