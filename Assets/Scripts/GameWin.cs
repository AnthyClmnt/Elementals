using TMPro;
using UnityEngine;

public class GameWin : MonoBehaviour
{
    public TMP_Text text;

    // when scene is loaded the result of the game is shown
    public void Start() 
    {
        text.text = GameManager.Instance.gameState == GameState.HeroWin ? "You won!!" : "Enemy won :("; // change text of the game result
    }
}
