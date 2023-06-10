using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UI;

public class Consts
{
    public Consts() { }

    public static int BoardHeight { get; set; } = 800;
    public static int BoardWidth { get; set; } = 450;
    public static int GateSizeY { get; set; } = 2;
    public static int GateSizeX { get; set; } = 1;
    public static int BorderSize { get; set; } = 2;
    public static int BallSize { get; set; } = 10;
    public static int PossibleMoveSize { get; set; } = 2;
    public static int SelectPossibleMoveSize { get; set; } = 14;
    public static Brush BackgroundColor { get; set; } = Brushes.SlateGray;
    public static Brush BoardColor { get; set; } = Brushes.DarkGreen;
    public static Brush BorderColor { get; set; } = Brushes.White;
    public static Brush BallColor { get; set; } = Brushes.White;
    public static Brush MoveColor { get; set; } = Brushes.White;
    public static Brush PossibleMoveColor { get; set; } = Brushes.DarkSalmon;
    public static Brush SelectPossibleMoveColor { get; set; } = Brushes.White;
}
