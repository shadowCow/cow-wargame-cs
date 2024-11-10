using CowFst;
using CowHexgrid;

namespace GameRules.Tests;

using GameFst = Fst<GameState, GameAction, GameEvent, GameError, GameContext>;

public class GameRulesTests
{
    [Fact]
    public void TestGameSetup()
    {
        var (gameFst, player1Id, player2Id) = Given.ANewGame();

        Then.Player1Is(gameFst.GetState(), player1Id);
        Then.Player2Is(gameFst.GetState(), player2Id);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TurnPhaseIsAttacking(gameFst.GetState());
    }

    [Fact]
    public void TestPlayerCanAttackAdjacentEnemyTile()
    {

    }

    [Fact]
    public void PlayerCannotAttackOwnTile()
    {

    }

    [Fact]
    public void PlayerCannotAttackUnoccupiedTile()
    {

    }

    [Fact]
    public void PlayerCannotAttackNonAdjacentEnemyTile()
    {

    }

    [Fact]
    public void TestEndAttackingPhase()
    {
        var (gameFst, player1Id, player2Id) = Given.ANewGame();

        var result = When.PlayerEndsAttackingPhase(gameFst, player1Id);

        Then.ResultIsPlayerEndedAttackPhase(result, player1Id);
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TurnPhaseIsReinforcing(gameFst.GetState());
        Then.GameIsOngoing(gameFst.GetState());
    }

    [Fact]
    public void TestPlayerCanReinforceUnoccupiedTile()
    {
        var gameFst = Given.ANewGameInReinforcePhase();

        var action = new GameAction.Reinforce(Given.Player1Id, new Coords(0, 0), new Coords(0, 1), 1);
        var result = When.PlayerReinforcesUnoccupiedTile(gameFst, action);

        Then.ResultIsPlayerReinforced(result, Given.Player1Id, action);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TurnPhaseIsReinforcing(gameFst.GetState());
        Then.UnitCountOnTileIs(gameFst.GetState(), action.From, 0);
        Then.UnitCountOnTileIs(gameFst.GetState(), action.To, 1);
    }

    [Fact]
    public void TestPlayerCanReinforceOwnTile()
    {
        var gameFst = Given.AFullBoardInReinforcePhase();

        var action = new GameAction.Reinforce(Given.Player1Id, new Coords(0, 0), new Coords(1, 0), 1);
        var result = When.PlayerReinforces(gameFst, action);

        Then.ResultIsPlayerReinforced(result, Given.Player1Id, action);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TurnPhaseIsReinforcing(gameFst.GetState());
        Then.UnitCountOnTileIs(gameFst.GetState(), action.From, 1);
        Then.UnitCountOnTileIs(gameFst.GetState(), action.To, 3);
    }

    [Fact]
    public void PlayerCanReinforceWithAllFromUnits()
    {
        var gameFst = Given.AFullBoardInReinforcePhase();

        var action = new GameAction.Reinforce(Given.Player1Id, new Coords(0, 0), new Coords(1, 0), 2);
        var result = When.PlayerReinforces(gameFst, action);

        Then.ResultIsPlayerReinforced(result, Given.Player1Id, action);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TurnPhaseIsReinforcing(gameFst.GetState());
        Then.UnitCountOnTileIs(gameFst.GetState(), action.From, 0);
        Then.UnitCountOnTileIs(gameFst.GetState(), action.To, 4);
    }

    [Fact]
    public void PlayerCannotReinforceNonAdjacentTile()
    {
        // var gameFst = Given.ANewGameInReinforcePhase();

        // var action = new GameAction.Reinforce(Given.Player1Id, new Coords(0, 0), new Coords(2, 0), 1);
        // var result = When.PlayerReinforcesUnoccupiedTile(gameFst, action);

        // Then.ResultIsCannotReinforceNonAdjacentTile(result, Given.Player1Id, action);
        // Then.GameIsOngoing(gameFst.GetState());
        // Then.TurnIsPlayer1(gameFst.GetState());
        // Then.TurnPhaseIsReinforcing(gameFst.GetState());
        // Then.UnitCountOnTileIs(gameFst.GetState(), action.From, 1);
        // Then.UnitCountOnTileIs(gameFst.GetState(), action.To, 0);
    }
    
    [Fact]
    public void PlayerCannotReinforceToOpponentTile()
    {
        var gameFst = Given.AFullBoardInReinforcePhase();

        var action = new GameAction.Reinforce(Given.Player1Id, new Coords(1, 0), new Coords(2, 0), 1);
        var result = When.PlayerReinforces(gameFst, action);

        Then.ResultIsCannotReinforceToOpponentTile(result, Given.Player1Id, action);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TurnPhaseIsReinforcing(gameFst.GetState());
        Then.UnitCountOnTileIs(gameFst.GetState(), action.From, 2);
        Then.UnitCountOnTileIs(gameFst.GetState(), action.To, 2);
    }

