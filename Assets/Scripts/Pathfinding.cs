using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Pathfinding
{
    public List<TileData> FindPath(TileData start, TileData destination, CardType cardType, int range = 0)
    {
        if (destination.character != null)
        {
            return new List<TileData>();
        }

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
                return GetPathList(start, destination, range);
            }

            foreach( TileData tile in GetNeighbourTiles(currentTile))
            {
                if((tile.walkable || (tile.tile.tileType == TileType.Water && cardType == CardType.Water)) && !closedList.Contains(tile))
                {
                    tile.tile.G = GetDistanceCost(start, tile);
                    tile.tile.H = GetDistanceCost(destination, tile);

                    tile.tile.parentTile = currentTile;

                    if (!openList.Contains(tile))
                    {
                        openList.Add(tile);
                    }
                } else
                {
                    continue;
                }
            }
        }

        Debug.Log("hi");
        return new List<TileData>();
    }

    private List<TileData> GetPathList(TileData start, TileData destination, int range)
    {
        List<TileData> finishedList = new();

        TileData currentTile = destination;

        while(currentTile.tile != start.tile )
        {
            finishedList.Add(currentTile);
            currentTile = currentTile.tile.parentTile;
        }

        finishedList.Reverse();

        if (range != 0 && finishedList.Count > range)
        {
            finishedList.RemoveRange(range, finishedList.Count - range);
        }

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
