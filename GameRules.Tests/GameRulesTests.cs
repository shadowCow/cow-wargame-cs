
using CowFst;

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

    [Fact]
    public void PlayerCanResign()
    {

    }

    [Fact]
    public void PlayerCannotResignOutOfTurn()
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
    public void PlayerCannotReinforceToOpponentTile()
    {

    }

    [Fact]
    public void PlayerCannotReinforceFromOpponentTile()
    {

    }

    [Fact]
    public void PlayerCannotReinforceFromUnownedTile()
    {

    }

    [Fact]
    public void PlayerCannotReinforceWithAllFromUnits()
    {

    }
}

internal static class Given
{
    public const string Player1Id = "p1";
    public const string Player2Id = "p2";

    internal static (GameFst gameFst, string player1Id, string player2Id) ANewGame()
    {
        return (GameRules.CreateFst(Player1Id, Player2Id, GameMaps.MapOne()), Player1Id, Player2Id);
    }
}

internal static class When
{

}

internal static class Then
{
    internal static void GameIsOngoing(GameState gameState)
    {
        Assert.Equal(new GameStatus.Ongoing(), gameState.Status);
    }

    internal static void Player1Is(GameState gameState, string player1Id)
    {
        Assert.Equal(player1Id, gameState.Player1Id);
    }

    internal static void Player2Is(GameState gameState, string player2Id)
    {
        Assert.Equal(player2Id, gameState.Player2Id);
    }

    internal static void TurnIsPlayer1(GameState gameState)
    {
        Assert.Equal(Given.Player1Id, gameState.PlayerIdForCurrentTurn);
    }

    internal static void TurnPhaseIsAttacking(GameState gameState)
    {
        Assert.Equal(TurnPhase.Attacking, gameState.TurnPhase);
    }
}