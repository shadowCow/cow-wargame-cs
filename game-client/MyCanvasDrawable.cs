using CowHexgrid;
using Microsoft.Maui.Graphics;

namespace game_client;

public class MyCanvasDrawable : IDrawable
{
    public Color RectangleColor { get; set; } = Colors.Blue;
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

        // Draw a simple line
        canvas.DrawLine(0, 0, 400, 400);

        // Draw a rectangle
        canvas.FillColor = RectangleColor;
        canvas.FillRectangle(50, 50, 100, 100);

        // Draw a circle
        canvas.FillColor = Colors.Red;
        canvas.FillCircle(100, 550, 50);

        // Draw text
        canvas.FontSize = 24;
        canvas.FontColor = Colors.Black;
        canvas.DrawString("Hello, MAUI!", 150, 300, 200, 100, HorizontalAlignment.Center, VerticalAlignment.Center);

        DrawHexgrid(canvas);
    }

    void DrawHexgrid(ICanvas canvas)
    {
        int numCols = 6;
        int numRows = 5;
        Hexgrid<Coords> hexgrid = new Hexgrid<Coords>(numCols, numRows);

        float hexRadius = 50f;
        float hexApothem = (float)(Math.Sqrt(3) / 2) * hexRadius;
        float firstCx = 500f;
        float firstCy = 100f;

        float cxToCx = hexRadius * 1.5f;

        float cx = firstCx - cxToCx;
        for (var c = 0; c < numCols; c++)
        {
            cx += cxToCx;
            for (var r = 0; r < numRows; r++)
            {
                float cy = firstCy + (hexApothem*2) * r;
                if (c % 2 != 0)
                {
                    cy += hexApothem;
                }
                Console.WriteLine($"c,r: {c},{r}  cx,cy: {cx},{cy}");

                DrawHex(canvas, cx, cy, hexRadius);
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

        // Draw the hexagon path on the canvas
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 2;
        canvas.DrawPath(path);
    }
}
