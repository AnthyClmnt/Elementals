using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Sprite sprite1, sprite2;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject highlight, invalidHighlight, range;

    private bool exited = false;

    public int G;
    public int H;

    public TileType tileType;

    public int F { 
        get 
        { 
            return G + H; 
        } 
    }

    public TileData parentTile;

    public void SetColour(bool isOffset)
    {
        spriteRenderer.sprite = isOffset ? sprite1 : sprite2;
    }

    public void SetTileType(TileType tileType)
    {
        this.tileType = tileType;
    }

    private bool IsTileWalkable(TileData tile)
    {
        Character selectedCharacter = InputManager.Instance.selectedCharacter;

        if(selectedCharacter != null)
        {
            return tile.walkable || (selectedCharacter.characterCard.cardType == CardType.Water && tileType == TileType.Water);
        }

        return tile.walkable;
    }

    void OnMouseEnter()
    {
        if(GameManager.Instance.gameState == GameState.HeroesTurn)
        {
            exited = false;

            TileData tile = (TileData)GridManager.Instance.GetTileData(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            StartCoroutine(Highlight(IsTileWalkable(tile)));
        }   
    }

    IEnumerator Highlight(bool walkable)
    {
        yield return new WaitForSeconds(.01f);

        if (exited)
        {
            yield break;
        }
        if (walkable)
        {
            highlight.SetActive(true);
        }
        else
        {
            invalidHighlight.SetActive(true);
        }
    }

    void OnMouseExit()
    {
        exited = true;
        highlight.SetActive(false);
        invalidHighlight.SetActive(false);
    }

    public void ShowRangeTile()
    {
        range.SetActive(true);
    }
    
    public void HideRangeTile()
    {
        range.SetActive(false);
    }
}