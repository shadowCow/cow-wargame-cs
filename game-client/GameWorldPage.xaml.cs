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
    private readonly GameFst gameFst;
    private readonly ObservableCollection<Coords> fromOptions = [];
    private readonly ObservableCollection<Coords> toOptions = [];
    private readonly ObservableCollection<int> reinforceQuantityOptions = [];

	public GameWorldPage()
	{
		InitializeComponent();

		NavigationPage.SetHasBackButton(this, false);
		NavigationPage.SetHasNavigationBar(this, false);

        gameFst = NewGame();

		Drawable = new MyCanvasDrawable(gameFst, backgroundColor);
		DrawingCanvas.Drawable = Drawable;
		DrawingCanvas.BackgroundColor = backgroundColor;

        BindPickerCollections();

        OnGameStateChanged(gameFst.GetState());

		var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnCanvasTapped; // Event handler for tap
        DrawingCanvas.GestureRecognizers.Add(tapGesture);
	}

    private void BindPickerCollections()
    {
        FromPicker.ItemsSource = fromOptions;
		ToPicker.ItemsSource = toOptions;
		ReinforceQuantityPicker.ItemsSource = reinforceQuantityOptions;
    }

    private static GameFst NewGame()
    {
        var hexgrid = GameMaps.MapOne();

        var p1StartingCoords = new Coords(0, 0);
        var p2StartingCoords = new Coords(hexgrid.GetNumColumns() - 1, hexgrid.GetNumRows() - 1);

		var p1StartingTile = hexgrid.GetTileAt(p1StartingCoords)!;
		var p2StartingTile = hexgrid.GetTileAt(p2StartingCoords)!;
		hexgrid.SetTileAt(p1StartingCoords.Q, p1StartingCoords.R, p1StartingTile with { Owner = TileOwner.Player1, NumUnits = 2 });
		hexgrid.SetTileAt(p2StartingCoords.Q, p2StartingCoords.R, p2StartingTile with { Owner = TileOwner.Player2, NumUnits = 2 });
		
        var gameFst = GameRules.GameRules.CreateFst(
            "player1",
            "player2",
            hexgrid,
            new GameContext(new RandomDiceRoller(), new RandomDiceRoller()));
        gameFst.ApplyEvent(new GameEvent.GameStarted());

        return gameFst;
    }

	private void OnFromPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedCoords = (Coords)FromPicker.SelectedItem;
        if (selectedCoords != null)
        {
            Console.WriteLine($"Selected From: ({selectedCoords.Q},{selectedCoords.R})");
            toOptions.Clear();

            // populate 'To' based on 'From' - neighbors
            var hexgrid = gameFst.GetState().Hexgrid;
            var q = selectedCoords.Q;
            var r = selectedCoords.R;
            var nw = hexgrid.GetCoordsNorthwestOf(q, r);
            var n = hexgrid.GetCoordsNorthOf(q, r);
            var ne = hexgrid.GetCoordsNortheastOf(q, r);
            var se = hexgrid.GetCoordsSoutheastOf(q, r);
            var s = hexgrid.GetCoordsSouthOf(q, r);
            var sw = hexgrid.GetCoordsSouthwestOf(q, r);

            if (hexgrid.GetTileAt(nw) is not null)
            {
                toOptions.Add(nw);
            }
            if (hexgrid.GetTileAt(n) is not null)
            {
                toOptions.Add(n);
            }
            if (hexgrid.GetTileAt(ne) is not null)
            {
                toOptions.Add(ne);
            }
            if (hexgrid.GetTileAt(se) is not null)
            {
                toOptions.Add(se);
            }
            if (hexgrid.GetTileAt(s) is not null)
            {
                toOptions.Add(s);
            }
            if (hexgrid.GetTileAt(sw) is not null)
            {
                toOptions.Add(sw);
            }
        }
    }

	private void OnToPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedCoords = (Coords)ToPicker.SelectedItem;
        if (selectedCoords != null)
        {
            Console.WriteLine($"Selected To: ({selectedCoords.Q},{selectedCoords.R})");

            if (gameFst.GetState().TurnPhase == TurnPhase.Reinforcing)
            {
                reinforceQuantityOptions.Clear();
            
                var from = (Coords)FromPicker.SelectedItem;
                var maxReinforcementQuantity = TileUnitLimits.MaximumReinforcementQuantity(
                    gameFst.GetState().Hexgrid.GetTileAt(from),
                    gameFst.GetState().Hexgrid.GetTileAt(selectedCoords));

                foreach (var i in Enumerable.Range(1, maxReinforcementQuantity))
                {
                    reinforceQuantityOptions.Add(i);
                }
            }
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
        switch (gameFst.GetState().TurnPhase)
        {
            case TurnPhase.Attacking:
            {
                var fromCoords = (Coords)FromPicker.SelectedItem;
                var toCoords = (Coords)ToPicker.SelectedItem;

                if (fromCoords is null)
                {
                    DisplayErrorMessage("Select tile to attack from");
                    return;
                }
                if (toCoords is null)
                {
                    DisplayErrorMessage("Select tile to attack");
                    return;
                }

                HandleGameAction(new GameAction.Attack(
                    gameFst.GetState().PlayerIdForCurrentTurn,
                    fromCoords,
                    toCoords));
                break;
            }
            case TurnPhase.Reinforcing:
            {
                var fromCoords = (Coords)FromPicker.SelectedItem;
                var toCoords = (Coords)ToPicker.SelectedItem;
                var quantity = (int?)ReinforceQuantityPicker.SelectedItem;

                if (fromCoords is null)
                {
                    DisplayErrorMessage("Select tile to reinforce from");
                    return;
                }
                if (toCoords is null)
                {
                    DisplayErrorMessage("Select tile to reinforce");
                    return;
                }
                if (quantity is null)
                {
                    DisplayErrorMessage("Select quantity to move");
                    return;
                }

                HandleGameAction(new GameAction.Reinforce(
                    gameFst.GetState().PlayerIdForCurrentTurn,
                    fromCoords,
                    toCoords,
                    quantity.Value));

                break;
            }
        };
    }

	private void OnEndTurnPhaseClicked(object sender, EventArgs e)
    {
        switch (gameFst.GetState().TurnPhase)
        {
            case TurnPhase.Attacking:
                HandleGameAction(new GameAction.EndAttackPhase(gameFst.GetState().PlayerIdForCurrentTurn));
                break;
            case TurnPhase.Reinforcing:
                HandleGameAction(new GameAction.EndReinforcePhase(gameFst.GetState().PlayerIdForCurrentTurn));
                break;
        };
    }

    private async void OnResignClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Confirm Resignation", 
                                         "Are you sure you want to resign?", 
                                         "Yes", 
                                         "No");

        if (answer)
        {
            HandleGameAction(new GameAction.Resign(gameFst.GetState().PlayerIdForCurrentTurn));
        }
    }

    private void HandleGameAction(GameAction action)
    {
        var result = gameFst.HandleCommand(action);
        switch (result)
        {
            case Result<GameEvent, GameError>.Success s:
                OnGameStateChanged(gameFst.GetState());
                break;
            case Result<GameEvent, GameError>.Err e:
                DisplayError(e);
                break;
        }
    }

    private void OnGameStateChanged(GameState gameState)
    {
        CurrentPlayerTurnLabel.Text = gameState.PlayerIdForCurrentTurn;
        TurnPhaseLabel.Text = gameState.TurnPhase.ToString();

        if (gameState.TurnPhase == TurnPhase.Reinforcing)
        {
            ReinforceQuantityPicker.IsEnabled = true;
        }
        else
        {
            ReinforceQuantityPicker.IsEnabled = false;
        }
        
        fromOptions.Clear();
        toOptions.Clear();
        reinforceQuantityOptions.Clear();

        gameState.Hexgrid.EnumerateCoordsByColumn()
            .Where(c => IsOwnedByCurrentPlayer(gameState.Hexgrid.GetTileAt(c), gameState))
            .ToList()
            .ForEach(c => fromOptions.Add(c));

        FromPicker.SelectedIndex = fromOptions.Count > 0
            ? 0
            : -1;

        DrawingCanvas.Invalidate();

        switch (gameState.Status)
        {
            case GameStatus.Completed c:
                var winnerMessage = c.Outcome switch
                {
                    GameOutcome.Winner w => $"{w.PlayerId} wins!",
                    GameOutcome.Tie t => "Tie game!",
                    _ => "", // should never get here
                };

                DisplayGameOver(winnerMessage);

                // yes, we do want to return
                return;
            default:
                break;
        }
    }

    private void DisplayGameOver(string message)
    {
        var task = DisplayAlert(
            "Game Over", 
            $"{message}",
            "Return to Main Menu");

        task.ContinueWith(async t =>
        {
            await Navigation.PopAsync();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }
    
    private static bool IsOwnedByCurrentPlayer(Tile? t, GameState gameState)
    {
        return t?.Owner == TileOwner.Player1 && gameState.IsPlayer1Turn() ||
            t?.Owner == TileOwner.Player2 && gameState.IsPlayer2Turn();
    }

    private void DisplayError(Result<GameEvent, GameError>.Err e)
    {
        Console.WriteLine($"Error: {e.Error}");
    }

    private void DisplayErrorMessage(string message)
    {
        DisplayAlert(
            "Error",
            message,
            "OK"
        );
    }

    private void OnCanvasTapped(object? sender, TappedEventArgs e)
    {
        // Handle the tap event
        var tapLocation = e.GetPosition(DrawingCanvas);
        Console.WriteLine($"Canvas tapped at: X={tapLocation?.X}, Y={tapLocation?.Y}");

		DrawingCanvas.Invalidate();
    }
}

