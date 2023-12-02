using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState gameState;

    public GameState previousGameState;

    public TMP_Text turnText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ChangeGameState(GameState.InitialiseGrid);
    }

    public void ChangeGameState(GameState newState, bool pauseState = false) 
    {
        if (pauseState)
        {
            previousGameState = gameState;
        }
        
        gameState = newState;
        switch(gameState)
        {
            case GameState.Pause:
                break;

            case GameState.Resume:
                ChangeGameState(previousGameState);
                break;

            case GameState.InitialiseGrid:
                GridManager.Instance.InitialiseGrid();
                break;

            case GameState.InitialiseCards:
                CardManager.Instance.GenerateHands();
                break;

            case GameState.HeroesTurn:
                turnText.text = "Hero's Turn";
                break;

            case GameState.EnemiesTurn:
                turnText.text = "Enemies' Turn";
                break;

            case GameState.HeroWin:
                Debug.Log("wooo hero won");
                SceneManager.LoadScene(2);
                break;

            case GameState.EnemyWin:
                Debug.Log("booo enemy won");
                SceneManager.LoadScene(2);
                break;
        }
    }
}
