using System;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    public static Action<Character> OnHeroDeath;
    public static Action<Character> OnEnemyDeath;

    public static Action<Character> OnHeroCharacterMove;

    public static void RaiseHeroDeath(Character character)
    {
        OnHeroDeath?.Invoke(character);
    }

    public static void RaiseEnemyDeath(Character character)
    {
        OnEnemyDeath?.Invoke(character);
    }

    public static void RaiseHeroCharacterMove(Character character)
    {
        OnHeroCharacterMove?.Invoke(character);
    }
}
