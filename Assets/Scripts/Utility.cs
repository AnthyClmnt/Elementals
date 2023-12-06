using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    public List<TileData> MoveAlongPath(List<TileData> path, Character pathCharacter)
    {
        var step = 4f * Time.deltaTime;

        pathCharacter.transform.position = Vector2.MoveTowards(pathCharacter.transform.position, path[0].gridLocation, step); // move to first/next path location
        pathCharacter.transform.position = new Vector3(pathCharacter.transform.position.x, pathCharacter.transform.position.y, -2f); // remain same z axis (rendering purposes)

        if (Vector2.Distance(pathCharacter.transform.position, path[0].gridLocation) < 0.00001f) // slight offset, as its possible character isn't perfectly aligned
        {
            PositionCharacterOnLine(path[0], pathCharacter); // if destination of path is the next step, set character to be standing on tile
            path.RemoveAt(0); // once at the first/next path location remove from the list
        }

        if (path.Count == 1) // if destination of path is the next step, set character to be standing on tile
        {
            GridManager.Instance.SetCharacterOnTile(pathCharacter, path[0].gridLocation);
        }

        return path;
    }

    // maintains sorting order, and positions character on the current path tile
    private void PositionCharacterOnLine(TileData tile, Character pathCharacter)
    {
        pathCharacter.transform.position = new Vector3(tile.gridLocation.x, tile.gridLocation.y + 0.0001f, pathCharacter.transform.position.z);
        pathCharacter.GetComponent<SpriteRenderer>().sortingOrder = tile.tile.GetComponent<SpriteRenderer>().sortingOrder; // maintains sorting order
        pathCharacter.standingOnTile = tile; // sets character to be standig on tile
    }
}
