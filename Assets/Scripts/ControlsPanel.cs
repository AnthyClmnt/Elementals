using UnityEngine;

public class ControlsPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel; // reference to panel which opens/closes for game control help

    public void OpenPanel() // opens the panel
    {
        EventSystem.RaiseGameStateChange(GameState.Pause); // event system used for controlling the paused state of the game
        panel.SetActive(true);
    }

    public void ClosePanel() // closes the panel
    {
        panel.SetActive(false);
        EventSystem.RaiseGameStateChange(GameState.Resume); // event system used to un-pause the game
    }
}
