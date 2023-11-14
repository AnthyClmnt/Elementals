using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RangeFinding
{
    private Pathfinding pathfinding;

    public List<TileData> GetRangeTiles(TileData startingTile, int range, CardType cardType, bool ignoreWalkable = false)
    {
        pathfinding = new Pathfinding(); 
        
        List<TileData> inRangeTiles = new();
        int stepCount = 0;

        inRangeTiles.Add(startingTile);

        var tilesForPreviousStep = new List<TileData>();
        tilesForPreviousStep.Add(startingTile);

        while(stepCount < range)
        {
            var surroundingTiles = new List<TileData>();

            foreach(var tile in tilesForPreviousStep)
            {
                var tiles = pathfinding.GetNeighbourTiles(tile);
                foreach(var tilez in tiles)
                {
                    if(tilez.walkable || (tilez.tile.tileType == TileType.Water && cardType == CardType.Water) || ignoreWalkable)
                    {
                        surroundingTiles.Add(tilez);
                    }
                }
            }

            inRangeTiles.AddRange(surroundingTiles);
            tilesForPreviousStep = surroundingTiles.Distinct().ToList();
            stepCount++;
        }

        return inRangeTiles.Distinct().ToList();
    }

    public List<TileData> GetDefenderTiles()
    {
        var tiles = GridManager.Instance.tileData;
        var gridWidth = GridManager.Instance.gridWidth;
        var gridHeight = GridManager.Instance.gridHeight;

        List<TileData> inRangeTiles = new List<TileData>();

        for ( int x = Mathf.RoundToInt(gridWidth / 2); x < gridWidth; x++ )
        {
            for ( int y = 0; y < gridHeight; y++)
            {
                Vector2Int pos = new(x, y);

                if (tiles[pos].character != null && tiles[pos].character.type == MobType.Hero)
                {
                    inRangeTiles.Add(tiles[pos]);
                }
            }
        }

        return inRangeTiles;
    }
}