using PaperSoccer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UI;

public class DrawingManager
{
    Canvas canvas;
    Game game;
    Mesh mesh;

    double xOffset;
    double yOffset;

    List<Line> Border;
    Point? selectedPossibleMove;

    public DrawingManager(Canvas boardCanvas, Game game)
    {
        this.canvas = boardCanvas;
        this.game = game;
        mesh = new Mesh(game);

        xOffset = Consts.BoardWidth / (double)(mesh.X);
        yOffset = Consts.BoardHeight / (double)(mesh.Y);

        Border = mesh.BorderEdges.Select(GetBorderLine).ToList();
        mesh.UpdatePossibleMoves();
    }

    public void DrawBoard(Point mousePosition)
    {
        canvas.Children.Clear();

        foreach(var border in Border)
            canvas.Children.Add(border);

        DrawPossibleMoves(mousePosition);
        DrawHistory();

        canvas.Children.Add(GetBall());
    }

    private void DrawPossibleMoves(Point mousePosition)
    {
        bool foundNextPoint = false;
        foreach (var possibleMove in mesh.PossibleMoves)
        {
            var possibleMovePoint = possibleMove.Map(xOffset, yOffset);
            if (Utility.PointNearbyOtherPoint(possibleMovePoint, Consts.SelectPossibleMoveSize, mousePosition))
            {
                selectedPossibleMove = possibleMove;
                foundNextPoint = true;
                canvas.Children.Add(GetSelectPossibleMove(possibleMove));
            }

            if (!foundNextPoint)
                selectedPossibleMove = null;

            canvas.Children.Add(GetPossibleMove(possibleMove));
        }
    }

    private void DrawHistory() 
    { 
        for(int x = 1; x < mesh.X - 1; ++x)
            for(int y = 1; y < mesh.Y - 1; ++y)
            {
               var v = Point.ToVertex(new Point(x, y));
               var neig = game.GetNeighbors(v.x, v.y);

               if (neig == null)
                   continue;

               foreach(var n in neig)
               {
                    if (game.Board[v.x, v.y] != null && game.ShouldDraw(v.x, v.y, n.X, n.Y))
                        canvas.Children.Add(GetHistoryLine(new Connection(Point.FromVertex((v.x, v.y)), Point.FromVertex((n.X, n.Y)))));
               }
            }
    }

    public void MakeMove()
    {
        if (selectedPossibleMove != null)
        {
            var move = Point.ToVertex(selectedPossibleMove);
            game.MakeMove(move);
            mesh.UpdatePossibleMoves();
            selectedPossibleMove = null;
        }
    }

    private Rectangle GetBall() => GetRectangle(mesh.Ball, Consts.BallColor, Consts.BallSize);
    private Rectangle GetPossibleMove(Point p) => GetRectangle(p, Consts.PossibleMoveColor, Consts.PossibleMoveSize);
    private Rectangle GetSelectPossibleMove(Point p) => GetRectangle(p, Consts.SelectPossibleMoveColor, Consts.SelectPossibleMoveSize, true);
    private Line GetBorderLine(Connection connection) => GetLine(connection, Consts.BorderColor);
    private Line GetHistoryLine(Connection connection) => GetLine(connection, Consts.MoveColor);

    private Line GetLine(Connection connection, Brush stroke)
    {
        connection = connection.Map(xOffset, yOffset);

        var line = new Line
        {
            SnapsToDevicePixels = true,
            StrokeThickness = Consts.BorderSize,
            X1 = connection.p1.x,
            X2 = connection.p2.x,
            Y1 = connection.p1.y,
            Y2 = connection.p2.y,
        };

        line.Stroke = stroke;

        line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
        return line;
    }

    private Rectangle GetRectangle(Point point, Brush color, int size, bool onlyBorder = false)
    {
        var rec = new Rectangle()
        {
            SnapsToDevicePixels = true,
            Height = size,
            Width = size,
            Stroke = color,
            StrokeThickness = 1,
            Fill = onlyBorder ? Brushes.Transparent : color
        };

        rec.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

        point = point.Map(xOffset, yOffset);
        Canvas.SetLeft(rec, point.x - size / 2);
        Canvas.SetTop(rec, point.y - size / 2);

        return rec;

    }

}
