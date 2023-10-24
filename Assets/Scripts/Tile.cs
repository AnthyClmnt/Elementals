using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color colour1, colour2;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject highlight;

    public void SetColour(bool isOffset)
    {
        spriteRenderer.color = isOffset ? colour2 : colour1;
    }

    void OnMouseEnter()
    {
        highlight.SetActive(true);
    }

    void OnMouseExit()
    {
        highlight.SetActive(false);
    }
}