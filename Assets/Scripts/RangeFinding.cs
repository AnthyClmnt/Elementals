using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RangeFinding
{
    private Pathfinding pathfinding;

    public List<TileData> GetRangeTiles(TileData startingTile, int range)
    {
        pathfinding = new Pathfinding(); 
        
        List<TileData> inRangeTiles = new();
        int stepCount = 0;

        inRangeTiles.Add(startingTile);

        var tilesForPreviousStep= new List<TileData>();
        tilesForPreviousStep.Add(startingTile);

        while(stepCount < range)
        {
            var surroundingTiles = new List<TileData>();

            foreach(var tile in tilesForPreviousStep)
            {
                var tiles = pathfinding.GetNeighbourTiles(tile);
                foreach(var tilez in tiles)
                {
                    if(tilez.walkable)
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
}