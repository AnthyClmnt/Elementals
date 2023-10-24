using System.Collections;
using System.Collections.Generic;
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
        ChangeGameState(GameState.InitialiseGame);
    }

    public void ChangeGameState(GameState newState) 
    {
        GameState = newState;
        switch(newState)
        {
            case GameState.InitialiseGame:
                GridManager.Instance.InitialiseGrid();
                break;
        }
    } 
}
