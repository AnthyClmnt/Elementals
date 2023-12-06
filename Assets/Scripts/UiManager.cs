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
        statsCanvas.enabled = false; // initially stats canvas is not visible as no character on the board is selected
    }

    public void HandleCharacterStatsUI(Character character) // handles the showing / hiding of the stats UI
    {
        statsCanvas.enabled = !statsCanvas.enabled; // will inverse the visability to on/off

        if (statsCanvas.enabled) // if now enabled
        {
            // will set the text of the UI
            charName.text = character.name;
            healthText.text = $"{character.characterCard.currHealth} / {character.characterCard.health}";
            attackText.text = $"{character.characterCard.attack}";
            rangeText.text = $"{character.characterCard.range}";
        }
    }
}
