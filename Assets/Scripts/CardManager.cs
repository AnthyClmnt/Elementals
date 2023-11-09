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

    private int GenerateRandomRange()
    {
        float randomValue = UnityEngine.Random.value;
        float bias = Mathf.Pow(randomValue, 1);

        return Mathf.RoundToInt(Mathf.Lerp(1, 10, bias));
    }

    private int GenerateWeightedStrength()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        return Mathf.FloorToInt(Mathf.Pow(randomValue, 0.5f) * 98) + 1;
    }

    void GenerateHand(List<Card> hand, bool userHand = false)
    {
        for (int i = 0; i < handSize; i++)
        {
            CardType randomCardType = (CardType) UnityEngine.Random.Range(0, Enum.GetValues(typeof(CardType)).Length);

            if (userHand)
            {
                PlayingCard playingCard = InstantiatePlayingCard(randomCardType, i);

                Card card = new(randomCardType, playingCard, GenerateWeightedStrength(), GenerateWeightedStrength(), GenerateRandomRange());
                hand.Add(card);

                playingCard.SetAttack(card.attack.ToString());
                playingCard.SetHealth(card.health.ToString());
                playingCard.SetRange(card.range.ToString());
            } else
            {
                Card card = new(randomCardType, gameObject.AddComponent<PlayingCard>(), GenerateWeightedStrength(), GenerateWeightedStrength(), GenerateRandomRange());
                hand.Add(card);
            }
        }
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
