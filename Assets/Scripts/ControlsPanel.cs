using UnityEngine;

public class ControlsPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    public void OpenPanel()
    {
        EventSystem.RaiseGameStateChange(GameState.Pause);
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        EventSystem.RaiseGameStateChange(GameState.Resume);
    }
}
