using System;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    public GameObject[] cardPrefabs;
    public GameObject[] characterPrefabs;
    public GameObject[] slots;
    public GameObject characterContainer;

    public int handSize;

    public Canvas canvas;

    public List<Card> userHand = new();
    public List<Card> aiHand = new();

    [SerializeField] private int minRange, maxRange;
    [SerializeField] private int minAttack, maxAttack;
    [SerializeField] private int minHealth, maxHealth;

    private GameDifficulty gameDifficulty;
    [SerializeField] private float easyMultiplier;
    [SerializeField] private float hardMultiplier;

    private void Awake()
    {
        Instance = this;

        gameDifficulty = GameManager.Instance.gameDifficulty;
    }

    public void GenerateHands()
    {
        // generates the user and Ai's hand before starting the game (giving turn to hero)
        GenerateHand(userHand, true); 
        GenerateHand(aiHand);

        EventSystem.RaiseGameStateChange(GameState.HeroesTurn); // moving on game state and begin the game  
    }

    // uses Gaussian distribution to calculate the range of the card
    private int RandomGaussianRange(int min, int max)
    {
        min = Mathf.Max(min, 1);

        float u1 = UnityEngine.Random.value;
        float u2 = UnityEngine.Random.value;

        float z0 = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);

        return Mathf.Clamp(Mathf.RoundToInt(4 + z0 * 1.5f), min, max);
    }

    // Generates random strenght for either health or attack damange
    private int GenerateStrength(int min, int max)
    {
        return UnityEngine.Random.Range(min , max + 1);
    }

    private void GenerateHand(List<Card> hand, bool userHand = false)
    {
        for (int i = 0; i < handSize; i++) // loops through until hand is of size handSize
        {
            CardType randomCardType = (CardType) UnityEngine.Random.Range(0, Enum.GetValues(typeof(CardType)).Length); // randomly choses between water, fire and earth

            if (userHand) // users hand need to be actually shown in the interface therefor bool: userHand controls whether to do this
            {
                PlayingCard playingCard = InstantiatePlayingCard(randomCardType, i); // create the playingCard gameObject

                Card card = CreateCard(randomCardType, playingCard, true); // create the card 
                hand.Add(card); // adds card to the hero's hand

                //sets the text on the playing card to the attributes
                playingCard.SetAttack(card.attack.ToString());
                playingCard.SetHealth(card.health.ToString());
                playingCard.SetRange(card.range.ToString());
            } else
            {
                hand.Add(CreateCard(randomCardType, gameObject.AddComponent<PlayingCard>(), false)); // creates and adds the card to the AI's hand
            }
        }
    }

    // creates and returns the card 
    private Card CreateCard(CardType cardType, PlayingCard card, bool userhand)
    {
        // user hand stats dont change regardless of difficulty and AI's stats remain default on medium difficulty
        if (userhand || gameDifficulty == GameDifficulty.Medium)
        {
            return new(cardType, card, GenerateStrength(minAttack, maxAttack), 
                GenerateStrength(minHealth, maxHealth), 
                RandomGaussianRange(minRange, maxRange));
        }

        if (gameDifficulty == GameDifficulty.Easy) // if easy, use the easy multiplier value for the attributes
        {
            return new(cardType, card, 
                GenerateStrength((int)(minAttack * easyMultiplier), (int)(maxAttack * easyMultiplier)), 
                GenerateStrength((int)(minHealth * easyMultiplier), (int)(maxHealth * easyMultiplier)), 
                RandomGaussianRange((int)(minRange * easyMultiplier), (int)(maxRange * easyMultiplier)));
        }

        else // must be hard difficulty, use the hard multiplier value for the attributes
        {
            return new(cardType, card, 
                GenerateStrength((int)(minAttack * hardMultiplier), (int)(maxAttack * hardMultiplier)), 
                GenerateStrength((int)(minHealth * hardMultiplier), (int)(maxHealth * hardMultiplier)), 
                RandomGaussianRange((int)(minRange * hardMultiplier), (int)(maxRange * hardMultiplier)));
        }
    }

    // creates and returns the instantiated playing card
    private PlayingCard InstantiatePlayingCard(CardType cardType, int index)
    {
        PlayingCard playingCard = Instantiate(cardPrefabs[(int)cardType]).GetComponent<PlayingCard>(); // using enum value will create the appropiate card type

        playingCard.transform.SetParent(canvas.transform, false); // set parent of the card as the playingCard
        playingCard.name = $"user: {cardType}"; // give it its name

        playingCard.transform.position = slots[index].transform.position; // moves card into slot (pre-defined and index of slot provided)

        return playingCard;
    }

    // calculates and returns which card is selected (possibly null)
    public Card? GetSelectedCard()
    {
        foreach(Card card in userHand)
        {
            if (card.card.selected)
            {
                return card;
            }
        }

        return null;
    }

    // Spawn in the Character gameObject (both hero and ememy (AI))
    public Character SpawnInCharacter(TileData tile, Card card, bool isHandAi = false)
    {
        Character character = Instantiate(characterPrefabs[(int)card.cardType]).GetComponent<Character>(); // creates character gameObject
        character.transform.position = new Vector3(tile.gridLocation.x, tile.gridLocation.y, -2f); // moves it to the chosen spawn position

        character.standingOnTile = tile; // sets character as standing on this tile
        character.type = isHandAi ? MobType.Enemy : MobType.Hero; // sets which type the character is
        character.characterCard = card;
        character.spawnTile = tile;

        var style = GetPlayStyle(card, !isHandAi); // gets the playStyle for the character (only used for AI)
        character.style = style;

        character.transform.SetParent(characterContainer.transform); 
        character.name = character.characterCard.name;

        GridManager.Instance.SetCharacterOnTile(character, tile.gridLocation); // sets tileData to know this character is standing on this tile
        PlayCard(card, isHandAi); // plays card (disgard from hand)

        return character;
    }

    // checks attrivute value is above given threshold 
    private bool ThresholdCheck(int value, int maxValue)
    {
        return value > maxValue * .51;
    }

    // Calculates and returns the playStyle of the Character
    private PlayStyle GetPlayStyle(Card card, bool userCard)
    {
        Attribute bestAttribute;
        if (userCard || gameDifficulty == GameDifficulty.Medium)
        {
            bestAttribute = card.GetBestAttribute(maxAttack, maxRange, maxHealth); // gets the best attribute of the card
        } else
        {
            if (gameDifficulty == GameDifficulty.Easy)
            {
                bestAttribute = card.GetBestAttribute((int)(maxAttack * easyMultiplier), (int)(maxRange * easyMultiplier), (int)(maxHealth * easyMultiplier)); // gets the best attribute of the card
            }
            else
            {
                bestAttribute = card.GetBestAttribute((int)(maxAttack * hardMultiplier), (int)(maxRange * hardMultiplier), (int)(maxHealth * hardMultiplier)); // gets the best attribute of the card
            }
        }

        // goes through different playStyles, if a check meets the criteria this playStyle will be returned
        if (bestAttribute == Attribute.Attack && ThresholdCheck(card.attack, maxAttack))
        {
            return PlayStyle.Aggressor;
        }

        else if (bestAttribute == Attribute.Range && ThresholdCheck(card.range, maxRange))
        {
            return PlayStyle.Roamer;
        }

        else if (bestAttribute == Attribute.Health && ThresholdCheck(card.health, maxHealth))
        {
            return PlayStyle.Defender;
        }

        else if (!ThresholdCheck(card.attack, maxAttack) && !ThresholdCheck(card.range, maxRange) && ThresholdCheck(card.health, maxHealth))
        {
            return PlayStyle.Scared;
        }

        // if none are met the style is Default
        return PlayStyle.Default;
    }

    // removes the card from the given hand
    public void PlayCard(Card card, bool isHandAi = false)
    {
        if (isHandAi) 
        {
            aiHand.Remove(card);
        }
        else
        {
            userHand.Remove(card);

            card.card.selected = false; // set as not selected
            Destroy(card.card.gameObject); // destroy the card GameObject from the hand 
        }
    }

    // utility function to prevent multiple cards from being selected
    public bool CardSelectedAllowed()
    {
        foreach(Card card in userHand)
        {
            if (card.card.selected == true)
            {
                return false;
            }
        }

        return true;
    }
}
