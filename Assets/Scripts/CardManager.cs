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

    private void Awake()
    {
        Instance = this;
    }

    public void GenerateHands()
    {
        GenerateHand(userHand, true);
        GenerateHand(aiHand);
        GameManager.Instance.ChangeGameState(GameState.HeroesTurn);
    }

    private int RandomGaussianRange(int min, int max)
    {
        float u1 = UnityEngine.Random.value;
        float u2 = UnityEngine.Random.value;

        float z0 = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);

        return Mathf.Clamp(Mathf.RoundToInt(4 + z0 * 1.5f), min, max);
    }

    private int GenerateWeightedStrength(int min, int max)
    {
        return UnityEngine.Random.Range(min , max + 1);
    }

    void GenerateHand(List<Card> hand, bool userHand = false)
    {
        for (int i = 0; i < handSize; i++)
        {
            CardType randomCardType = (CardType) UnityEngine.Random.Range(0, Enum.GetValues(typeof(CardType)).Length);

            if (userHand)
            {
                PlayingCard playingCard = InstantiatePlayingCard(randomCardType, i);

                Card card = CreateCard(randomCardType, playingCard);
                hand.Add(card);

                playingCard.SetAttack(card.attack.ToString());
                playingCard.SetHealth(card.health.ToString());
                playingCard.SetRange(card.range.ToString());
            } else
            {
                hand.Add(CreateCard(randomCardType, gameObject.AddComponent<PlayingCard>()));
            }
        }
    }

    private Card CreateCard(CardType cardType, PlayingCard card)
    {
        return new(cardType, card, GenerateWeightedStrength(minAttack, maxAttack), GenerateWeightedStrength(minHealth, maxHealth), RandomGaussianRange(minRange, maxRange));
    }

    private PlayingCard InstantiatePlayingCard(CardType cardType, int index)
    {
        PlayingCard playingCard = Instantiate(cardPrefabs[(int)cardType]).GetComponent<PlayingCard>();

        playingCard.transform.SetParent(canvas.transform, false);
        playingCard.name = $"user: {cardType}";

        playingCard.transform.position = slots[index].transform.position;

        return playingCard;
    }

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

    public Character SpawnInCharacter(TileData tile, Card card, bool isHandAi = false)
    {
        Character character = Instantiate(characterPrefabs[(int)card.cardType]).GetComponent<Character>();
        character.transform.position = new Vector3(tile.gridLocation.x, tile.gridLocation.y, -2f);

        character.standingOnTile = tile;
        character.type = isHandAi ? MobType.Enemy : MobType.Hero;
        character.characterCard = card;
        character.spawnTile = tile;

        var style = GetPlayStyle(card);

        character.style = style;

        character.transform.SetParent(characterContainer.transform);
        character.name = character.characterCard.name;

        GridManager.Instance.SetCharacterOnTile(character, tile.gridLocation);
        PlayCard(card, isHandAi);

        return character;
    }

    private bool ThresholdCheck(int value, int maxValue)
    {
        return value > maxValue * .51;
    }

    private PlayStyle GetPlayStyle(Card card)
    {
        Attribute bestAttribute = card.GetBestAttribute(maxAttack, maxRange, maxHealth);

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

        return PlayStyle.Default;
    }

    public void PlayCard(Card card, bool isHandAi = false)
    {
        if (isHandAi)
        {
            aiHand.Remove(card);
        }
        else
        {
            userHand.Remove(card);

            card.card.selected = false;
            Destroy(card.card.gameObject);
        }
    }

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
