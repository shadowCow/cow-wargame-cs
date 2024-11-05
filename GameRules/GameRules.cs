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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
