using System.Collections.Generic;
using UnityEngine;using System;

// Data script stores public structs and enums used throughout 

public struct TileData // stores information about tiles
{
    public Tile tile; // the actual tile script which is attacted to the tile gameObject 
    public Vector2Int gridLocation; // the grid location of the tile
    public bool walkable; // if the tile is walkable (not earth or water)
    public bool shrineLocation; // if the tile is a shrine location
    public Character character; // which character is standing on the tile (null if none)
    public Shrine shrine; // the shrine script which is on the tile (null if none)
}

public struct ShrineData // information about the shrine
{
    public Tile tile; // tile script the shrine is on 
    public MobType shrineType; // is hero or enemy (AI)
    public int health; // health of the shrine
    public int currHealth; // current health of the shrine

    public ShrineData(Tile tile, MobType shrineType)
    {
        this.tile = tile;
        this.shrineType = shrineType;
        health = 200;
        currHealth = 200;
    } 
}

// stores information of the Card generated
public struct Card
{
    public CardType cardType; // fire, water or eaeth card?
    public PlayingCard card; // the within hand playing card 
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


        switch (cardType) // based on the cardType name and desciption is added 
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

    // will return the best Attribute of the card as a percentage of the maxiumum value it can be
    private readonly int GetAttributePercent(int value, int maxValue)
    {
        return (int)Math.Round((double)value / maxValue * 100);
    }

    // returs the best attribute
    public readonly Attribute GetBestAttribute(int maxAttack, int maxRange, int maxHealth)
    {
        // gets all attributes as a percentage of the maximum they can be
        int attackPercentage = GetAttributePercent(attack, maxAttack);
        int rangePercentage = GetAttributePercent(range, maxRange);
        int healthPercentage = GetAttributePercent(health, maxHealth);

        // gets the max of the three attriubtes
        int best = Mathf.Max(attackPercentage, Mathf.Max(rangePercentage, healthPercentage));

        // returns the tpye of Attribute which is best
        if (best == attackPercentage)
        {
            return Attribute.Attack;
        } 
        else if (best == healthPercentage)
        {
            return Attribute.Health;
        }
        else
        {
            return Attribute.Range;
        }
    }
}

public enum MobType
{
    Hero = 0,
    Enemy = 1
}

public enum GameDifficulty
{
    Easy = 0,
    Medium = 1,
    Hard = 2
}

public enum Attribute
{
    Attack = 0,
    Range = 1,
    Health = 2,
}

public enum PlayStyle // based on best/worst attribute(s)
{
    Roamer = 0, // good range
    Defender = 1, // good health
    Aggressor = 2, // good attack
    Default = 3, // balanced card and doesn't meet criteria for any other playStyle
    Scared = 4, // poor everything 
}

public enum GameState // controls the game state
{
    InitialiseGrid = 0,
    InitialiseCards = 1,
    HeroesTurn = 2,
    EnemiesTurn = 3,
    HeroWin = 4,
    EnemyWin = 5,
    Pause = 6,
    Resume = 7,
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
