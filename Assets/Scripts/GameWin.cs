using TMPro;
using UnityEngine;

public class GameWin : MonoBehaviour
{
    public TMP_Text text;

    public void Start()
    {
        text.text = GameManager.Instance.gameState == GameState.HeroWin ? "You won!!" : "Oh no, enemy won!";
    }
}
