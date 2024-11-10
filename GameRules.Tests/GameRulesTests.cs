using CowFst;
using CowHexgrid;

namespace GameRules.Tests;

using GameFst = Fst<GameState, GameAction, GameEvent, GameError, GameContext>;

public class GameRulesTests
{
    [Fact]
    public void TestGameSetup()
    {
        var gameFst = Given.ANewGame();

        Then.Player1Is(gameFst.GetState(), Given.Player1Id);
        Then.Player2Is(gameFst.GetState(), Given.Player2Id);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TurnPhaseIsAttacking(gameFst.GetState());
    }

    [Fact]
    public void TestPlayerCanAttackAdjacentEnemyTile()
    {
        var gameFst = Given.AFullBoardInAttackPhase();

        var attackAction = new GameAction.Attack(Given.Player1Id, new Coords(1, 0), new Coords(2, 0));
        var result = When.PlayerAttacks(gameFst, attackAction);

        Then.ResultIsPlayerAttacked(result, attackAction);
        Then.UnitCountOnTileIs(gameFst.GetState(), attackAction.From, 2);
        Then.UnitCountOnTileIs(gameFst.GetState(), attackAction.To, 2);
    }

    [Fact]
    public void PlayerCannotAttackOwnTile()
    {
        var gameFst = Given.ANewGame();

        var result = When.PlayerAttacks(gameFst, new GameAction.Attack(Given.Player1Id, new Coords(0, 0), new Coords(0, 0)));

        Then.ResultIsCannotAttackOwnTile(result, Given.Player1Id, new Coords(0, 0));
    }

    [Fact]
    public void PlayerCannotAttackUnoccupiedTile()
    {
        var gameFst = Given.ANewGame();

        var result = When.PlayerAttacks(gameFst, new GameAction.Attack(Given.Player1Id, new Coords(0, 0), new Coords(0, 1)));

        Then.ResultIsCannotAttackUnownedTile(result, Given.Player1Id, new Coords(0, 1));
    }

    [Fact]
    public void PlayerCannotAttackNonAdjacentEnemyTile()
    {
        var gameFst = Given.ANewGame();

        var result = When.PlayerAttacks(gameFst, new GameAction.Attack(Given.Player1Id, new Coords(0, 0), new Coords(2, 1)));

        Then.ResultIsCannotAttackNonAdjacentTile(result, Given.Player1Id, new Coords(2, 1));
    }

    [Fact]
    public void PlayerCannotAttackFromUnownedTile()
    {
        var gameFst = Given.ANewGame();

        var result = When.PlayerAttacks(gameFst, new GameAction.Attack(Given.Player1Id, new Coords(1, 1), new Coords(2, 1)));

        Then.ResultIsMustAttackFromTileYouOwn(result, Given.Player1Id, new Coords(1, 1));
    }

    [Fact]
    public void PlayerCannotAttackFromOpponentTile()
    {
        var gameFst = Given.AFullBoardInAttackPhase();

        var result = When.PlayerAttacks(gameFst, new GameAction.Attack(Given.Player1Id, new Coords(2, 0), new Coords(2, 1)));

        Then.ResultIsMustAttackFromTileYouOwn(result, Given.Player1Id, new Coords(2, 0));
    }

    [Fact]
    public void TestEndAttackingPhase()
    {
        var gameFst = Given.ANewGame();

        var result = When.PlayerEndsAttackingPhase(gameFst, Given.Player1Id);

        Then.ResultIsPlayerEndedAttackPhase(result, Given.Player1Id);
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
        var gameFst = Given.AGameDominatedByPlayer1InReinforcePhase();

        var action = new GameAction.Reinforce(Given.Player1Id, new Coords(0, 0), new Coords(2, 0), 1);
        var result = When.PlayerReinforces(gameFst, action);

        Then.ResultIsCannotReinforceNonAdjacentTile(result, Given.Player1Id, action);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TurnPhaseIsReinforcing(gameFst.GetState());
        Then.UnitCountOnTileIs(gameFst.GetState(), action.From, 2);
        Then.UnitCountOnTileIs(gameFst.GetState(), action.To, 2);
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
        var gameFst = Given.ANewGame();

        var result = When.PlayerEndsReinforcingPhase(gameFst, Given.Player1Id);

        Then.ResultIsPlayerEndedReinforcingPhase(result, Given.Player1Id);
        Then.TurnIsPlayer2(gameFst.GetState());
        Then.TurnPhaseIsAttacking(gameFst.GetState());
        Then.GameIsOngoing(gameFst.GetState());
    }

    [Fact]
    public void PlayerCanResign()
    {
        var gameFst = Given.ANewGame();

        var result = When.PlayerResigns(gameFst, Given.Player1Id);

        Then.ResultIsPlayerResigned(result, Given.Player1Id);
        Then.GameIsOverWithWinner(gameFst.GetState(), Given.Player2Id);
    }

