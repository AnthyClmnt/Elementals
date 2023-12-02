using System.Collections.Generic;
using UnityEngine;using System;

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

public struct ScenarioState
{
    public List<Character> HeroCharacters;
    public List<Character> AiCharacters;
    public List<Card> AiCards;
    public Shrine HeroShrine;
    public Shrine AiShrine;
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

    private readonly int GetAttributePercent(int value, int maxValue)
    {
        return (int)Math.Round((double)value / maxValue * 100);
    }

    public readonly Attribute GetBestAttribute(int maxAttack, int maxRange, int maxHealth)
    {
        int attackPercentage = GetAttributePercent(attack, maxAttack);
        int rangePercentage = GetAttributePercent(range, maxRange);
        int healthPercentage = GetAttributePercent(health, maxHealth);

        int best = Mathf.Max(attackPercentage, Mathf.Max(rangePercentage, healthPercentage));

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

public enum Attribute
{
    Attack = 0,
    Range = 1,
    Health = 2,
}

public enum PlayStyle // based on best/worst attribute(s)
{
    Aggressor = 0, // good attack
    Roamer = 1, // good range
    Defender = 2, // good health
    Scared = 3, // poor everything 
    Default = 4, // balanced card
}

public enum GameState
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
