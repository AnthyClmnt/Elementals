using TMPro;
using UnityEngine;

public class PlayingCard : MonoBehaviour
{
    public TMP_Text healthText;
    public TMP_Text attackText;
    public TMP_Text rangeText;

    public Vector3 targetScale; // Adjust the target scale as needed
    public float transitionTime = 0.25f;

    private Vector3 originalScale;
    private Vector3 origionalPos;

    public bool selected = false;

    private void Start()
    {
        originalScale = transform.localScale;
        origionalPos = transform.position;
        targetScale = new Vector3(originalScale.x * 1.2f, originalScale.y * 1.2f, originalScale.z * 1.2f);
    }

    public void SetHealth(string text)
    {
        healthText.text = text;
    }

    public void SetAttack(string text)
    {
        attackText.text = text;
    }

    public void SetRange(string text)
    {
        rangeText.text = text;
    }

    private bool SelectionAllowed()
    {
        return CardManager.Instance.CardSelectedAllowed() && GameManager.Instance.gameState == GameState.HeroesTurn;
    }

    private void OnMouseEnter()
    {
        if (!selected && SelectionAllowed())
        {
            LeanTween.scale(gameObject, targetScale, transitionTime).setEase(LeanTweenType.easeOutSine);
            LeanTween.moveY(gameObject, origionalPos.y + 1f, transitionTime).setEase(LeanTweenType.easeOutSine);
        }
    }
    
    private void OnMouseExit()
    {
        if (!selected && SelectionAllowed())
        {
            LeanTween.scale(gameObject, originalScale, transitionTime).setEase(LeanTweenType.easeOutSine);
            LeanTween.moveY(gameObject, origionalPos.y, transitionTime).setEase(LeanTweenType.easeOutSine);
        }
    }

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0) && (SelectionAllowed() || selected))
        {
            selected = !selected;
        }
    }
}