    [Fact]
    public void PlayerCannotResignOutOfTurn()
    {
        var gameFst = Given.ANewGame();
        var originalState = gameFst.GetState();

        var result = When.PlayerResigns(gameFst, Given.Player2Id);

        Then.ResultIsOutOfTurnError(result, Given.Player2Id);
        Then.GameStateIsUnchanged(originalState, gameFst.GetState());
    }

    [Fact]
    public void PlayerWinsWhenOpponentIsEliminated()
    {

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

    internal static GameFst ANewGame()
    {
        return GameRules.CreateFst(Player1Id, Player2Id, GameMaps.TinyGrassland());
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

    internal static GameFst AGameDominatedByPlayer1InReinforcePhase()
    {
        var state = new GameState(
            Player1Id,
            Player2Id,
            Player1Id,
            TurnPhase.Reinforcing,
            GameMaps.TinyGrasslandMostlyPlayer1(),
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

    internal static GameFst AFullBoardInAttackPhase()
    {
        var state = new GameState(
            Player1Id,
            Player2Id,
            Player1Id,
            TurnPhase.Attacking,
            GameMaps.TinyGrasslandFullyPopulated(),
            new GameStatus.Ongoing());

        return GameRules.CreateFst(state);
    }
}

internal static class When
{
    internal static object PlayerAttacks(GameFst gameFst, GameAction.Attack attack)
    {
        return gameFst.HandleCommand(attack);
    }

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

    internal static void ResultIsCannotAttackOwnTile(object result, string playerId, Coords to)
    {
        var expectedError = new GameError.CannotAttackOwnTile(playerId, to);
        ExpectError(result, expectedError);
    }

    internal static void ResultIsCannotReinforceNonAdjacentTile(object result, string playerId, GameAction.Reinforce action)
    {
        var expectedError = new GameError.CannotReinforceNonAdjacentTile(playerId, action.To);
        ExpectError(result, expectedError);
    }

    internal static void ResultIsCannotReinforceToOpponentTile(object result, string playerId, GameAction.Reinforce action)
    {
        var expectedError = new GameError.CannotReinforceToOpponentTile(playerId, action.To);
        ExpectError(result, expectedError);
    }

    internal static void ResultIsOutOfTurnError(object result, string playerId)
    {
        var expectedError = new GameError.OutOfTurnError(playerId);
        ExpectError(result, expectedError);
    }

    internal static void ResultIsPlayerEndedAttackPhase(Result<GameEvent, GameError> result, string playerId)
    {
        var expectedEvent = new GameEvent.PlayerEndedAttackPhase(playerId);
        ExpectEvent(result, expectedEvent);
    }

    internal static void ResultIsPlayerEndedReinforcingPhase(object result, string playerId)
    {
        var expectedEvent = new GameEvent.PlayerEndedReinforcePhase(playerId);
        ExpectEvent(result, expectedEvent);
    }

    internal static void ResultIsPlayerReinforced(object result, string playerId, GameAction.Reinforce action)
    {
        var expectedEvent = new GameEvent.PlayerReinforced(playerId, action.From, action.To, action.Quantity);
        ExpectEvent(result, expectedEvent);
    }

    internal static void ResultIsPlayerResigned(object result, string playerId)
    {
        var expectedEvent = new GameEvent.PlayerResigned(playerId);
        ExpectEvent(result, expectedEvent);
    }

    internal static void ResultIsReinforceInvalidFrom(object result, string playerId, GameAction.Reinforce action)
    {
        var expectedError = new GameError.ReinforceInvalidFrom(playerId, action.From);
        ExpectError(result, expectedError);
    }

    internal static void ExpectError(object result, GameError expectedError)
    {
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

    internal static void ExpectEvent(object result, GameEvent expectedEvent)
    {
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

    internal static void ResultIsCannotAttackUnownedTile(object result, string playerId, Coords coords)
    {
        var expectedResult = new GameError.CannotAttackUnownedTile(playerId, coords);
        ExpectError(result, expectedResult);
    }

    internal static void ResultIsCannotAttackNonAdjacentTile(object result, string playerId, Coords coords)
    {
        var expectedResult = new GameError.CannotAttackNonAdjacentTile(playerId, coords);
        ExpectError(result, expectedResult);
    }

    internal static void ResultIsMustAttackFromTileYouOwn(object result, string playerId, Coords coords)
    {
        var expectedResult = new GameError.MustAttackFromTileYouOwn(playerId, coords);
        ExpectError(result, expectedResult);
    }

    internal static void ResultIsPlayerAttacked(object result, GameAction.Attack attackAction)
    {
        var expectedResult = new GameEvent.PlayerAttacked(attackAction.PlayerId, attackAction.From, attackAction.To, 0, 0);
        ExpectEvent(result, expectedResult);
    }
}