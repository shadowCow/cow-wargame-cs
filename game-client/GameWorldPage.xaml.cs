using CowFst;
using CowDice;
using GameRules;
using CowHexgrid;
using System.Collections.ObjectModel;

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

		BindingContext = new StatusPanel();

		// TODO - source from game state
        var from = new ObservableCollection<Coords>
        {
            new(0, 0),
			new(0, 1),
			new(1, 0),
        };
        FromPicker.ItemsSource = from;
		FromPicker.SelectedIndex = 0;

		// TODO - source from game state
		var to = new ObservableCollection<Coords>
		{
			new (0, 2),
			new (1, 1),
			new (2, 0),
		};
		ToPicker.ItemsSource = to;
		ToPicker.SelectedIndex = 0;

		// TODO - source from game state
		var reinforceQuantity = new ObservableCollection<int>
		{
			0,
			1,
			2,
		};
		ReinforceQuantityPicker.ItemsSource = reinforceQuantity;
		ReinforceQuantityPicker.SelectedIndex = 0;

		var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnCanvasTapped; // Event handler for tap
        DrawingCanvas.GestureRecognizers.Add(tapGesture);
	}

	private void OnFromPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedCoords = (Coords)FromPicker.SelectedItem;
        if (selectedCoords != null)
        {
            // Do something with the selected person
            Console.WriteLine($"Selected From: ({selectedCoords.Q},{selectedCoords.R})");
        }
    }

	private void OnToPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedCoords = (Coords)FromPicker.SelectedItem;
        if (selectedCoords != null)
        {
            // Do something with the selected person
            Console.WriteLine($"Selected To: ({selectedCoords.Q},{selectedCoords.R})");
        }
    }

	private void OnReinforceQuantityPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedQuantity = (int?)ReinforceQuantityPicker.SelectedItem;
        if (selectedQuantity != null)
        {
            // Do something with the selected person
            Console.WriteLine($"Selected Quantity: {selectedQuantity}");
        }
    }

	private void OnPerformActionClicked(object sender, EventArgs e)
    {
        // Code to handle the button click
        Console.WriteLine("Perform Action clicked!");
    }

	private void OnEndTurnPhaseClicked(object sender, EventArgs e)
    {
        // Code to handle the button click
        Console.WriteLine("End Turn Phase clicked!");
    }

	private void OnCanvasTapped(object? sender, TappedEventArgs e)
    {
        // Handle the tap event
        var tapLocation = e.GetPosition(DrawingCanvas);
        Console.WriteLine($"Canvas tapped at: X={tapLocation?.X}, Y={tapLocation?.Y}");

		DrawingCanvas.Invalidate();
    }
}

