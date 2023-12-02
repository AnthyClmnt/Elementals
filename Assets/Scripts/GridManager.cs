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
        tileData = new Dictionary<Vector2Int, TileData>();

        GenerateGrid();
        CreateShrines();
        PositionCamera();
        GameManager.Instance.ChangeGameState(GameState.InitialiseCards);
    }

    private void PositionCamera()
    {
        float cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float cameraHalfHeight = mainCamera.orthographicSize;

        float cameraX = cameraHalfWidth - 1;
        float cameraY = gridHeight - 1 - cameraHalfHeight + 1;

        mainCamera.transform.position = new Vector3(cameraX, cameraY, -10f);

        Vector3 cardCanvasPos = mainCamera.transform.position;
        cardCanvasPos.z = -1;
        cardCanvasPos.y -= cameraHalfHeight - 1.75f;

        cardCanvas.transform.position = cardCanvasPos;
    }

    private void GenerateGrid()
    {
        bool validGrid = false;

        while (!validGrid)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3Int tilePos = new(x, y, 0);

                    bool isGrass = Random.value > .3;

                    if (x < 2 || x >= gridWidth - 2)
                    {
                        isGrass = true;
                    }

                    float randomTile = Random.value;
                    Tile selectedTile = isGrass ? tiles[0] : randomTile > .5 ? tiles[1] : tiles[2];

                    Tile newTile = Instantiate(selectedTile, tilePos, Quaternion.identity);

                    newTile.SetTileType(isGrass ? TileType.Grass : randomTile > .5 ? TileType.Water : TileType.Rock);

                    newTile.name = $"{x},{y}";
                    newTile.transform.parent = this.gameObject.transform;

                    if (isGrass)
                    {
                        newTile.SetColour((x + y) % 2 == 0);
                    }
                    tileData.Add(new Vector2Int(x, y), new TileData { tile = newTile, gridLocation = (Vector2Int)tilePos, walkable = isGrass });
                }
            }

            var path = pathfinding.FindPath(tileData[new(0, 0)], tileData[new(gridWidth - 1, gridHeight - 1)], CardType.Fire);

            if (path.Count > 0)
            {
                validGrid = true;
            } else
            {
                foreach (var tile in tileData.Values)
                {
                    Destroy(tile.tile.gameObject);
                }
                tileData.Clear();
            }
        }
    }

    private void CreateShrines()
    {
        int halfGridHeight = (gridHeight - 1) / 2;
        int minShrineRange = Mathf.RoundToInt(halfGridHeight - (.25f * halfGridHeight));
        int maxShrineRange = Mathf.RoundToInt(halfGridHeight + (.75f * halfGridHeight));

        var randomY = Random.Range(minShrineRange, maxShrineRange);

        GameObject heroShrineObject = Instantiate(shrine, new Vector3(0, randomY, -1), Quaternion.identity);

        if (heroShrineObject.TryGetComponent<Shrine>(out var heroShrine))
        {
            TileData heroShrineTile = tileData[new Vector2Int(0, randomY)];

            heroShrine.InitShineData(heroShrineTile.tile, MobType.Hero);

            heroShrineObject.name = "Hero Shrine";
            heroShrineObject.transform.parent = heroShrineTile.tile.transform;

            heroShrineTile.shrineLocation = true;
            heroShrineTile.shrine = heroShrine;
            tileData[heroShrineTile.gridLocation] = heroShrineTile;

            heroShineTileData = heroShrineTile;
        }

        randomY = Random.Range(minShrineRange, maxShrineRange);

        GameObject enemyShrineObject = Instantiate(shrine, new Vector3(gridWidth - 1, randomY, -1), Quaternion.identity);

        if (enemyShrineObject.TryGetComponent<Shrine>(out var enemyShrine))
        {
            TileData enemyShrineTile = tileData[new Vector2Int(gridWidth - 1, randomY)];

            enemyShrine.InitShineData(enemyShrineTile.tile, MobType.Enemy);

            enemyShrineObject.name = "Enemy Shrine";
            enemyShrineObject.transform.parent = enemyShrineTile.tile.transform;

            enemyShrineTile.shrineLocation = true;
            enemyShrineTile.shrine = enemyShrine;
            tileData[enemyShrineTile.gridLocation] = enemyShrineTile;

            enemyShineTileData = enemyShrineTile;
        }
    }
    
    public void SetCharacterOnTile(Character character, Vector2Int gridLocaton)
    {
        if (tileData.ContainsKey(gridLocaton))
        {
            TileData tile = tileData[gridLocaton];

            tile.character = character;

            tileData[gridLocaton] = tile;
        }
    }

    public void RemoveCharacterFromTile(Vector2Int gridLocaton)
    {
        if (tileData.ContainsKey(gridLocaton))
        {
            TileData OrigionalTile = tileData[gridLocaton];

            OrigionalTile.character = null;
            TileData editedTile = new TileData { tile = OrigionalTile.tile, gridLocation = OrigionalTile.gridLocation, walkable = OrigionalTile.walkable };

            tileData[gridLocaton] = editedTile;
        }
    }

    public TileData? GetTileData(Vector2 pos)
    {
        int x = Mathf.RoundToInt((pos.x));
        int y= Mathf.RoundToInt((pos.y));
        Vector2Int coord = new(x, y);

        if (tileData.TryGetValue(coord, out TileData tile))
        {
            return tile;
        }

        return null;
    }
}