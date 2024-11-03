using CowFst;
using CowHexgrid;

namespace GameRules;

public static class GameRules
{
    public static Fst<GameState, GameAction, GameEvent, GameError, GameContext> CreateFst(
        string player1Id,
        string player2Id,
        Hexgrid<Tile> hexgrid)
    {
        var initialState = new GameState(
            player1Id,
            player2Id,
            player1Id,
            TurnPhase.Attacking,
            hexgrid,
            new GameStatus.Ongoing());
        var context = new GameContext();

        return new Fst<GameState, GameAction, GameEvent, GameError, GameContext>(
            HandleCommand,
            Transition,
            context,
            initialState
        );
    }

    public static Result<GameEvent, GameError> HandleCommand(
        GameState state,
        GameAction action,
        GameContext context)
    {
        return new Result<GameEvent, GameError>.Err(new GameError.UnknownCommand("dunno"));
    }

    public static GameState Transition(
        GameState state,
        GameEvent evt)
    {
        return state;   
    }
}

public record GameState(
    string Player1Id,
    string Player2Id,
    string PlayerIdForCurrentTurn,
    TurnPhase TurnPhase,
    Hexgrid<Tile> Hexgrid,
    GameStatus Status);

public enum TurnPhase
{
    Attacking,
    Reinforcing,
}

public abstract record GameStatus
{
    private GameStatus() {}

    public sealed record Ongoing() : GameStatus;
    public sealed record Completed(GameOutcome Outcome) : GameStatus;
}

public abstract record GameOutcome
{
    private GameOutcome() {}

    public sealed record Winner(string PlayerId) : GameOutcome;
    public sealed record Tie() : GameOutcome;
}

public abstract record GameAction
{
    private GameAction() {}

    public sealed record Resign(string PlayerId) : GameAction;
    public sealed record Attack(string PlayerId, Coords From, Coords To) : GameAction;
    public sealed record EndAttackPhase(string PlayerId) : GameAction;
    public sealed record Reinforce(string PlayerId, Coords From, Coords To, int Quantity) : GameAction;
    public sealed record EndReinforcePhase(string PlayerId) : GameAction;
}

public abstract record GameEvent
{
    private GameEvent() {}

    public sealed record PlayerResigned(string PlayerId) : GameEvent;
    public sealed record PlayerAttacked(string PlayerId, Coords From, Coords To, int FromLost, int ToLost) : GameEvent;
    public sealed record PlayerEndedAttackPhase(string PlayerId) : GameEvent;
    public sealed record PlayerReinforced(string PlayerId, Coords From, Coords To, int Quantity) : GameEvent;
    public sealed record PlayerEndedReinforcePhase(string PlayerId) : GameEvent;
}

public abstract record GameError
{
    private GameError() {}

    public sealed record UnknownCommand(string CommandName) : GameError;
    public sealed record OutOfTurnError(string PlayerId) : GameError;
    public sealed record InvalidAttackFrom(string PlayerId, Coords From) : GameError;
    public sealed record InvalidAttackTo(string PlayerId, Coords To) : GameError;
    public sealed record ReinforceFromInsufficientQuantity(string PlayerId) : GameError;
    public sealed record ReinforceInvalidFrom(string PlayerId) : GameError;
    public sealed record ReinforceInvalidTo(string PlayerId) : GameError;
}

public record GameContext();
