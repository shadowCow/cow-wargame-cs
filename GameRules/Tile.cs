namespace GameRules;

public abstract record Tile
{
    private Tile() {}

    public sealed record Grassland() : Tile;
    public sealed record Water() : Tile;
    public sealed record Forest() : Tile;
    public sealed record Mountain() : Tile;
}