using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Sprite sprite1, sprite2;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject highlight, invalidHighlight, range;

    private bool exited = false;

    // Stores G, H and E costs for pathfinding
    public int G; 
    public int H;
    public float E;

    public TileType tileType;

    // get function to return the F cost for pathfinding
    public int F { 
        get 
        { 
            return G + H + Mathf.RoundToInt(E); 
        } 
    }

    public TileData parentTile; // stores the parent tile for pathfinding

    // used for grass tiles to change the sprite creating checkebox grid (sprite are the same apart from colour difference)
    public void SetColour(bool isOffset)
    {
        spriteRenderer.sprite = isOffset ? sprite1 : sprite2;
    }

    // sets the tile type of the tile
    public void SetTileType(TileType tileType)
    {
        this.tileType = tileType;
    }

    // checks if tile is walkable returning true/false
    private bool IsTileWalkable(TileData tile)
    {
        Character selectedCharacter = InputManager.Instance.selectedCharacter;

        if(selectedCharacter != null)
        {
            return tile.walkable || (selectedCharacter.characterCard.cardType == CardType.Water && tileType == TileType.Water);
        }

        return tile.walkable;
    }

    // highlights hovered grid tile
    void OnMouseOver()
    {
        if(GameManager.Instance.gameState == GameState.HeroesTurn) // ensure its the users turn
        {
            exited = false; // controls the coroutine break functionality

            TileData? tile = GridManager.Instance.GetTileData(Camera.main.ScreenToWorldPoint(Input.mousePosition)); // gets the tile user is hovering over from vector position of mouse

            if(tile.HasValue)
            {
                StartCoroutine(Highlight(IsTileWalkable(tile.Value))); // begins the coroutine
            }
        }   
    }

     // coroutine used to delay slightly the highlighting of the cell (purely visual improvement)
    IEnumerator Highlight(bool walkable)
    {
        yield return new WaitForSeconds(.01f);

        if (exited) // if user is no longer hovering over tile break
        {
            yield break;
        }
        if (walkable) // if tile is walkable white highlight is shown
        {
            highlight.SetActive(true);
        }
        else // if tile is not walkable, red highlight is shown
        {
            invalidHighlight.SetActive(true);
        }
    }

    void OnMouseExit()
    {
        exited = true;
        // de-activate all the highlight GameObjects
        highlight.SetActive(false);
        invalidHighlight.SetActive(false);
    }

    public void ShowRangeTile()
    {
        // activates the range highlight for the this tile
        range.SetActive(true);
    }
    
    public void HideRangeTile()
    {
        // de-activates the range highlight for this tile
        range.SetActive(false);
    }
}