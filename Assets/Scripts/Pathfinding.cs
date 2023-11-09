using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding
{
    public List<TileData> FindPath(TileData start, TileData destination)
    {
        List<TileData> openList = new();
        List<TileData> closedList = new();

        openList.Add(start);

        while (openList.Count > 0)
        {
            TileData currentTile = openList.OrderBy(x => x.tile.F).First();

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            if(currentTile.tile == destination.tile)
            {
                return GetPathList(start, destination);
            }

            foreach( TileData tile in GetNeighbourTiles(currentTile))
            {
                if(!tile.walkable || closedList.Contains(tile)) 
                {
                    continue;
                }

                tile.tile.G = GetDistanceCost(start, tile);
                tile.tile.H = GetDistanceCost(destination, tile);

                tile.tile.parentTile = currentTile;

                if(!openList.Contains(tile))
                {
                    openList.Add(tile);
                }
            }
        }

        return new List<TileData>();
    }

    private List<TileData> GetPathList(TileData start, TileData destination)
    {
        List<TileData> finishedList = new();

        TileData currentTile = destination;

        while(currentTile.tile != start.tile )
        {
            finishedList.Add(currentTile);
            currentTile = currentTile.tile.parentTile;
        }

        finishedList.Reverse();

        return finishedList;
    }

    private int GetDistanceCost(TileData start, TileData tile)
    {
        return Mathf.Abs(start.gridLocation.x - tile.gridLocation.x) + Mathf.Abs(start.gridLocation.y - tile.gridLocation.y);
    }

    public List<TileData> GetNeighbourTiles(TileData currentTile)
    {
        var tiles = GridManager.Instance.tileData;

        List<TileData> neighbours = new();

        Vector2Int checkLocation = new(currentTile.gridLocation.x + 1, currentTile.gridLocation.y);
        if(tiles.ContainsKey(checkLocation))
        {
            neighbours.Add(tiles[checkLocation]);
        }

        checkLocation = new(currentTile.gridLocation.x - 1, currentTile.gridLocation.y);
        if (tiles.ContainsKey(checkLocation))
        {
            neighbours.Add(tiles[checkLocation]);
        }

        checkLocation = new(currentTile.gridLocation.x, currentTile.gridLocation.y + 1);
        if (tiles.ContainsKey(checkLocation))
        {
            neighbours.Add(tiles[checkLocation]);
        }

        checkLocation = new(currentTile.gridLocation.x, currentTile.gridLocation.y - 1);
        if (tiles.ContainsKey(checkLocation))
        {
            neighbours.Add(tiles[checkLocation]);
        }

        return neighbours;
    }
}
