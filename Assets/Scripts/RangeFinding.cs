using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RangeFinding
{
    private Pathfinding pathfinding;

    // will find and return list of tile within range of a given character
    public List<TileData> GetRangeTiles(TileData startingTile, int range, CardType cardType, bool ignoreWalkable = false)
    {
        pathfinding = new Pathfinding(); // uses pathfinding to know if within range
        
        List<TileData> inRangeTiles = new(); // initilly empty list of in range tiles
        int stepCount = 0; // counter to keep track of how far away from origionl tile we have gone

        inRangeTiles.Add(startingTile); // add the initial tile

        var tilesForPreviousStep = new List<TileData>(); // keep track of the previous steps tiles
        tilesForPreviousStep.Add(startingTile); // also adds the initial tile

        while(stepCount < range) // loop until we hit the range allowed
        {
            var surroundingTiles = new List<TileData>();

            foreach(var tile in tilesForPreviousStep) // loop through all the tiles in the previous step
            {
                var neighbourTiles = pathfinding.GetNeighbourTiles(tile); // get their neighbour tiles
                foreach(var neighbourTile in neighbourTiles) // loop through the neighbouring tiles
                {
                    // check if its walkable, but range also used for attack so ignoreWalkable bool used so water character can still be attacked when standing in a water tile
                    if (neighbourTile.walkable || (neighbourTile.tile.tileType == TileType.Water && cardType == CardType.Water) || ignoreWalkable)
                    {
                        surroundingTiles.Add(neighbourTile); // add to the surrouning tiles list
                    }
                }
            }

            inRangeTiles.AddRange(surroundingTiles); // add all the vaid surrounding tiles into inRangeTiles
            tilesForPreviousStep = surroundingTiles.Distinct().ToList(); // set up the next loop for have a distinct list of surrounding tiles to check
            stepCount++; // increment the step (range)
        }

        return inRangeTiles.Distinct().ToList(); // finally ensure a distint list of inRangeTiles and return 
    }

    // Returns list of tiles which contain a hero character and a defender character is able to attack from
    public List<TileData> GetDefenderTilesWithHeroCharacters()
    {
        // access to all tileData and gridHeight and gridWidth
        var tiles = GridManager.Instance.tileData; 
        var gridWidth = GridManager.Instance.gridWidth;
        var gridHeight = GridManager.Instance.gridHeight;

        List<TileData> heroCharacterTiles = new List<TileData>();

        for ( int x = Mathf.RoundToInt(gridWidth / 2); x < gridWidth; x++ ) // loop through the AI's side of the grid 
        {
            for ( int y = 0; y < gridHeight; y++)
            {
                Vector2Int pos = new(x, y);

                if (tiles[pos].character != null && tiles[pos].character.type == MobType.Hero) // checks if the current tile being loooked at contains a character which is of type Hero
                {
                    heroCharacterTiles.Add(tiles[pos]); // if so adds the tile to possible attack tiles
                }
            }
        }

        return heroCharacterTiles;
    }
}