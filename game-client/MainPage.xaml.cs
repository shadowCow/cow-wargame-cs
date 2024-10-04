namespace game_client;

public partial class MainPage : ContentPage
{
	public MyCanvasDrawable Drawable { get; private set; }

	public MainPage()
	{
		InitializeComponent();

		Drawable = new MyCanvasDrawable();
		DrawingCanvas.Drawable = Drawable;

		var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnCanvasTapped; // Event handler for tap
        DrawingCanvas.GestureRecognizers.Add(tapGesture);
	}

	private void OnCanvasTapped(object? sender, TappedEventArgs e)
    {
        // Handle the tap event
        var tapLocation = e.GetPosition(DrawingCanvas);
        Console.WriteLine($"Canvas tapped at: X={tapLocation?.X}, Y={tapLocation?.Y}");
        
        Drawable.ToggleRectangleColor();
		
		DrawingCanvas.Invalidate();
    }
}