    [Fact]
    public void PlayerCannotReinforceFromOpponentTile()
    {
        var gameFst = Given.ANewGameInReinforcePhase();

        var action = new GameAction.Reinforce(Given.Player1Id, new Coords(2, 1), new Coords(2, 0), 1);
        var result = When.PlayerReinforces(gameFst, action);

        Then.ResultIsReinforceInvalidFrom(result, Given.Player1Id, action);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TurnPhaseIsReinforcing(gameFst.GetState());
        Then.UnitCountOnTileIs(gameFst.GetState(), action.From, 1);
        Then.UnitCountOnTileIs(gameFst.GetState(), action.To, 0);
    }

    [Fact]
    public void PlayerCannotReinforceFromUnownedTile()
    {
        var gameFst = Given.ANewGameInReinforcePhase();

        var action = new GameAction.Reinforce(Given.Player1Id, new Coords(0, 1), new Coords(1, 1), 1);
        var result = When.PlayerReinforces(gameFst, action);

        Then.ResultIsReinforceInvalidFrom(result, Given.Player1Id, action);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TurnPhaseIsReinforcing(gameFst.GetState());
        Then.UnitCountOnTileIs(gameFst.GetState(), action.From, 0);
        Then.UnitCountOnTileIs(gameFst.GetState(), action.To, 0);
    }

    [Fact]
    public void TestEndReinforcingPhase()
    {
        var (gameFst, player1Id, player2Id) = Given.ANewGame();

        var result = When.PlayerEndsReinforcingPhase(gameFst, player1Id);

        Then.ResultIsPlayerEndedReinforcingPhase(result, player1Id);
        Then.TurnIsPlayer2(gameFst.GetState());
        Then.TurnPhaseIsAttacking(gameFst.GetState());
        Then.GameIsOngoing(gameFst.GetState());
    }

    [Fact]
    public void PlayerCanResign()
    {
        var (gameFst, player1Id, player2Id) = Given.ANewGame();

        var result = When.PlayerResigns(gameFst, player1Id);

        Then.ResultIsPlayerResigned(result, player1Id);
        Then.GameIsOverWithWinner(gameFst.GetState(), player2Id);
    }

    [Fact]
    public void PlayerCannotResignOutOfTurn()
    {
        var (gameFst, player1Id, player2Id) = Given.ANewGame();
        var originalState = gameFst.GetState();

        var result = When.PlayerResigns(gameFst, player2Id);

        Then.ResultIsOutOfTurnError(result, player2Id);
        Then.GameStateIsUnchanged(originalState, gameFst.GetState());
    }

    [Fact]
    public void TestPlayerOneTurn()
    {
        
    }

    [Fact]
    public void TestPlayerTwoTurn()
    {

    }

    [Fact]
    public void TestWholeGame()
    {

    }
}

internal static class Given
{
    public const string Player1Id = "p1";
    public const string Player2Id = "p2";

    internal static (GameFst gameFst, string player1Id, string player2Id) ANewGame()
    {
        return (GameRules.CreateFst(Player1Id, Player2Id, GameMaps.TinyGrassland()), Player1Id, Player2Id);
    }

    internal static GameFst ANewGameInReinforcePhase()
    {
        var state = new GameState(
            Player1Id,
            Player2Id,
            Player1Id,
            TurnPhase.Reinforcing,
            GameMaps.TinyGrassland(),
            new GameStatus.Ongoing());

        return GameRules.CreateFst(state);
    }

    internal static GameFst AFullBoardInReinforcePhase()
    {
        var state = new GameState(
            Player1Id,
            Player2Id,
            Player1Id,
            TurnPhase.Reinforcing,
            GameMaps.TinyGrasslandFullyPopulated(),
            new GameStatus.Ongoing());

        return GameRules.CreateFst(state);
    }
}

internal static class When
{
    internal static Result<GameEvent, GameError> PlayerEndsAttackingPhase(GameFst gameFst, string playerId)
    {
        return gameFst.HandleCommand(new GameAction.EndAttackPhase(playerId));
    }

    internal static object PlayerEndsReinforcingPhase(GameFst gameFst, string playerId)
    {
        return gameFst.HandleCommand(new GameAction.EndReinforcePhase(playerId));
    }

    internal static object PlayerReinforces(GameFst gameFst, GameAction.Reinforce action)
    {
        return gameFst.HandleCommand(action);
    }

    internal static object PlayerReinforcesUnoccupiedTile(GameFst gameFst, GameAction.Reinforce action)
    {
        var toTile = gameFst.GetState().Hexgrid.GetTileAt(action.To.Q, action.To.R);
        Assert.Equal(TileOwner.Unowned, toTile?.Owner);

        return gameFst.HandleCommand(action);
    }

    internal static object PlayerResigns(GameFst gameFst, string playerId)
    {
        return gameFst.HandleCommand(new GameAction.Resign(playerId));
    }
}

internal static class Then
{
    internal static void GameIsOngoing(GameState gameState)
    {
        Assert.Equal(new GameStatus.Ongoing(), gameState.Status);
    }

    internal static void GameIsOverWithWinner(GameState gameState, string playerId)
    {
        Assert.Equal(new GameStatus.Completed(new GameOutcome.Winner(playerId)), gameState.Status);
    }

