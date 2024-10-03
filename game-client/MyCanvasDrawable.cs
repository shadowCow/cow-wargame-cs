using Microsoft.Maui.Graphics;

namespace game_client;

public class MyCanvasDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Set the stroke and fill color
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 2;

        // Draw a simple line
        canvas.DrawLine(0, 0, 400, 400);

        // Draw a rectangle
        canvas.FillColor = Colors.Blue;
        canvas.FillRectangle(50, 50, 100, 100);

        // Draw a circle
        canvas.FillColor = Colors.Red;
        canvas.FillCircle(700, 550, 50);

        // Draw text
        canvas.FontSize = 24;
        canvas.FontColor = Colors.Black;
        canvas.DrawString("Hello, MAUI!", 150, 300, 200, 100, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}