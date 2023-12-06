using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// find path between a start and destination tileData tiles
public class Pathfinding
{
    public List<TileData> FindPath(TileData start, TileData destination, CardType cardType, int range = 0, bool avoidOpponent = false)
    {
        List<Character> heroCharacters = InputManager.Instance.charactersInPlay;

        List<TileData> openList = new(); // list of not yet considered tiles
        List<TileData> closedList = new(); // list of considered tiles

        openList.Add(start); // add initial start tile

        while (openList.Count > 0)
        {
            // order open list to get the highest F value from the list, allowing us to consider only the best possible next move in the path
            TileData currentTile = openList.OrderBy(x => x.tile.F).First(); 

            // remove from open and put into closed
            openList.Remove(currentTile); 
            closedList.Add(currentTile);

            if(currentTile.tile == destination.tile) // if we have reached destination, stop traversing the grid
            {
                return GetPathList(start, destination, cardType, range); // get the final path
            }

            foreach( TileData tile in GetNeighbourTiles(currentTile)) // loop through all neigouring tiles of the current tile
            {
                // check if tile is walkable, or if the character being moved is a water card and the tile being looked at is of type water
                // also ensure our cloest list doesn't contain this tile, as if it does we know it doesn't help in finding the best path
                if((tile.walkable || (tile.tile.tileType == TileType.Water && cardType == CardType.Water)) && !closedList.Contains(tile))
                {
                    // calculate the G and H costs for the tile
                    tile.tile.G = GetDistanceCost(start, tile); 
                    tile.tile.H = GetDistanceCost(destination, tile);
                    tile.tile.E = avoidOpponent ? GetEnemyCost(tile, heroCharacters) : 0; // nullifies the E cost to 0 if werent not avoiding opponents

                    tile.tile.parentTile = currentTile; // set the parent tile to the current tile (used at the end to construct the path)

                    if (!openList.Contains(tile)) // add open list if not already in there
                    {
                        openList.Add(tile);
                    }
                } else
                {
                    continue; // continue if not a valid tile or already disgarded in the cloesed list
                }
            }
        }

        return new List<TileData>(); // if no path is possible return empty list
    }

    // constucts and returns the final path 
    private List<TileData> GetPathList(TileData start, TileData destination, CardType cardType, int range)
    {
        List<TileData> finishedList = new();

        TileData currentTile = destination; // works from back to front 

        while (currentTile.tile != start.tile) // loopd through each tile 
        {
            finishedList.Add(currentTile); // adding it to the finished list
            currentTile = currentTile.tile.parentTile; // and makes the next tile to add the parent of the previous 
        }

        finishedList.Reverse(); // once the path has been constructed, it is reversed to get the path from start -> destination

        // AI movement needs to be restircted to the range of the character (range defaults to 0)
        if (range != 0 && finishedList.Count > range) // check if path needs to be truncated
        {
            finishedList.RemoveRange(range, finishedList.Count - range); // will truncate the path to the allowed range of the character
        }

        return finishedList;
    }

    // calculates the G and H cost between two tiles
    private int GetDistanceCost(TileData start, TileData tile)
    {
        return Mathf.Abs(start.gridLocation.x - tile.gridLocation.x) + Mathf.Abs(start.gridLocation.y - tile.gridLocation.y);
    }

    // Calcualtes E cost (enemy cost), used for Aggressor and Default playStyles, finding routes to avoid hero characters
    private float GetEnemyCost(TileData tile, List<Character> heroCharacters)
    {
        float totalInfluence = 0.0f; // initial inflience is none

        foreach (var hero in heroCharacters) // loop through all hero characters in play
        {
            float distance = Vector2Int.Distance(tile.gridLocation, hero.standingOnTile.gridLocation); // get the distance between the hero and the current tile
            totalInfluence += 1.0f / (distance + 1.0f); // calcualates total influences from 0 - 1 (0 = minimum influence on tile, 1 = maximum influence on tile)
        }

        float heroCost = totalInfluence * 40.0f; // has high weighting to ensure characters avoid hero characters almost at all costs

        return heroCost;
    }

    // returns a list of tileData tiles which neighbour the given tile
    public List<TileData> GetNeighbourTiles(TileData currentTile)
    {
        var tiles = GridManager.Instance.tileData; // gets access to all tileData

        List<TileData> neighbours = new();

        // tile above 
        Vector2Int checkLocation = new(currentTile.gridLocation.x + 1, currentTile.gridLocation.y);
        if(tiles.ContainsKey(checkLocation)) // used dictionary in order to check if the tile were looking for exits
        {
            neighbours.Add(tiles[checkLocation]); // if it does we add it to the neighbours list, ready for it to be checked
        }

        // tile below
        checkLocation = new(currentTile.gridLocation.x - 1, currentTile.gridLocation.y);
        if (tiles.ContainsKey(checkLocation))
        {
            neighbours.Add(tiles[checkLocation]);
        }

        // tile to the right 
        checkLocation = new(currentTile.gridLocation.x, currentTile.gridLocation.y + 1);
        if (tiles.ContainsKey(checkLocation))
        {
            neighbours.Add(tiles[checkLocation]);
        }

        // tile to the left
        checkLocation = new(currentTile.gridLocation.x, currentTile.gridLocation.y - 1);
        if (tiles.ContainsKey(checkLocation))
        {
            neighbours.Add(tiles[checkLocation]);
        }

        return neighbours;
    }
}
