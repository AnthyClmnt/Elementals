using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void LoadMenu() // loads the main menu scene
    {
        SceneManager.LoadScene(0);
    }

    public void PlayGame() // loads the game scene
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame() // quits the game
    {
        Debug.Log("Quitting");
        Application.Quit();
        EditorApplication.isPlaying = false;
    }
}
