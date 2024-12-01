using CowFst;
using CowDice;
using GameRules;

namespace game_client;

using GameFst = Fst<GameState, GameAction, GameEvent, GameError, GameContext>;

public partial class GameWorldPage : ContentPage
{
	public MyCanvasDrawable Drawable { get; private set; }
	private readonly Color backgroundColor = Colors.LightGray;

	public GameWorldPage()
	{
		InitializeComponent();

		NavigationPage.SetHasBackButton(this, false);
		NavigationPage.SetHasNavigationBar(this, false);

		var hexgrid = GameMaps.MapOne();
		var p1StartingTile = hexgrid.GetTileAt(0, 0)!;
		var p2StartingTile = hexgrid.GetTileAt(5, 5)!;
		hexgrid.SetTileAt(0, 0, p1StartingTile with { Owner = TileOwner.Player1, NumUnits = 2 });
		hexgrid.SetTileAt(5, 5, p2StartingTile with { Owner = TileOwner.Player2, NumUnits = 2 });
		GameFst gameFst = GameRules.GameRules.CreateFst(
            "player1",
            "player2",
            hexgrid,
            new GameContext(new RandomDiceRoller(), new RandomDiceRoller()));

		Drawable = new MyCanvasDrawable(gameFst, backgroundColor);
		DrawingCanvas.Drawable = Drawable;
		DrawingCanvas.BackgroundColor = backgroundColor;

		var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnCanvasTapped; // Event handler for tap
        DrawingCanvas.GestureRecognizers.Add(tapGesture);
	}

	private void OnCanvasTapped(object? sender, TappedEventArgs e)
    {
        // Handle the tap event
        var tapLocation = e.GetPosition(DrawingCanvas);
        Console.WriteLine($"Canvas tapped at: X={tapLocation?.X}, Y={tapLocation?.Y}");
        
		DrawingCanvas.Invalidate();
    }
}

