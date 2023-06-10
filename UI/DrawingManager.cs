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
    Label Player1;
    Label Player2;
    Label? Winner;

    public Point? SelectedPossibleMove;
    public bool IsGameOver { get; set; }


    public DrawingManager(Canvas boardCanvas, Game game)
    {
        this.canvas = boardCanvas;
        this.game = game;
        mesh = new Mesh(game);

        xOffset = Consts.TileSize;
        yOffset = Consts.TileSize;

        boardCanvas.Height = Consts.TileSize * mesh.Y;
        boardCanvas.Width = Consts.TileSize * mesh.X;

        Border = mesh.BorderEdges.Select(GetBorderLine).ToList();
        mesh.UpdatePossibleMoves();

        Player1 = GetLabel(new Point(1, mesh.Y - 2), Consts.Player1, Consts.BorderColor);
        Player2 = GetLabel(new Point(1, 1), Consts.Player2, Consts.BorderColor);
    }

    public void GameFinished(bool playerWon)
    {
        IsGameOver = true;
        if (playerWon)
            Winner = GetLabel(new Point(mesh.X / 2 - 1, 0), $"Winner: {Consts.Player1}", Consts.BorderColor);
        else
            Winner = GetLabel(new Point(mesh.X / 2 - 1, mesh.Y - 1), $"Winner: {Consts.Player2}", Consts.BorderColor);
    }

    public void Update()
    {
        if (!IsGameOver && 
            ((game.PlayerMove && game.Player1 == Strategy.Player) || 
             (!game.PlayerMove && game.Player2 == Strategy.Player)))
            mesh.UpdatePossibleMoves();
    }

    public void DrawBoard(Point mousePosition)
    {
        canvas.Children.Clear();

        DrawBorder();        
        DrawHistory();
    
        if(!IsGameOver && ((game.PlayerMove && game.Player1 == Strategy.Player) ||
             (!game.PlayerMove && game.Player2 == Strategy.Player)))
            DrawPossibleMoves(mousePosition);

        canvas.Children.Add(GetBall());
    }
    
    private void DrawBorder()
    {
        foreach (var border in Border)
            canvas.Children.Add(border);

        if(Winner != null)
            canvas.Children.Add(Winner);
        else
        {
            if (game.PlayerMove)
                canvas.Children.Add(Player1);
            else
                canvas.Children.Add(Player2);
        }
    }

    private void DrawPossibleMoves(Point mousePosition)
    {
        bool foundNextPoint = false;
        foreach (var possibleMove in mesh.PossibleMoves)
        {
            var possibleMovePoint = possibleMove.Map(xOffset, yOffset);
            if (Utility.PointNearbyOtherPoint(possibleMovePoint, Consts.SelectPossibleMoveSize, mousePosition))
            {
                SelectedPossibleMove = possibleMove;
                foundNextPoint = true;
                canvas.Children.Add(GetSelectPossibleMove(possibleMove));
            }

            if (!foundNextPoint)
                SelectedPossibleMove = null;

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

    private Label GetLabel(Point point, string text, Brush color)
    {
        var label = new Label();
        label.Content = text;
        label.FontSize = 12;
        label.Foreground = color;

        var mapped = point.Map(xOffset, yOffset);

        label.Measure(new System.Windows.Size(Consts.TileSize, Consts.TileSize));
        var x = mapped.x + Consts.TileSize / 2 - label.DesiredSize.Width / 2;
        var y = mapped.y + Consts.TileSize / 2 - label.DesiredSize.Height / 2;

        Canvas.SetLeft(label, x);
        Canvas.SetTop(label, y);

        return label;
    }

}
