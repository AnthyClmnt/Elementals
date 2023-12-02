using UnityEngine;

public class ControlsPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    public void OpenPanel()
    {
        GameManager.Instance.ChangeGameState(GameState.Pause, true);
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        GameManager.Instance.ChangeGameState(GameState.Resume);
    }
}
