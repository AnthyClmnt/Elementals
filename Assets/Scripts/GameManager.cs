using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState gameState;

    public GameState previousGameState;

    public TMP_Text turnText;

    public GameDifficulty gameDifficulty;

    private void Awake()
    {
        Instance = this;

        gameDifficulty = (GameDifficulty)PlayerPrefs.GetInt("SelectedDifficulty", 1);
    }

    private void Start()
    {
        ChangeGameState(GameState.InitialiseGrid); // initial game state is to construct the grid
    }

    // changes the state of the game
    public void ChangeGameState(GameState newState, bool pauseState = false)
    {
        if (pauseState) // pause state needs to know which state to resume with
        {
            previousGameState = gameState;
        }
        
        gameState = newState; // set the new game state
        switch(gameState)
        {
            case GameState.Pause:
                break;

            case GameState.Resume:
                ChangeGameState(previousGameState);
                break;

            case GameState.InitialiseGrid:
                GridManager.Instance.InitialiseGrid(); // calls the grid manager script to generate the grid
                break;

            case GameState.InitialiseCards:
                CardManager.Instance.GenerateHands(); // calls the card manager to generate the hands
                break;

            case GameState.HeroesTurn:
                turnText.text = "Hero's Turn"; // updates the text showing who's turn it is
                break;

            case GameState.EnemiesTurn:
                turnText.text = "Enemie's Turn";
                break;

            case GameState.HeroWin:
                SceneManager.LoadScene(2); // loads the game over/winning screen
                break;

            case GameState.EnemyWin:
                SceneManager.LoadScene(2);
                break;
        }
    }
}
