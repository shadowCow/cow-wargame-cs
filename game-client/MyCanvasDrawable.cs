using CowHexgrid;
using GameRules;
using CowFst;
using Microsoft.Maui.Graphics;

namespace game_client;

using GameFst = Fst<GameState, GameAction, GameEvent, GameError, GameContext>;


public class MyCanvasDrawable(GameFst gameFst, Color backgroundColor) : IDrawable
{
    private readonly float borderSize = 20f;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        Console.WriteLine($"Drawing rect, (L, T, W, H): {dirtyRect.Left}, {dirtyRect.Top}, {dirtyRect.Width}, {dirtyRect.Height}");
        DrawGameState(canvas, dirtyRect);
    }

    public void DrawGameState(ICanvas canvas, RectF dirtyRect)
    {
        DrawHexgrid(canvas, dirtyRect, gameFst.GetState().Hexgrid);
    }

    void DrawHexgrid(ICanvas canvas, RectF dirtyRect, Hexgrid<Tile> hexgrid)
    {
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 2;

        int numCols = hexgrid.GetNumColumns();
        int numRows = hexgrid.GetNumRows();

        float availableWidth = dirtyRect.Width - 2 * borderSize;
        float availableHeight = dirtyRect.Height - 2 * borderSize;

        float radius = ComputeHexRadiusSoGridFillsSpace(numCols, numRows, availableWidth, availableHeight);
        float apothem = (float)(Math.Sqrt(3) / 2) * radius;
        float cxToCx = radius * 1.5f;

        float gridWidth = (numCols - 1) * cxToCx + 2 * radius;
        float gridHeight = (apothem * 2 * numRows) + apothem;

        float widthInset = (availableWidth - gridWidth) / 2;
        float heightInset = (availableHeight - gridHeight) / 2;

        float firstCx = borderSize + widthInset + radius;
        float firstCy = borderSize + heightInset + apothem;

        float cx = firstCx - cxToCx;
        for (var c = 0; c < numCols; c++)
        {
            cx += cxToCx;
            for (var r = 0; r < numRows; r++)
            {
                float cy = firstCy + (apothem*2) * r;
                if (c % 2 != 0)
                {
                    cy += apothem;
                }
                Console.WriteLine($"c,r: {c},{r}  cx,cy: {cx},{cy}");

                Tile? tile = hexgrid.GetTileAt(new Coords(c, r));
                if (tile is not null)
                {
                    Console.WriteLine($"drawing tile {c},{r}");
                    DrawHexTile(canvas, cx, cy, radius, apothem, tile);
                }
            }
        }
    }

    void DrawHexTile(ICanvas canvas, float centerX, float centerY, float radius, float apothem, Tile tile)
    {
        // Path for the hexagon
        PathF path = new();

        // Loop to calculate the 6 vertices of the hexagon
        for (int i = 0; i < 6; i++)
        {
            // Angle in radians
            float angle_deg = 60 * i;
            float angle_rad = (float)(Math.PI / 180 * angle_deg);

            // Calculate the x and y coordinates of each vertex
            float x = centerX + radius * (float)Math.Cos(angle_rad);
            float y = centerY + radius * (float)Math.Sin(angle_rad);

            // Move to the first vertex, then draw lines to the other vertices
            if (i == 0)
                path.MoveTo(x, y); // Move to the first vertex
            else
                path.LineTo(x, y); // Draw line to the next vertex
        }

        // Close the hexagon by connecting the last point to the first point
        path.Close();

        canvas.FillColor = GetHexColor(tile);
        canvas.FillPath(path);
        
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 2;        
        canvas.DrawPath(path);

        // draw text overlay
        canvas.FontSize = 16;
        canvas.FontColor = Colors.Black;
        if (tile.Owner != TileOwner.Unowned)
        {
            var playerText = tile.Owner == TileOwner.Player1
                ? "P1"
                : "P2";

            var unitText = tile.NumUnits.ToString();

            var text = $"{playerText}\n{unitText}";
            var textLeft = centerX - (radius/2);
            var textTop = centerY - (apothem/2);
            var textWidthBound = radius;
            var textHeightBound = apothem;
            Console.WriteLine($"drawing string: {text}, {textLeft}, {textTop}, {textWidthBound}, {textHeightBound}");
            canvas.DrawString(
                text,
                textLeft,
                textTop,
                textWidthBound,
                textHeightBound,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);
        }
    }

    static float ComputeHexRadiusSoGridFillsSpace(int numCols, int numRows, float availableWidth, float availableHeight)
    {
        float maxHexHeight = availableHeight / (numRows + 0.5f);
        float maxHexWidth = availableWidth / numCols;

        float maxRadiusFromMaxWidth = maxHexWidth / 2;
        float maxRadiusFromMaxHeight = maxHexHeight / (float)Math.Sqrt(3f);
        
        return Math.Min(maxRadiusFromMaxWidth, maxRadiusFromMaxHeight);
    }

    Color GetHexColor(Tile tile)
    {
        return tile.Terrain switch
        {
            TileTerrain.Grassland => Colors.Yellow,
            TileTerrain.Forest => Colors.Green,
            TileTerrain.Mountain => Colors.Gray,
            TileTerrain.Water => Colors.Blue,
            _ => backgroundColor,
        };
    }

    /*
    To render the game world, with a zoom-able, move-able camera...

    I need to do...

    GameState -> GameWorld

    GameWorld will consist of 'drawables', which have world positions and sizes.
    E.g. lengths in meters. drawables are things like 'lines' or 'paths', etc.

    Then we have a camera. camera is positioned relative to the game world.
    camera can zoom, pan, and move.
    */
}
