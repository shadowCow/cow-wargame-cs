using CowDice;
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
    public void PlayerAttacksAdjacentEnemyTileAndLoses()
    {
        var gameFst = Given.AFullBoardInAttackPhase(Given.AMinDiceRoller(), Given.AMaxDiceRoller());

        var attackAction = new GameAction.Attack(Given.Player1Id, new Coords(1, 0), new Coords(2, 0));
        var result = When.PlayerAttacks(gameFst, attackAction);

        var expectedEvent = new GameEvent.PlayerAttacked(Given.Player1Id, attackAction.From, attackAction.To, 1, 6, 1, 0);
        Then.ResultIsPlayerAttacked(result, expectedEvent);
        Then.UnitCountOnTileIs(gameFst.GetState(), attackAction.From, 1);
        Then.UnitCountOnTileIs(gameFst.GetState(), attackAction.To, 2);
        Then.TileOwnerIs(gameFst.GetState(), attackAction.From, TileOwner.Player1);
        Then.TileOwnerIs(gameFst.GetState(), attackAction.To, TileOwner.Player2);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnPhaseIsAttacking(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
    }

    [Fact]
    public void PlayerAttacksAdjacentEnemyTileAndWins()
    {
        var gameFst = Given.AFullBoardInAttackPhase(Given.AMaxDiceRoller(), Given.AMinDiceRoller());

        var attackAction = new GameAction.Attack(Given.Player1Id, new Coords(1, 0), new Coords(2, 0));
        var result = When.PlayerAttacks(gameFst, attackAction);

        var expectedEvent = new GameEvent.PlayerAttacked(Given.Player1Id, attackAction.From, attackAction.To, 6, 1, 0, 1);
        Then.ResultIsPlayerAttacked(result, expectedEvent);
        Then.UnitCountOnTileIs(gameFst.GetState(), attackAction.From, 2);
        Then.UnitCountOnTileIs(gameFst.GetState(), attackAction.To, 1);
        Then.TileOwnerIs(gameFst.GetState(), attackAction.From, TileOwner.Player1);
        Then.TileOwnerIs(gameFst.GetState(), attackAction.To, TileOwner.Player2);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnPhaseIsAttacking(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
    }

    [Fact]
    public void PlayerAttacksAndRemovesEnemyFromTile()
    {
        var gameFst = Given.AFullBoardInAttackPhase(Given.AMaxDiceRoller(), Given.AMinDiceRoller(), 1);

        var attackAction = new GameAction.Attack(Given.Player1Id, new Coords(1, 0), new Coords(2, 0));
        var result = When.PlayerAttacks(gameFst, attackAction);

        var expectedEvent = new GameEvent.PlayerAttacked(Given.Player1Id, attackAction.From, attackAction.To, 6, 1, 0, 1);
        Then.ResultIsPlayerAttacked(result, expectedEvent);
        Then.UnitCountOnTileIs(gameFst.GetState(), attackAction.From, 1);
        Then.UnitCountOnTileIs(gameFst.GetState(), attackAction.To, 0);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnPhaseIsAttacking(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TileOwnerIs(gameFst.GetState(), attackAction.From, TileOwner.Player1);
        Then.TileOwnerIs(gameFst.GetState(), attackAction.To, TileOwner.Unowned);
    }

    [Fact]
    public void PlayerAttacksAndLosesTile()
    {
        var gameFst = Given.AFullBoardInAttackPhase(Given.AMinDiceRoller(), Given.AMaxDiceRoller(), 1);

        var attackAction = new GameAction.Attack(Given.Player1Id, new Coords(1, 0), new Coords(2, 0));
        var result = When.PlayerAttacks(gameFst, attackAction);

        var expectedEvent = new GameEvent.PlayerAttacked(Given.Player1Id, attackAction.From, attackAction.To, 1, 6, 1, 0);
        Then.ResultIsPlayerAttacked(result, expectedEvent);
        Then.UnitCountOnTileIs(gameFst.GetState(), attackAction.From, 0);
        Then.UnitCountOnTileIs(gameFst.GetState(), attackAction.To, 1);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnPhaseIsAttacking(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TileOwnerIs(gameFst.GetState(), attackAction.From, TileOwner.Unowned);
        Then.TileOwnerIs(gameFst.GetState(), attackAction.To, TileOwner.Player2);
    }

    [Theory]
    // same terrain does not add bonuses
    [InlineData(TileTerrain.Grassland, TileTerrain.Grassland, 1, 1, 6, 1)]
    [InlineData(TileTerrain.Forest, TileTerrain.Forest, 1, 1, 6, 1)]
    [InlineData(TileTerrain.Mountain, TileTerrain.Mountain, 1, 1, 6, 1)]
    // more units helps roll on same terrain
    [InlineData(TileTerrain.Grassland, TileTerrain.Grassland, 2, 1, 7, 1)]
    [InlineData(TileTerrain.Forest, TileTerrain.Forest, 2, 1, 7, 1)]
    [InlineData(TileTerrain.Mountain, TileTerrain.Mountain, 2, 1, 7, 1)]
    [InlineData(TileTerrain.Grassland, TileTerrain.Grassland, 1, 2, 6, 2)]
    [InlineData(TileTerrain.Forest, TileTerrain.Forest, 1, 2, 6, 2)]
    [InlineData(TileTerrain.Mountain, TileTerrain.Mountain, 1, 2, 6, 2)]
    // different terrain adds bonuses
    [InlineData(TileTerrain.Forest, TileTerrain.Grassland, 1, 1, 7, 1)]
    [InlineData(TileTerrain.Mountain, TileTerrain.Forest, 1, 1, 7, 1)]
    [InlineData(TileTerrain.Mountain, TileTerrain.Grassland, 1, 1, 8, 1)]
    [InlineData(TileTerrain.Grassland, TileTerrain.Forest, 1, 1, 6, 2)]
    [InlineData(TileTerrain.Forest, TileTerrain.Mountain, 1, 1, 6, 2)]
    [InlineData(TileTerrain.Grassland, TileTerrain.Mountain, 1, 1, 6, 3)]
    // more units helps roll on different terrain
    [InlineData(TileTerrain.Forest, TileTerrain.Grassland, 2, 1, 8, 1)]
    [InlineData(TileTerrain.Mountain, TileTerrain.Forest, 2, 1, 8, 1)]
    [InlineData(TileTerrain.Mountain, TileTerrain.Grassland, 2, 1, 9, 1)]
    [InlineData(TileTerrain.Grassland, TileTerrain.Forest, 1, 2, 6, 3)]
    [InlineData(TileTerrain.Forest, TileTerrain.Mountain, 1, 2, 6, 3)]
    [InlineData(TileTerrain.Grassland, TileTerrain.Mountain, 1, 2, 6, 4)]
    public void AttackAndDefenseBonusesAttackerWins(
        TileTerrain attackerTerrain,
        TileTerrain defenderTerrain,
        int attackerUnits,
        int defenderUnits,
        int expectedAttackerRoll,
        int expectedDefenderRoll)
    {
        var gameFst = Given.TheTiniestBoardInAttackPhase(attackerTerrain, defenderTerrain, attackerUnits, defenderUnits, Given.AMaxDiceRoller(), Given.AMinDiceRoller());

        var attackAction = new GameAction.Attack(Given.Player1Id, new Coords(0, 0), new Coords(1, 0));
        var result = When.PlayerAttacks(gameFst, attackAction);

        var expectedEvent = new GameEvent.PlayerAttacked(Given.Player1Id, attackAction.From, attackAction.To, expectedAttackerRoll, expectedDefenderRoll, 0, 1);
        var expectedDefenderUnits = defenderUnits - 1;
        var expectedDefendedTileOwner = expectedDefenderUnits > 0
            ? TileOwner.Player2
            : TileOwner.Unowned;
        Then.ResultIsPlayerAttacked(result, expectedEvent);
        Then.UnitCountOnTileIs(gameFst.GetState(), attackAction.From, attackerUnits);
        Then.UnitCountOnTileIs(gameFst.GetState(), attackAction.To, defenderUnits - 1);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnPhaseIsAttacking(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TileOwnerIs(gameFst.GetState(), attackAction.From, TileOwner.Player1);
        Then.TileOwnerIs(gameFst.GetState(), attackAction.To, expectedDefendedTileOwner);
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
        var gameFst = Given.AFullBoardInAttackPhase(Given.AMaxDiceRoller(), Given.AMinDiceRoller());

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
    public void PlayerCannotReinforceWaterTile()
    {
        var gameFst = Given.ANewGameInReinforcePhaseWithWater();

        var action = new GameAction.Reinforce(Given.Player1Id, new Coords(0, 0), new Coords(0, 1), 1);
        var result = When.PlayerReinforces(gameFst, action);

        Then.ResultIsCannotReinforceTileType(result, Given.Player1Id, action.To, TileTerrain.Water);
        Then.GameIsOngoing(gameFst.GetState());
        Then.TurnIsPlayer1(gameFst.GetState());
        Then.TurnPhaseIsReinforcing(gameFst.GetState());
        Then.UnitCountOnTileIs(gameFst.GetState(), action.From, 1);
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

    [Fact(Skip = "Not implemented yet")]
    public void PlayerWinsWhenOpponentIsEliminated()
    {

    }

    [Fact(Skip = "Not implemented yet")]
    public void TestPlayerOneTurn()
    {
        
    }

    [Fact(Skip = "Not implemented yet")]
    public void TestPlayerTwoTurn()
    {

    }

    [Fact(Skip = "Not implemented yet")]
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
        return GameRules.CreateFst(
            Player1Id,
            Player2Id,
            GameMaps.TinyGrassland(),
            new GameContext(AMaxDiceRoller(), AMinDiceRoller()));
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

        return GameRules.CreateFst(state, new GameContext(AMaxDiceRoller(), AMinDiceRoller()));
    }

    internal static GameFst ANewGameInReinforcePhaseWithWater()
    {
        var state = new GameState(
            Player1Id,
            Player2Id,
            Player1Id,
            TurnPhase.Reinforcing,
            GameMaps.TinyGrasslandWithLakes(),
            new GameStatus.Ongoing());

        return GameRules.CreateFst(state, new GameContext(AMaxDiceRoller(), AMinDiceRoller()));
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

        return GameRules.CreateFst(state, new GameContext(AMaxDiceRoller(), AMinDiceRoller()));
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

        return GameRules.CreateFst(state, new GameContext(AMaxDiceRoller(), AMinDiceRoller()));
    }

    internal static GameFst AFullBoardInAttackPhase(
        IDiceRoller attackerDiceRoller,
        IDiceRoller defenderDiceRoller,
        int unitsPerTile = 2)
    {
        var state = new GameState(
            Player1Id,
            Player2Id,
            Player1Id,
            TurnPhase.Attacking,
            GameMaps.TinyGrasslandFullyPopulated(unitsPerTile),
            new GameStatus.Ongoing());

        return GameRules.CreateFst(state, new GameContext(attackerDiceRoller, defenderDiceRoller));
    }

    internal static GameFst TheTiniestBoardInAttackPhase(TileTerrain attackerTerrain, TileTerrain defenderTerrain, int attackerUnits, int defenderUnits, IDiceRoller attackerDiceRoller, IDiceRoller defenderDiceRoller)
    {
        var state = new GameState(
            Player1Id,
            Player2Id,
            Player1Id,
            TurnPhase.Attacking,
            GameMaps.TiniestMap(attackerTerrain, attackerUnits, defenderTerrain, defenderUnits),
            new GameStatus.Ongoing());

        return GameRules.CreateFst(state, new GameContext(attackerDiceRoller, defenderDiceRoller));
    }

    internal static IDiceRoller AMaxDiceRoller()
    {
        return new MaxDiceRoller();
    }

    internal static IDiceRoller AMinDiceRoller()
    {
        return new MinDiceRoller();
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

    internal static void TileOwnerIs(GameState gameState, Coords from, TileOwner expectedOwner)
    {
        var tileOwner = gameState.Hexgrid.GetTileAt(from)?.Owner;
        Assert.Equal(expectedOwner, tileOwner);
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

    internal static void ResultIsPlayerAttacked(object result, GameEvent.PlayerAttacked expectedResult)
    {
        ExpectEvent(result, expectedResult);
    }

    internal static void ResultIsCannotReinforceTileType(object result, string player1Id, Coords coords, TileTerrain terrain)
    {
        var expectedError = new GameError.CannotReinforceTileType(player1Id, coords, terrain);
        ExpectError(result, expectedError);
    }
}