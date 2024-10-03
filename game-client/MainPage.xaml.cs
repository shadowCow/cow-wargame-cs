namespace game_client;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		DrawingCanvas.Drawable = new MyCanvasDrawable();
	}
}

