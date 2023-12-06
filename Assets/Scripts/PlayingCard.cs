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
        // stores the origional positon and scale of the playingCard before any animation occurs
        originalScale = transform.localScale; 
        origionalPos = transform.position;
        targetScale = new Vector3(originalScale.x * 1.2f, originalScale.y * 1.2f, originalScale.z * 1.2f);
    }

    // utulity function to set text of the health
    public void SetHealth(string text)
    {
        healthText.text = text;
    }

    // utulity function to set text of the attack damage
    public void SetAttack(string text)
    {
        attackText.text = text;
    }

    // utulity function to set text of the range
    public void SetRange(string text)
    {
        rangeText.text = text;
    }

    // controls if user is allowed to select a card
    private bool SelectionAllowed()
    {
        return CardManager.Instance.CardSelectedAllowed() && GameManager.Instance.gameState == GameState.HeroesTurn; // checks if it is the users turn and if no other cards are currently selected
    }

    private void OnMouseEnter()
    {
        if (!selected && SelectionAllowed()) // ensure its not already selected and selection allowed
        {
            // animation to scale the card and move it up 
            LeanTween.scale(gameObject, targetScale, transitionTime).setEase(LeanTweenType.easeOutSine);
            LeanTween.moveY(gameObject, origionalPos.y + 1f, transitionTime).setEase(LeanTweenType.easeOutSine);
        }
    }
    
    private void OnMouseExit()
    {
        if (!selected && SelectionAllowed()) // ensure its not already selected and selection allowed
        {
            // animation to return to origional scale and position
            LeanTween.scale(gameObject, originalScale, transitionTime).setEase(LeanTweenType.easeOutSine);
            LeanTween.moveY(gameObject, origionalPos.y, transitionTime).setEase(LeanTweenType.easeOutSine);
        }
    }

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0) && (SelectionAllowed() || selected)) // if user clicks card while over it, and this is the selected card or no other cards are selected
        {
            selected = !selected; // inverse the selection: selection -> unselected and unselected -> selected
        }
    }
}
