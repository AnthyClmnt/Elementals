using TMPro;
using UnityEngine;

public class UiManager: MonoBehaviour
{
    public static UiManager Instance;

    public Canvas statsCanvas;

    public TMP_Text charName;
    public TMP_Text healthText;
    public TMP_Text attackText;
    public TMP_Text rangeText;

    private void Awake()
    {
        Instance = this;
        statsCanvas.enabled = false;
    }

    public void HandleCharacterStatsUI(Character character)
    {
        if (statsCanvas.enabled)
        {
            statsCanvas.enabled = false; 
        } else
        {
            statsCanvas.enabled = true;

            charName.text = character.name;
            healthText.text = $"{character.characterCard.currHealth} / {character.characterCard.health}";
            attackText.text = $"{character.characterCard.attack}";
            rangeText.text = $"{character.characterCard.range}";
        }
    }
}
