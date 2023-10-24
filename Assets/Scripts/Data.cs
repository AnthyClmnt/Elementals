using UnityEngine;

[System.Serializable]
public struct TileData
{
    public Tile tile;
    public Vector2 position;
}

public enum GameState
{
    InitialiseGame = 0,
    HeroesTurn = 1,
    EnemiesTurn = 2
}