    internal static void GameStateIsUnchanged(GameState originalState, GameState gameState)
    {
        Assert.Equal(originalState, gameState);
    }

    internal static void Player1Is(GameState gameState, string player1Id)
    {
        Assert.Equal(player1Id, gameState.Player1Id);
    }

    internal static void Player2Is(GameState gameState, string player2Id)
    {
        Assert.Equal(player2Id, gameState.Player2Id);
    }

    internal static void ResultIsCannotReinforceNonAdjacentTile(object result, string playerId, GameAction.Reinforce action)
    {
        var expectedError = new GameError.CannotReinforceNonAdjacentTile(playerId, action.To);
        switch (result)
        {
            case Result<GameEvent, GameError>.Err e:
                Assert.Equal(expectedError, e.Error);
                break;
            default:
                Assert.Fail($"expected Result.Err, but was Result.Success: ${result}");
                break;
        }
    }

    internal static void ResultIsCannotReinforceToOpponentTile(object result, string playerId, GameAction.Reinforce action)
    {
        var expectedError = new GameError.CannotReinforceToOpponentTile(playerId, action.To);
        switch (result)
        {
            case Result<GameEvent, GameError>.Err e:
                Assert.Equal(expectedError, e.Error);
                break;
            default:
                Assert.Fail($"expected Result.Err, but was Result.Success: ${result}");
                break;
        }
    }

    internal static void ResultIsOutOfTurnError(object result, string playerId)
    {
        var expectedError = new GameError.OutOfTurnError(playerId);
        switch (result)
        {
            case Result<GameEvent, GameError>.Err e:
                Assert.Equal(expectedError, e.Error);
                break;
            default:
                Assert.Fail($"expected Result.Err, but was Result.Success: ${result}");
                break;
        }
    }

    internal static void ResultIsPlayerEndedAttackPhase(Result<GameEvent, GameError> result, string playerId)
    {
        var expectedEvent = new GameEvent.PlayerEndedAttackPhase(playerId);
        switch (result)
        {
            case Result<GameEvent, GameError>.Success s:
                Assert.Equal(expectedEvent, s.Value);
                break;
            default:
                Assert.Fail($"expected Result.Success, but was Result.Err: ${result}");
                break;
        }
    }

    internal static void ResultIsPlayerEndedReinforcingPhase(object result, string playerId)
    {
        var expectedEvent = new GameEvent.PlayerEndedReinforcePhase(playerId);
        switch (result)
        {
            case Result<GameEvent, GameError>.Success s:
                Assert.Equal(expectedEvent, s.Value);
                break;
            default:
                Assert.Fail($"expected Result.Success, but was Result.Err: ${result}");
                break;
        }
    }

    internal static void ResultIsPlayerReinforced(object result, string playerId, GameAction.Reinforce action)
    {
        var expectedEvent = new GameEvent.PlayerReinforced(playerId, action.From, action.To, action.Quantity);
        switch (result)
        {
            case Result<GameEvent, GameError>.Success s:
                Assert.Equal(expectedEvent, s.Value);
                break;
            default:
                Assert.Fail($"expected Result.Success, but was Result.Err: ${result}");
                break;
        }
    }

    internal static void ResultIsPlayerResigned(object result, string playerId)
    {
        var expectedEvent = new GameEvent.PlayerResigned(playerId);
        switch (result)
        {
            case Result<GameEvent, GameError>.Success s:
                Assert.Equal(expectedEvent, s.Value);
                break;
            default:
                Assert.Fail($"expected Result.Success, but was Result.Err: ${result}");
                break;
        }
    }

    internal static void ResultIsReinforceInvalidFrom(object result, string playerId, GameAction.Reinforce action)
    {
        var expectedError = new GameError.ReinforceInvalidFrom(playerId, action.From);
        switch (result)
        {
            case Result<GameEvent, GameError>.Err e:
                Assert.Equal(expectedError, e.Error);
                break;
            default:
                Assert.Fail($"expected Result.Err, but was Result.Success: ${result}");
                break;
        }
    }

    internal static void TurnIsPlayer1(GameState gameState)
    {
        Assert.Equal(Given.Player1Id, gameState.PlayerIdForCurrentTurn);
    }

    internal static void TurnIsPlayer2(GameState gameState)
    {
        Assert.Equal(Given.Player2Id, gameState.PlayerIdForCurrentTurn);
    }

    internal static void TurnPhaseIsAttacking(GameState gameState)
    {
        Assert.Equal(TurnPhase.Attacking, gameState.TurnPhase);
    }

    internal static void TurnPhaseIsReinforcing(GameState gameState)
    {
        Assert.Equal(TurnPhase.Reinforcing, gameState.TurnPhase);
    }

    internal static void UnitCountOnTileIs(GameState gameState, Coords from, int v)
    {
        var tile = gameState.Hexgrid.GetTileAt(from.Q, from.R);

        Assert.Equal(v, tile?.NumUnits);
    }
}