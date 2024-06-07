using PaperSoccer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UI;

public class Point
{
    public int x;
    public int y;

    Point() { }
    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Point Map(double offsetX, double offsetY)
        => new((int)(x * offsetX), (int)(y * offsetY));

    public static Point FromVertex(Vertex v)
        => new(v.X + 1, v.Y + 1);    
    public static Point FromVertex((int x, int y) v)
        => new(v.x + 1, v.y + 1);

    public static (int x, int y) ToVertex(Point v)
        => (v.x - 1, v.y - 1);
}

public class Connection
{
    public Point p1 { get; set; }
    public Point p2 { get; set; }
    
    public Connection(Point p1, Point p2)
    {
        this.p1 = p1;
        this.p2 = p2;
    }

    public Connection Map(double offsetX, double offsetY)
        => new(p1.Map(offsetX, offsetY), p2.Map(offsetX, offsetY));
}

public class Mesh
{
    Game game { get; set; }

    public int X { get; set; }
    public int Y { get; set; }

    public Point Ball { get => Point.FromVertex(game.BallPositionVertex); }

    public List<Connection> BorderEdges { get; set; } = new List<Connection>();
    public List<Point> PossibleMoves { get; set; } = new List<Point>();

    public static readonly object locker = new object();

    public Mesh(Game game)
    {
        this.game = game;
        // pierwszy i ostatni leży na granicy siatki
        X = game.MaxX + 1;
        Y = game.MaxY + 1;  

        CreateBorder();
        UpdatePossibleMoves();
    }

    public void UpdatePossibleMoves()
    {

        lock (locker)
        {
            PossibleMoves = new List<Point>();

            PossibleMoves.AddRange(game.GetPossibleMoves().Select(Point.FromVertex));
        }
    }

    private void CreateBorder()
    {
        // pion
        for (int y = 1; y < Y - 1; y++)
        {
            var x = y == 1 || y == Y - 2 ? (X - Consts.GateSizeX) / 2 : 1;
            BorderEdges.Add(new Connection(new (x, y), new (x, y + 1)));

            x = y == 1 || y == Y - 2 ? (X + Consts.GateSizeX + 1) / 2 : X - 1;
            BorderEdges.Add(new Connection(new (x, y), new(x, y + 1)));
        }

        // poziom
        for (int x = 1; x < X - 1; x++)
        {
            var y = x >= (X - Consts.GateSizeX) / 2 && x < (X + Consts.GateSizeX + 1) / 2 ? 1 : 2;
            BorderEdges.Add(new Connection(new (x, y), new (x + 1, y)));

            y = x >= (X - Consts.GateSizeX) / 2 && x < (X + Consts.GateSizeX + 1) / 2 ? Y - 1 : Y - 2;
            BorderEdges.Add(new Connection(new (x, y), new (x + 1, y)));
        }
    }

}
