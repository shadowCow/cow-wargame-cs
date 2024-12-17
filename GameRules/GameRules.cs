using CowDice;
using CowFst;
using CowHexgrid;

namespace GameRules;

public static class GameRules
{   
    public static Fst<GameState, GameAction, GameEvent, GameError, GameContext> CreateFst(
        string player1Id,
        string player2Id,
        Hexgrid<Tile> hexgrid,
        GameContext context)
    {
        var initialState = new GameState(
            player1Id,
            player2Id,
            player1Id,
            TurnPhase.Attacking,
            hexgrid,
            new GameStatus.Ongoing());

        return CreateFst(initialState, context);
    }

    public static Fst<GameState, GameAction, GameEvent, GameError, GameContext> CreateFst(
        GameState initialState,
        GameContext context)
    {
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
        if (state.IsGameOver())
        {
            return new Result<GameEvent, GameError>.Err(new GameError.CannotActWhenGameIsOver(a.PlayerId));
        }
        if (state.PlayerIdForCurrentTurn != a.PlayerId)
        {
            return OutOfTurnResult(a.PlayerId);
        }
        if (state.TurnPhase != TurnPhase.Attacking)
        {
            return OutOfPhaseResult(a.PlayerId, state.TurnPhase, TurnPhase.Attacking);
        }

        var fromTile = state.Hexgrid.GetTileAt(a.From);
        var toTile = state.Hexgrid.GetTileAt(a.To);

        if (fromTile is null)
        {
            return new Result<GameEvent, GameError>.Err(new GameError.TileOutOfBounds(a.From));
        }
        if (toTile is null)
        {
            return new Result<GameEvent, GameError>.Err(new GameError.TileOutOfBounds(a.To));
        }
        if (state.IsPlayer1Turn() && fromTile.Owner != TileOwner.Player1)
        {
            return new Result<GameEvent, GameError>.Err(new GameError.MustAttackFromTileYouOwn(a.PlayerId, a.From));
        }
        if (state.IsPlayer2Turn() && fromTile.Owner != TileOwner.Player2)
        {
            return new Result<GameEvent, GameError>.Err(new GameError.MustAttackFromTileYouOwn(a.PlayerId, a.From));
        }
        if (toTile.Owner == TileOwner.Player1 && state.IsPlayer1Turn())
        {
            return new Result<GameEvent, GameError>.Err(new GameError.CannotAttackOwnTile(a.PlayerId, a.To));
        }
        if (toTile.Owner == TileOwner.Player2 && state.IsPlayer2Turn())
        {
            return new Result<GameEvent, GameError>.Err(new GameError.CannotAttackOwnTile(a.PlayerId, a.To));
        }
        if (toTile.Owner == TileOwner.Unowned)
        {
            return new Result<GameEvent, GameError>.Err(new GameError.CannotAttackUnownedTile(a.PlayerId, a.To));
        }

        if (!state.Hexgrid.AreNeighbors(a.From, a.To))
        {
            return new Result<GameEvent, GameError>.Err(new GameError.CannotAttackNonAdjacentTile(a.PlayerId, a.To));
        }

        var (attackerBonus, defenderBonus) = RollingBonuses.Compute(fromTile, toTile);

        var attackerRoll = context.AttackerDiceRoller.RollMdN(1, 6).Sum() + attackerBonus;
        var defenderRoll = context.DefenderDiceRoller.RollMdN(1, 6).Sum() + defenderBonus;

        var (attackerLost, defenderLost) = attackerRoll > defenderRoll
            ? (0, 1)
            : (1, 0);

        return new Result<GameEvent, GameError>.Success(new GameEvent.PlayerAttacked(a.PlayerId, a.From, a.To, attackerRoll, defenderRoll, attackerLost, defenderLost));
    }

    private static Result<GameEvent, GameError> HandleEndAttackPhase(GameState state, GameAction.EndAttackPhase a, GameContext context)
    {
        if (state.IsGameOver())
        {
            return new Result<GameEvent, GameError>.Err(new GameError.CannotActWhenGameIsOver(a.PlayerId));
        }
        if (state.PlayerIdForCurrentTurn != a.PlayerId)
        {
            return OutOfTurnResult(a.PlayerId);
        }
        if (state.TurnPhase != TurnPhase.Attacking)
        {
            return OutOfPhaseResult(a.PlayerId, state.TurnPhase, TurnPhase.Attacking);
        }

        return new Result<GameEvent, GameError>.Success(new GameEvent.PlayerEndedAttackPhase(a.PlayerId));
    }

