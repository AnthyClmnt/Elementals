using System;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    public GameObject[] cardPrefabs;
    public GameObject[] slots;

    public int handSize;

    public Canvas canvas;

    public List<Card> userHand = new();
    public List<Card> aiHand = new();

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

    private int RandomGaussianRange()
    {
        float u1 = UnityEngine.Random.value;
        float u2 = UnityEngine.Random.value;

        float z0 = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);

        return Mathf.Clamp(Mathf.RoundToInt(4 + z0 * 1.5f), 2, 8);
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
        return new(cardType, card, GenerateWeightedStrength(15, 30), GenerateWeightedStrength(50, 75), RandomGaussianRange());
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

    public void PlayCard(Card card)
    {
        userHand.Remove(card);

        card.card.selected = false;
        Destroy(card.card.gameObject);
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
