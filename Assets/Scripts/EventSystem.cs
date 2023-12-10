using System;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    // list of game events 
    public static Action<Character> OnHeroDeath;
    public static Action<Character> OnEnemyDeath;

    public static Action<Character> OnHeroCharacterMove;

    public static Action<GameState> OnGameStateChange;

    // invokes an event for the death of a hero
    public static void RaiseHeroDeath(Character character) 
    {
        OnHeroDeath?.Invoke(character);
    }

    // invokes an event for the death of an enemy (ai)
    public static void RaiseEnemyDeath(Character character)
    {
        OnEnemyDeath?.Invoke(character);
    }

    // invokes an event for the movement of a hero character (can calcualte pathfinding for AI characters which are listening for a certain character movement)
    public static void RaiseHeroCharacterMove(Character character)
    {
        OnHeroCharacterMove?.Invoke(character);
    }

    // invokes an event for the change of the game state
    public static void RaiseGameStateChange(GameState gameState)
    {
        OnGameStateChange?.Invoke(gameState);
    }
}