    private static Result<GameEvent, GameError> HandleReinforce(GameState state, GameAction.Reinforce a, GameContext context)
    {
        if (state.IsGameOver())
        {
            return new Result<GameEvent, GameError>.Err(new GameError.CannotActWhenGameIsOver(a.PlayerId));
        }
        if (state.PlayerIdForCurrentTurn != a.PlayerId)
        {
            return OutOfTurnResult(a.PlayerId);
        }
        if (state.TurnPhase != TurnPhase.Reinforcing)
        {
            return OutOfPhaseResult(a.PlayerId, state.TurnPhase, TurnPhase.Reinforcing);
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
        if (!state.Hexgrid.AreNeighbors(a.From, a.To))
        {
            return new Result<GameEvent, GameError>.Err(new GameError.CannotReinforceNonAdjacentTile(a.PlayerId, a.To));
        }
        if (toTile.Terrain == TileTerrain.Water)
        {
            return new Result<GameEvent, GameError>.Err(new GameError.CannotReinforceTileType(a.PlayerId, a.To, toTile.Terrain));
        }
        if (toTile.NumUnits + a.Quantity > TileUnitLimits.Compute(toTile))
        {
            return new Result<GameEvent, GameError>.Err(new GameError.CannotReinforceTileBeyondCapacity(a.PlayerId, a.To, TileUnitLimits.Compute(toTile)));
        }

        return new Result<GameEvent, GameError>.Success(new GameEvent.PlayerReinforced(a.PlayerId, a.From, a.To, a.Quantity));
    }

    private static Result<GameEvent, GameError> HandleEndReinforcePhase(GameState state, GameAction.EndReinforcePhase a, GameContext context)
    {
        if (state.IsGameOver())
        {
            return new Result<GameEvent, GameError>.Err(new GameError.CannotActWhenGameIsOver(a.PlayerId));
        }
        if (state.PlayerIdForCurrentTurn != a.PlayerId)
        {
            return OutOfTurnResult(a.PlayerId);
        }
        if (state.TurnPhase != TurnPhase.Reinforcing)
        {
            return OutOfPhaseResult(a.PlayerId, state.TurnPhase, TurnPhase.Reinforcing);
        }

        return new Result<GameEvent, GameError>.Success(new GameEvent.PlayerEndedReinforcePhase(a.PlayerId));
    }

    private static Result<GameEvent, GameError> HandleResign(GameState state, GameAction.Resign a, GameContext context)
    {
        if (state.IsGameOver())
        {
            return new Result<GameEvent, GameError>.Err(new GameError.CannotActWhenGameIsOver(a.PlayerId));
        }
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
    
    private static Result<GameEvent, GameError> OutOfPhaseResult(string playerId, TurnPhase currentPhase, TurnPhase requiredPhase)
    {
        return new Result<GameEvent, GameError>.Err(new GameError.ActionOutOfPhase(playerId, currentPhase, requiredPhase));
    }

    public static GameState Transition(
        GameState state,
        GameEvent evt)
    {
        return evt switch
        {
            GameEvent.GameStarted e => ApplyGameStarted(state, e),
            GameEvent.PlayerAttacked e => ApplyPlayerAttacked(state, e),
            GameEvent.PlayerEndedAttackPhase e => ApplyPlayerEndedAttackPhase(state, e),
            GameEvent.PlayerReinforced e => ApplyPlayerReinforced(state, e),
            GameEvent.PlayerEndedReinforcePhase e => ApplyPlayerEndedReinforcePhase(state, e),
            GameEvent.PlayerResigned e => ApplyPlayerResigned(state, e),
            _ => state,
        };
    }

    private static GameState ApplyGameStarted(GameState state, GameEvent.GameStarted e)
    {
        AddStartOfTurnUnits(state, TileOwner.Player1);

        return state;
    }

    private static GameState ApplyPlayerAttacked(GameState state, GameEvent.PlayerAttacked e)
    {
        var fromTile = state.Hexgrid.GetTileAt(e.From);
        var toTile = state.Hexgrid.GetTileAt(e.To);

        var newFromUnits = fromTile.NumUnits - e.FromLost;
        var newToUnits = toTile.NumUnits - e.ToLost;

        var newFromOwner = newFromUnits == 0
            ? TileOwner.Unowned
            : fromTile.Owner;
        var newToOwner = newToUnits == 0
            ? TileOwner.Unowned
            : toTile.Owner;

        state.Hexgrid.SetTileAt(e.From.Q, e.From.R, fromTile with { NumUnits = newFromUnits, Owner = newFromOwner });
        state.Hexgrid.SetTileAt(e.To.Q, e.To.R, toTile with { NumUnits = newToUnits, Owner = newToOwner });

        if (!state.DoesPlayerOwnAnyTiles(TileOwner.Player1))
        {
            return state with {
                Status = new GameStatus.Completed(new GameOutcome.Winner(state.Player2Id)),
            };
        }
        if (!state.DoesPlayerOwnAnyTiles(TileOwner.Player2))
        {
            return state with {
                Status = new GameStatus.Completed(new GameOutcome.Winner(state.Player1Id)),
            };
        }

        return state;
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

        var newFromOwner = newFromQuantity == 0
            ? TileOwner.Unowned
            : fromTile.Owner;

        var newToOwner = newToQuantity > 0
            ? fromTile.Owner
            : TileOwner.Unowned;

        state.Hexgrid.SetTileAt(e.From.Q, e.From.R, fromTile with { Owner = newFromOwner, NumUnits = newFromQuantity });
        state.Hexgrid.SetTileAt(e.To.Q, e.To.R, toTile with { Owner = newToOwner, NumUnits = newToQuantity });

        return state;
    }

    private static GameState ApplyPlayerEndedReinforcePhase(GameState state, GameEvent.PlayerEndedReinforcePhase e)
    {
        var nextPlayerTurn = state.PlayerIdForCurrentTurn == state.Player1Id
            ? state.Player2Id
            : state.Player1Id;

        var tileOwnerForNewArmies = nextPlayerTurn == state.Player1Id
            ? TileOwner.Player1
            : TileOwner.Player2;

        AddStartOfTurnUnits(state, tileOwnerForNewArmies);

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

    static void AddStartOfTurnUnits(GameState state, TileOwner tileOwnerForNewArmies)
    {
        foreach (var coords in state.Hexgrid.EnumerateCoordsByColumn())
        {
            var tile = state.Hexgrid.GetTileAt(coords.Q, coords.R);
            if (tile == null || tile.Owner != tileOwnerForNewArmies)
            {
                continue;
            }

            var newNumUnits = Math.Min(tile.NumUnits + 1, TileUnitLimits.Compute(tile));

            state.Hexgrid.SetTileAt(coords.Q, coords.R, tile with { NumUnits = newNumUnits });
        }
    }
}

public static class RollingBonuses
{
    public static (int attackBonuses, int defenderBonuses) Compute(Tile attackerTile, Tile defenderTile)
    {
        var (aBonus, dBonus) = (attackerTile.Terrain, defenderTile.Terrain) switch
        {
            (TileTerrain.Forest, TileTerrain.Grassland) => (1, 0),
            (TileTerrain.Mountain, TileTerrain.Grassland) => (2, 0),
            (TileTerrain.Mountain, TileTerrain.Forest) => (1, 0),
            (TileTerrain.Grassland, TileTerrain.Forest) => (0, 1),
            (TileTerrain.Grassland, TileTerrain.Mountain) => (0, 2),
            (TileTerrain.Forest, TileTerrain.Mountain) => (0, 1),
            _ => (0, 0),
        };

        if (attackerTile.NumUnits > defenderTile.NumUnits)
        {
            aBonus += 1;
        }
        if (defenderTile.NumUnits > attackerTile.NumUnits)
        {
            dBonus += 1;
        }

        return (aBonus, dBonus);
    }
}

public static class TileUnitLimits
{
    public const int GrasslandCapacity = 10;
    public const int ForestCapacity = 7;
    public const int MountainCapacity = 4;

    public static int Compute(Tile tile)
    {
        return tile.Terrain switch
        {
            TileTerrain.Grassland => 10,
            TileTerrain.Forest => 7,
            TileTerrain.Mountain => 4,
            _ => 0,
        };
    }

    public static int MaximumReinforcementQuantity(Tile? from, Tile? to)
    {
        if (from is null || to is null)
        {
            return 0;
        }

        var toCapacity = Compute(to) - to.NumUnits;

        return Math.Min(from.NumUnits, toCapacity);
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

    public bool DoesPlayerOwnAnyTiles(TileOwner owner)
    {
        return Hexgrid.EnumerateCoordsByColumn()
            .Select(Hexgrid.GetTileAt)
            .Where(t => t is not null && t.Owner == owner)
            .Any();
    }

    public bool IsGameOver()
    {
        return Status switch
        {
            GameStatus.Completed => true,
            _ => false,
        };
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

    public sealed record GameStarted() : GameEvent;
    public sealed record PlayerResigned(string PlayerId) : GameEvent;
    public sealed record PlayerAttacked(string PlayerId, Coords From, Coords To, int AttackerRoll, int DefenderRoll, int FromLost, int ToLost) : GameEvent;
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
    public sealed record MustAttackFromTileYouOwn(string PlayerId, Coords From) : GameError;
    public sealed record CannotAttackOwnTile(string PlayerId, Coords To) : GameError;
    public sealed record CannotAttackUnownedTile(string PlayerId, Coords To) : GameError;
    public sealed record CannotAttackNonAdjacentTile(string PlayerId, Coords To) : GameError;
    public sealed record ReinforceFromInsufficientQuantity(string PlayerId) : GameError;
    public sealed record ReinforceInvalidFrom(string PlayerId, Coords Coords) : GameError;
    public sealed record ReinforceInvalidTo(string PlayerId) : GameError;
    public sealed record CannotReinforceNonAdjacentTile(string PlayerId, Coords To) : GameError;
    public sealed record CannotReinforceToOpponentTile(string PlayerId, Coords To) : GameError;
    public sealed record CannotReinforceTileType(string PlayerId, Coords Coords, TileTerrain Terrain) : GameError;
    public sealed record CannotReinforceTileBeyondCapacity(string PlayerId, Coords Coords, int Capacity) : GameError;
    public sealed record CannotActWhenGameIsOver(string PlayerId) : GameError;
    public sealed record TileOutOfBounds(Coords Coords) : GameError;
    public sealed record ActionOutOfPhase(string PlayerId, TurnPhase CurrentPhase, TurnPhase PhaseNeededForAction) : GameError;
}

public record GameContext(IDiceRoller AttackerDiceRoller, IDiceRoller DefenderDiceRoller);
