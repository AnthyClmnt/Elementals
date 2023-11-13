using UnityEngine;

public struct TileData
{
    public Tile tile;
    public Vector2Int gridLocation;
    public bool walkable;
    public bool shrineLocation;
    public Character character;
    public Shrine shrine;
}

public struct ShrineData
{
    public Tile tile;
    public MobType shrineType;
    public int health;
    public int currHealth;

    public ShrineData(Tile tile, MobType shrineType)
    {
        this.tile = tile;
        this.shrineType = shrineType;
        health = 200;
        currHealth = 200;
    } 
}

public struct Card
{
    public CardType cardType;
    public PlayingCard card;
    public string name;
    public string description;

    public int attack;
    public int health;
    public int currHealth;
    public int range;

    public Card(CardType cardType, PlayingCard card, int attack, int health, int range)
    {
        this.cardType = cardType;
        this.card = card;
        this.attack = attack;
        this.health = health;
        currHealth = health;
        this.range = range;


        switch (cardType)
        {
            case CardType.Fire:
                this.name = "FireBoi";
                this.description = "Fireboi is very hot";
                break;

            case CardType.Water:
                this.name = "WaterBoi";
                this.description = "WaterBoi is very cool";
                break;

            default:
                this.name = "EarthBoi";
                this.description = "EarthBoi is really boring";
                break;
        }
    }
}

public enum MobType
{
    Hero = 0,
    Enemy = 1
}

public enum GameState
{
    InitialiseGrid = 0,
    InitialiseCards = 1,
    HeroesTurn = 2,
    EnemiesTurn = 3,
    HeroWin = 4,
    EnemyWin = 5,
}

public enum TileType
{
    Grass = 0,
    Rock = 1,
    Water = 2
}

public enum CardType
{
    Fire = 0,
    Water = 1,
    Earth = 2
}
