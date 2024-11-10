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

    public static Fst<GameState, GameAction, GameEvent, GameError, GameContext> CreateFst(
        GameState initialState)
    {
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
        return action switch
        {
            GameAction.Attack a => HandleAttack(state, a, context),
            GameAction.EndAttackPhase a => HandleEndAttackPhase(state, a, context),
            GameAction.Reinforce a => HandleReinforce(state, a, context),
            GameAction.EndReinforcePhase a => HandleEndReinforcePhase(state, a, context),
            GameAction.Resign a => HandleResign(state, a, context),
            _ => new Result<GameEvent, GameError>.Err(new GameError.UnknownCommand(nameof(action))),
        };
    }

    private static Result<GameEvent, GameError> HandleAttack(GameState state, GameAction.Attack a, GameContext context)
    {
        throw new NotImplementedException();
    }

    private static Result<GameEvent, GameError> HandleEndAttackPhase(GameState state, GameAction.EndAttackPhase a, GameContext context)
    {
        if (state.PlayerIdForCurrentTurn != a.PlayerId)
        {
            return OutOfTurnResult(a.PlayerId);
        }

        return new Result<GameEvent, GameError>.Success(new GameEvent.PlayerEndedAttackPhase(a.PlayerId));
    }

    private static Result<GameEvent, GameError> HandleReinforce(GameState state, GameAction.Reinforce a, GameContext context)
    {
        if (state.PlayerIdForCurrentTurn != a.PlayerId)
        {
            return OutOfTurnResult(a.PlayerId);
        }
        if (state.TurnPhase != TurnPhase.Reinforcing)
        {
            return new Result<GameEvent, GameError>.Err(new GameError.ActionOutOfPhase(a.PlayerId, state.TurnPhase));
        }

        var fromTile = state.Hexgrid.GetTileAt(a.From.Q, a.From.R);
        var toTile = state.Hexgrid.GetTileAt(a.To.Q, a.To.R);

        if (fromTile is null)
        {
            return new Result<GameEvent, GameError>.Err(new GameError.TileOutOfBounds(a.From));
        }
        if (toTile is null)
        {
            return new Result<GameEvent, GameError>.Err(new GameError.TileOutOfBounds(a.To));
        }
        if (fromTile.Owner == TileOwner.Unowned)
        {
            return new Result<GameEvent, GameError>.Err(new GameError.ReinforceInvalidFrom(a.PlayerId, a.From));
        }
        if (fromTile.Owner == TileOwner.Player1 && !state.IsPlayer1Turn())
        {
            return new Result<GameEvent, GameError>.Err(new GameError.ReinforceInvalidFrom(a.PlayerId, a.From));
        }
        if (fromTile.Owner == TileOwner.Player2 && !state.IsPlayer2Turn())
        {
            return new Result<GameEvent, GameError>.Err(new GameError.ReinforceInvalidFrom(a.PlayerId, a.From));
        }
        if (toTile.Owner != TileOwner.Unowned && toTile.Owner != fromTile.Owner)
        {
            return new Result<GameEvent, GameError>.Err(new GameError.CannotReinforceToOpponentTile(a.PlayerId, a.To));
        }
        // if (!state.Hexgrid.AreNeighbors(a.From, a.To))
        // {
        //     return new Result<GameEvent, GameError>.Err(new GameError.CannotReinforceNonAdjacentTile(a.PlayerId, a.To));
        // }

        return new Result<GameEvent, GameError>.Success(new GameEvent.PlayerReinforced(a.PlayerId, a.From, a.To, a.Quantity));
    }

    private static Result<GameEvent, GameError> HandleEndReinforcePhase(GameState state, GameAction.EndReinforcePhase a, GameContext context)
    {
        if (state.PlayerIdForCurrentTurn != a.PlayerId)
        {
            return OutOfTurnResult(a.PlayerId);
        }

        return new Result<GameEvent, GameError>.Success(new GameEvent.PlayerEndedReinforcePhase(a.PlayerId));
    }

    private static Result<GameEvent, GameError> HandleResign(GameState state, GameAction.Resign a, GameContext context)
    {
        if (state.PlayerIdForCurrentTurn != a.PlayerId)
        {
            return OutOfTurnResult(a.PlayerId);
        }

        return new Result<GameEvent, GameError>.Success(new GameEvent.PlayerResigned(a.PlayerId));
    }

    private static Result<GameEvent, GameError> OutOfTurnResult(string playerId)
    {
        return new Result<GameEvent, GameError>.Err (new GameError.OutOfTurnError(playerId));
    }

    public static GameState Transition(
        GameState state,
        GameEvent evt)
    {
        return evt switch
        {
            GameEvent.PlayerAttacked e => ApplyPlayerAttacked(state, e),
            GameEvent.PlayerEndedAttackPhase e => ApplyPlayerEndedAttackPhase(state, e),
            GameEvent.PlayerReinforced e => ApplyPlayerReinforced(state, e),
            GameEvent.PlayerEndedReinforcePhase e => ApplyPlayerEndedReinforcePhase(state, e),
            GameEvent.PlayerResigned e => ApplyPlayerResigned(state, e),
            _ => state,
        };
    }

    private static GameState ApplyPlayerAttacked(GameState state, GameEvent.PlayerAttacked e)
    {
        throw new NotImplementedException();
    }

    private static GameState ApplyPlayerEndedAttackPhase(GameState state, GameEvent.PlayerEndedAttackPhase e)
    {
        return state with { TurnPhase = TurnPhase.Reinforcing };
    }

    private static GameState ApplyPlayerReinforced(GameState state, GameEvent.PlayerReinforced e)
    {
        var fromTile = state.Hexgrid.GetTileAt(e.From.Q, e.From.R);
        var toTile = state.Hexgrid.GetTileAt(e.To.Q, e.To.R);

        var newFromQuantity = fromTile.NumUnits - e.Quantity;
        var newToQuantity = toTile.NumUnits + e.Quantity;

        state.Hexgrid.SetTileAt(e.From.Q, e.From.R, fromTile with { NumUnits = newFromQuantity });
        state.Hexgrid.SetTileAt(e.To.Q, e.To.R, toTile with { NumUnits = newToQuantity });

        return state;
    }

    private static GameState ApplyPlayerEndedReinforcePhase(GameState state, GameEvent.PlayerEndedReinforcePhase e)
    {
        var nextPlayerTurn = state.PlayerIdForCurrentTurn == state.Player1Id
            ? state.Player2Id
            : state.Player1Id;

        return state with {
            PlayerIdForCurrentTurn = nextPlayerTurn,
            TurnPhase = TurnPhase.Attacking,
        };
    }

    private static GameState ApplyPlayerResigned(GameState state, GameEvent.PlayerResigned e)
    {
        var winner = e.PlayerId == state.Player1Id
            ? state.Player2Id
            : state.Player1Id;

        return state with {
            Status = new GameStatus.Completed(new GameOutcome.Winner(winner)),
        };
    }
}

public record GameState(
    string Player1Id,
    string Player2Id,
    string PlayerIdForCurrentTurn,
    TurnPhase TurnPhase,
    Hexgrid<Tile> Hexgrid,
    GameStatus Status)
{
    public bool IsPlayer1Turn()
    {
        return PlayerIdForCurrentTurn == Player1Id;
    }

    public bool IsPlayer2Turn()
    {
        return !IsPlayer1Turn();
    }
}

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
    public sealed record ReinforceInvalidFrom(string PlayerId, Coords Coords) : GameError;
    public sealed record ReinforceInvalidTo(string PlayerId) : GameError;
    public sealed record CannotReinforceNonAdjacentTile(string PlayerId, Coords To) : GameError;
    public sealed record CannotReinforceToOpponentTile(string PlayerId, Coords To) : GameError;
    public sealed record TileOutOfBounds(Coords Coords) : GameError;
    public sealed record ActionOutOfPhase(string PlayerId, TurnPhase CurrentPhase) : GameError;
}

public record GameContext();
