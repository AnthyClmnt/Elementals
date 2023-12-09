using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    private Pathfinding pathfinding;

    [SerializeField] public int gridWidth, gridHeight;
    [SerializeField] private Tile[] tiles;
    [SerializeField] private GameObject shrine; // probably needs to change to script 
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas cardCanvas;

    public Dictionary<Vector2Int, TileData> tileData;

    public TileData heroShineTileData;
    public TileData enemyShineTileData;

    private void Awake()
    {
        Instance = this;
        pathfinding = new Pathfinding();
    }

    public void InitialiseGrid()
    {
        tileData = new Dictionary<Vector2Int, TileData>(); // initialises the dictionary of tileData

        GenerateGrid(); // firtly generates the grid
        CreateShrines(); // then generates the hero and enemy shrines
        PositionCamera(); // positions the game camera

        EventSystem.RaiseGameStateChange(GameState.InitialiseCards); // when finished passed game state to creating the hands
    }

    // easier to move the camera to centre the grid, rather than offsetting the tiles of the grid 
    private void PositionCamera()
    {
        // fixes the grid in the top left of the camera
        float cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float cameraHalfHeight = mainCamera.orthographicSize;

        float cameraX = cameraHalfWidth - 1;
        float cameraY = gridHeight - 1 - cameraHalfHeight + 1;

        mainCamera.transform.position = new Vector3(cameraX, cameraY, -10f);

        // the card slots also need to be moved to be slightly off the bottom-centre of the screen
        Vector3 cardCanvasPos = mainCamera.transform.position;
        cardCanvasPos.z = -1;
        cardCanvasPos.y -= cameraHalfHeight - 1.75f;

        cardCanvas.transform.position = cardCanvasPos;
    }

    // Generates the grid
    private void GenerateGrid()
    {
        bool validGrid = false; // used to ensure grid generared can actually be traversed 

        while (!validGrid) // contunously looping through generation until valid grid is made
        {
            // loop through each x and y co-ordinate of the grid
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3Int tilePos = new(x, y, 0); // get the vector position of the new tile

                    bool isGrass = Random.value > .3; // 70% of the tiles are grass

                    if (x < 2 || x >= gridWidth - 2) // spawn tiles for both hero and enemy must be grass
                    {
                        isGrass = true;
                    }

                    float randomTile = Random.value;
                    Tile selectedTile = isGrass ? tiles[0] : randomTile > .5 ? tiles[1] : tiles[2]; // if not greas randomly choose between water and rock

                    Tile newTile = Instantiate(selectedTile, tilePos, Quaternion.identity); // create the tile gameObject

                    newTile.SetTileType(isGrass ? TileType.Grass : randomTile > .5 ? TileType.Water : TileType.Rock); // set the tile type (changes the sprite)

                    newTile.name = $"{x},{y}";
                    newTile.transform.parent = this.gameObject.transform;

                    if (isGrass)
                    {
                        newTile.SetColour((x + y) % 2 == 0); // to show grid, checkerbox pattern applied to grass tiles (slightly chaning the colour)
                    }
                    tileData.Add(new Vector2Int(x, y), new TileData { tile = newTile, gridLocation = (Vector2Int)tilePos, walkable = isGrass }); // add the tile to the dictionary
                }
            }

            // once grid is generated path finding ensures at least a path between the top left and bottom right of grid
            var path = pathfinding.FindPath(tileData[new(0, 0)], tileData[new(gridWidth - 1, gridHeight - 1)], CardType.Fire);

            if (path.Count > 0) // if one is found, the loop can end 
            {
                validGrid = true;
            } else
            {
                foreach (var tile in tileData.Values)
                {
                    Destroy(tile.tile.gameObject); // otherwise remove all the gameObjects 
                }
                tileData.Clear(); // and clear the dictionary before trying again
            }
        }
    }

    // Genrates the hero and enemy (AI) shines
    private void CreateShrines()
    {
        // limits allowed tile range to close to the middle of the grid
        int halfGridHeight = (gridHeight - 1) / 2;
        int minShrineRange = Mathf.RoundToInt(halfGridHeight - (.25f * halfGridHeight));
        int maxShrineRange = Mathf.RoundToInt(halfGridHeight + (.75f * halfGridHeight));

        
        var randomY = Random.Range(minShrineRange, maxShrineRange); // chooses random Y value to spawn shrine
        GameObject heroShrineObject = Instantiate(shrine, new Vector3(0, randomY, -1), Quaternion.identity); // spawns in shrine gameObject
        PopulateShrineData(heroShrineObject, randomY, false); // populates initial shrine and tileData data

        randomY = Random.Range(minShrineRange, maxShrineRange);
        GameObject enemyShrineObject = Instantiate(shrine, new Vector3(gridWidth - 1, randomY, -1), Quaternion.identity);
        PopulateShrineData(enemyShrineObject, randomY);
    }

    private void PopulateShrineData(GameObject shrineObject, int Y, bool enemyShrine = true)
    {
        if (shrineObject.TryGetComponent<Shrine>(out Shrine shrine)) // gets the Shrine script from the gameObject
        {
            TileData shrineTile = tileData[new Vector2Int(enemyShrine ? gridWidth - 1 : 0, Y)]; // gets the tileData the shrine is on

            shrine.InitShineData(shrineTile.tile, enemyShrine ? MobType.Enemy : MobType.Hero); // sets the inital shrine data

            shrineObject.name = enemyShrine ? "Enenmy Shrine" : "Hero Shrine";
            shrineObject.transform.parent = shrineTile.tile.transform;

            shrineTile.shrineLocation = true; // set the tileData to know its a shrine location
            shrineTile.shrine = shrine; // sets the Shrine to the new shrine  
            tileData[shrineTile.gridLocation] = shrineTile; // updates the dictionary to the upated tileData

            // used for easier access to know the grid location of the shrine, set either the enemy or hero shrine tile data
            if (enemyShrine)
            {
                enemyShineTileData = shrineTile;
            }
            else
            {
                heroShineTileData = shrineTile;
            }
        }
    }
    
    // sets character within a tile to the passed character
    public void SetCharacterOnTile(Character character, Vector2Int gridLocaton)
    {
        // check the vector given is within the dictionary
        if (tileData.ContainsKey(gridLocaton))
        {
            TileData tile = tileData[gridLocaton];

            tile.character = character; // sets the character 

            tileData[gridLocaton] = tile; // updates tileData dictionary with updated tile
        }
    }

    // sets character within a tile to null
    public void RemoveCharacterFromTile(Vector2Int gridLocaton)
    {
        // check the vector given is within the dictionary 
        if (tileData.ContainsKey(gridLocaton))
        {
            TileData tile = tileData[gridLocaton];

            tile.character = null; // remove the character

            tileData[gridLocaton] = tile; // updates tileData dictionary with updated tile
        }
    }

    // utility function used to get tileData from a Vector2
    public TileData? GetTileData(Vector2 pos)
    {
        int x = Mathf.RoundToInt((pos.x));
        int y= Mathf.RoundToInt((pos.y));
        Vector2Int coord = new(x, y);

        if (tileData.TryGetValue(coord, out TileData tile)) // if tile exists in dictionary return the tile
        {
            return tile;
        }

        return null; // otherwise reutrn null
    }
}