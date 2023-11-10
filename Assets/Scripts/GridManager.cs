using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [SerializeField] private int gridWidth, gridHeight;
    [SerializeField] private Tile[] tiles;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas cardCanvas, statsCanvas;

    public Dictionary<Vector2Int, TileData> tileData;

    private void Awake()
    {
        Instance = this;
    }

    public void InitialiseGrid()
    {
        tileData = new Dictionary<Vector2Int, TileData>();
        GenerateGrid();
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

        Vector3 statsCanvasPos = mainCamera.transform.position;
        statsCanvasPos.z = -1;
        statsCanvasPos.y += cameraHalfHeight;
        statsCanvasPos.x += cameraHalfWidth;

        statsCanvas.transform.position = statsCanvasPos;
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3Int tilePos = new(x, y, 0);

                bool isGrass = Random.value > .3;

                if(x < 2 || x >= gridWidth - 2)
                {
                    isGrass = true;
                }

                Tile selectedTile = isGrass ? tiles[0] : Random.value > .5 ? tiles[1] : tiles[2];

                Tile newTile = Instantiate(selectedTile, tilePos, Quaternion.identity);

                newTile.name = $"{x},{y}";
                newTile.transform.parent = this.gameObject.transform;

                if (isGrass) {
                    newTile.SetColour((x + y) % 2 == 0);
                }
                tileData.Add(new Vector2Int(x, y), new TileData { tile = newTile, gridLocation = (Vector2Int)tilePos, walkable = isGrass });
            }
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