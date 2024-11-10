namespace GameRules;

public record Tile(
    TileOwner Owner,
    TileTerrain Terrain,
    int NumUnits
)
{
    public static Tile Create(TileTerrain terrain)
    {
        return new Tile(TileOwner.Unowned, terrain, 0);
    }
}

public enum TileOwner
{
    Unowned,
    Player1,
    Player2,
}

public enum TileTerrain
{
    Grassland,
    Forest,
    Mountain,
    Water,
}
