using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PaperSoccer;

public enum GameMode
{
    PlayWithAi,
    WatchAi,
}

public enum GameStatus
{
    OnGoing,
    Draw,
    Win
}

public enum Strategy
{
    Heurystyka,
    MCTS,
    MCTS_RAVE,
}