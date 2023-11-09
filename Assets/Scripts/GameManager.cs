using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState GameState;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ChangeGameState(GameState.InitialiseGrid);
    }

    public void ChangeGameState(GameState newState) 
    {
        GameState = newState;
        switch(newState)
        {
            case GameState.InitialiseGrid:
                GridManager.Instance.InitialiseGrid();
                break;

            case GameState.InitialiseCards:
                CardManager.Instance.GenerateHands();
                break;

            case GameState.HeroesTurn:
                break;

            case GameState.EnemiesTurn:
                Debug.Log("enemies turn");
                ChangeGameState(GameState.HeroesTurn);
                break;
        }
    }
}
