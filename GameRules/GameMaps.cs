using CowHexgrid;

namespace GameRules;

public static class GameMaps
{
    public static Hexgrid<Tile> MapOne()
    {
        var grid = new Hexgrid<Tile>(6, 6);

        grid.SetTileAt(0, 1, new Tile.Forest());
        grid.SetTileAt(0, 2, new Tile.Forest());
        grid.SetTileAt(0, 3, new Tile.Forest());
        grid.SetTileAt(0, 4, new Tile.Forest());
        grid.SetTileAt(0, 5, new Tile.Forest());

        grid.SetTileAt(1, 1, new Tile.Forest());
        grid.SetTileAt(1, 2, new Tile.Grassland());
        grid.SetTileAt(1, 3, new Tile.Grassland());
        grid.SetTileAt(1, 4, new Tile.Grassland());
        grid.SetTileAt(1, 5, new Tile.Forest());

        grid.SetTileAt(2, 1, new Tile.Water());
        grid.SetTileAt(2, 2, new Tile.Mountain());
        grid.SetTileAt(2, 3, new Tile.Grassland());
        grid.SetTileAt(2, 4, new Tile.Mountain());
        grid.SetTileAt(2, 5, new Tile.Water());

        grid.SetTileAt(3, 1, new Tile.Grassland());
        grid.SetTileAt(3, 2, new Tile.Water());
        grid.SetTileAt(3, 3, new Tile.Water());
        grid.SetTileAt(3, 4, new Tile.Grassland());
        grid.SetTileAt(3, 5, new Tile.Forest());

        grid.SetTileAt(4, 1, new Tile.Mountain());
        grid.SetTileAt(4, 2, new Tile.Mountain());
        grid.SetTileAt(4, 3, new Tile.Water());
        grid.SetTileAt(4, 4, new Tile.Grassland());
        grid.SetTileAt(4, 5, new Tile.Forest());

        grid.SetTileAt(5, 1, new Tile.Forest());
        grid.SetTileAt(5, 2, new Tile.Grassland());
        grid.SetTileAt(5, 3, new Tile.Grassland());
        grid.SetTileAt(5, 4, new Tile.Water());
        grid.SetTileAt(5, 5, new Tile.Forest());

        return grid;
    }
}