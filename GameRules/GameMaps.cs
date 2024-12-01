using CowHexgrid;

namespace GameRules;

public static class GameMaps
{
    public static Hexgrid<Tile> MapOne()
    {
        var grid = new Hexgrid<Tile>(6, 6);

        grid.SetTileAt(0, 0, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(0, 1, Tile.Create(TileTerrain.Forest));
        grid.SetTileAt(0, 2, Tile.Create(TileTerrain.Forest));
        grid.SetTileAt(0, 3, Tile.Create(TileTerrain.Forest));
        grid.SetTileAt(0, 4, Tile.Create(TileTerrain.Forest));
        grid.SetTileAt(0, 5, Tile.Create(TileTerrain.Forest));

        grid.SetTileAt(1, 0, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(1, 1, Tile.Create(TileTerrain.Forest));
        grid.SetTileAt(1, 2, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(1, 3, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(1, 4, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(1, 5, Tile.Create(TileTerrain.Forest));

        grid.SetTileAt(2, 0, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(2, 1, Tile.Create(TileTerrain.Water));
        grid.SetTileAt(2, 2, Tile.Create(TileTerrain.Mountain));
        grid.SetTileAt(2, 3, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(2, 4, Tile.Create(TileTerrain.Mountain));
        grid.SetTileAt(2, 5, Tile.Create(TileTerrain.Water));

        grid.SetTileAt(3, 0, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(3, 1, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(3, 2, Tile.Create(TileTerrain.Water));
        grid.SetTileAt(3, 3, Tile.Create(TileTerrain.Water));
        grid.SetTileAt(3, 4, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(3, 5, Tile.Create(TileTerrain.Forest));

        grid.SetTileAt(4, 0, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(4, 1, Tile.Create(TileTerrain.Mountain));
        grid.SetTileAt(4, 2, Tile.Create(TileTerrain.Mountain));
        grid.SetTileAt(4, 3, Tile.Create(TileTerrain.Water));
        grid.SetTileAt(4, 4, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(4, 5, Tile.Create(TileTerrain.Forest));

        grid.SetTileAt(5, 0, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(5, 1, Tile.Create(TileTerrain.Forest));
        grid.SetTileAt(5, 2, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(5, 3, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(5, 4, Tile.Create(TileTerrain.Water));
        grid.SetTileAt(5, 5, Tile.Create(TileTerrain.Forest));

        return grid;
    }

    public static Hexgrid<Tile> TinyGrassland()
    {
        var grid = new Hexgrid<Tile>(3, 2);

        grid.SetTileAt(0, 0, new Tile(TileOwner.Player1, TileTerrain.Grassland, 1));
        grid.SetTileAt(0, 1, Tile.Create(TileTerrain.Grassland));

        grid.SetTileAt(1, 0, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(1, 1, Tile.Create(TileTerrain.Grassland));

        grid.SetTileAt(2, 0, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(2, 1, new Tile(TileOwner.Player2, TileTerrain.Grassland, 1));

        return grid;
    }

    public static Hexgrid<Tile> TinyGrasslandFullyPopulated(int unitsPerTile = 2)
    {
        var grid = new Hexgrid<Tile>(3, 2);

        grid.SetTileAt(0, 0, new Tile(TileOwner.Player1, TileTerrain.Grassland, unitsPerTile));
        grid.SetTileAt(0, 1, new Tile(TileOwner.Player1, TileTerrain.Grassland, unitsPerTile));

        grid.SetTileAt(1, 0, new Tile(TileOwner.Player1, TileTerrain.Grassland, unitsPerTile));
        grid.SetTileAt(1, 1, new Tile(TileOwner.Player2, TileTerrain.Grassland, unitsPerTile));

        grid.SetTileAt(2, 0, new Tile(TileOwner.Player2, TileTerrain.Grassland, unitsPerTile));
        grid.SetTileAt(2, 1, new Tile(TileOwner.Player2, TileTerrain.Grassland, unitsPerTile));

        return grid;
    }

    public static Hexgrid<Tile> TinyGrasslandMostlyPlayer1()
    {
        var grid = new Hexgrid<Tile>(3, 2);

        grid.SetTileAt(0, 0, new Tile(TileOwner.Player1, TileTerrain.Grassland, 2));
        grid.SetTileAt(0, 1, new Tile(TileOwner.Player1, TileTerrain.Grassland, 2));

        grid.SetTileAt(1, 0, new Tile(TileOwner.Player1, TileTerrain.Grassland, 2));
        grid.SetTileAt(1, 1, new Tile(TileOwner.Player1, TileTerrain.Grassland, 2));

        grid.SetTileAt(2, 0, new Tile(TileOwner.Player1, TileTerrain.Grassland, 2));
        grid.SetTileAt(2, 1, new Tile(TileOwner.Player2, TileTerrain.Grassland, 2));

        return grid;
    }

    public static Hexgrid<Tile> TinyGrasslandWithLakes()
    {
        var grid = new Hexgrid<Tile>(3, 2);

        grid.SetTileAt(0, 0, new Tile(TileOwner.Player1, TileTerrain.Grassland, 1));
        grid.SetTileAt(0, 1, Tile.Create(TileTerrain.Water));

        grid.SetTileAt(1, 0, Tile.Create(TileTerrain.Grassland));
        grid.SetTileAt(1, 1, Tile.Create(TileTerrain.Grassland));

        grid.SetTileAt(2, 0, Tile.Create(TileTerrain.Water));
        grid.SetTileAt(2, 1, new Tile(TileOwner.Player2, TileTerrain.Grassland, 1));

        return grid;
    }

    public static Hexgrid<Tile> TiniestMap(TileTerrain player1Terrain, int player1Units, TileTerrain player2Terrain, int player2Units)
    {
        var grid = new Hexgrid<Tile>(2, 1);

        grid.SetTileAt(0, 0, new Tile(TileOwner.Player1, player1Terrain, player1Units));
        grid.SetTileAt(1, 0, new Tile(TileOwner.Player2, player2Terrain, player2Units));

        return grid;
    }
}