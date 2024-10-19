using CowHexgrid;
using Microsoft.Maui.Graphics;

namespace game_client;

public class MyCanvasDrawable : IDrawable
{
    public Color RectangleColor { get; set; } = Colors.Blue;
    private readonly float borderSize = 20f;
    public void ToggleRectangleColor()
    {
        Console.WriteLine($"ToggleRectangleColor, before = {RectangleColor}");
        if (RectangleColor != Colors.Blue)
        {
            RectangleColor = Colors.Blue;
        }
        else
        {
            RectangleColor = Colors.Green;
        }
        Console.WriteLine($"ToggleRectangleColor, after = {RectangleColor}");
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        Console.WriteLine($"drawing, RectangleColor = {RectangleColor}");
        // Set the stroke and fill color
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 2;

        DrawHexgrid(canvas, dirtyRect);
    }

    void DrawHexgrid(ICanvas canvas, RectF dirtyRect)
    {
        int numCols = 6;
        int numRows = 5;
        Hexgrid<Coords> hexgrid = new(numCols, numRows);

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

                DrawHex(canvas, cx, cy, radius);
            }
        }
    }

    void DrawHex(ICanvas canvas, float centerX, float centerY, float radius)
    {
        // Path for the hexagon
        PathF path = new PathF();

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

        canvas.FillColor = Colors.Green;
        canvas.FillPath(path);
        
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 2;        
        canvas.DrawPath(path);
    }

    float ComputeHexRadiusSoGridFillsSpace(int numCols, int numRows, float availableWidth, float availableHeight)
    {
        float maxHexHeight = availableHeight / (numRows + 0.5f);
        float maxHexWidth = availableWidth / numCols;

        float maxRadiusFromMaxWidth = maxHexWidth / 2;
        float maxRadiusFromMaxHeight = maxHexHeight / (float)Math.Sqrt(3f);
        
        return Math.Min(maxRadiusFromMaxWidth, maxRadiusFromMaxHeight);
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
