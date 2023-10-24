using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [SerializeField] private int gridWidth, gridHeight;
    [SerializeField] private Tile tile;
    [SerializeField] private Transform mainCamera;

    private TileData[ , ] tileData;

    private void Awake()
    {
        Instance = this;
    }

    public void InitialiseGrid()
    {
        tileData = new TileData[gridWidth, gridHeight];
        GenerateGrid();
        CenterCamera();
    }

    private void CenterCamera()
    {
        float centerX = gridWidth / 2 - .5f;
        float centerY = gridHeight / 2f - .5f;

        mainCamera.transform.position = new Vector3(centerX, centerY, -10f);
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 tilePos = new(x, y, 0);
                Tile newTile = Instantiate(tile, tilePos, Quaternion.identity);

                newTile.name = $"{x},{y}";
                newTile.transform.parent = this.gameObject.transform;

                newTile.SetColour((x + y) % 2 == 0);

                tileData[x, y] = new TileData { tile = newTile, position = tilePos };
            }
        }
    }

    public Tile GetTileData(Vector2 mousePos)
    {
        int x = Mathf.RoundToInt((mousePos.x));
        int y= Mathf.RoundToInt((mousePos.y));

        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return tileData[x, y].tile;
        }

        return null;
    }
}