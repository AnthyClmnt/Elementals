using System.Collections.Generic;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    public static AiManager Instance;

    private Pathfinding pathfinding;
    private RangeFinding rangeFinding;

    private List<TileData> path = new();
    private Character pathCharacter;

    private List<Character> characterInPlay = new();
    private List<Card> hand;

    private bool isMoving = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        pathfinding = new Pathfinding();
        rangeFinding =  new RangeFinding();
    }

    private void LateUpdate()
    {
        if(GameManager.Instance.GameState == GameState.EnemiesTurn)
        {
            hand = CardManager.Instance.aiHand;

            if(hand.Count == 0 && characterInPlay.Count == 0)
            {
                EndTurn();
                return;
            }
            
            if (characterInPlay.Count == 0 && !isMoving) 
            {
                var tileData = GridManager.Instance.tileData;

                Character character = CardManager.Instance.SpawnInCharacter(tileData[new Vector2Int(18, 2)], SelectCharacter(), true);
                characterInPlay.Add(character);

                EndTurn();
            }
            else if (!isMoving && characterInPlay.Count > 0)
            {
                var destination = GridManager.Instance.heroShineTileData;
                var nullableCharacterTile = GridManager.Instance.GetTileData(characterInPlay[0].transform.position);

                if (nullableCharacterTile.HasValue)
                {
                    TileData characterTile = nullableCharacterTile.Value;

                    var rangeTiles = rangeFinding.GetRangeTiles(characterTile, characterTile.character.characterCard.range, characterTile.character.characterCard.cardType);

                    Character characterToAttack = null;
                    foreach (TileData tile in rangeTiles)
                    {
                        if (tile.character && tile.character.type == MobType.Hero)
                        {
                            characterToAttack = tile.character;
                            break;
                        }
                    }

                    if (rangeTiles.Contains(GridManager.Instance.heroShineTileData))
                    {
                        destination.shrine.TakeDamage(characterTile.character.characterCard.attack);
                        EndTurn();
                    }

                    else if (characterToAttack != null)
                    {
                        characterToAttack.TakeDamage(characterTile.character.characterCard.attack);
                        EndTurn();
                    }

                    else if (characterTile.gridLocation != destination.gridLocation)
                    {
                        path = pathfinding.FindPath(characterTile, destination, characterTile.character.characterCard.cardType);

                        if (path[path.Count - 1].shrineLocation)
                        {
                            path.RemoveAt(path.Count - 1);
                        }

                        if (path.Count > 0)
                        {
                            GridManager.Instance.RemoveCharacterFromTile(characterTile.gridLocation);

                            if (path.Count > characterTile.character.characterCard.range)
                            {
                                path.RemoveRange(characterTile.character.characterCard.range, path.Count - characterTile.character.characterCard.range);

                                if (path[path.Count - 1].character != null)
                                {
                                    path.RemoveAt(path.Count - 1);
                                }
                            }
                            pathCharacter = characterTile.character;
                        }
                    }
                }
            }

            if (path.Count > 0)
            {
                isMoving = true;
                MoveAlongPath();
            } 
            else
            {
                EndTurn();
            }
        }
    }

    private void MoveAlongPath()
    {
        var step = 4f * Time.deltaTime;

        pathCharacter.transform.position = Vector2.MoveTowards(pathCharacter.transform.position, path[0].gridLocation, step);
        pathCharacter.transform.position = new Vector3(pathCharacter.transform.position.x, pathCharacter.transform.position.y, -2f);

        if (Vector2.Distance(pathCharacter.transform.position, path[0].gridLocation) < 0.00001f)
        {
            PositionCharacterOnLine(path[0]);
            path.RemoveAt(0);
        }

        if (path.Count == 1)
        {
            GridManager.Instance.SetCharacterOnTile(pathCharacter, path[0].gridLocation);
        }

        if (path.Count == 0)
        {
            isMoving = false;
            EndTurn();
        }
    }

    private void PositionCharacterOnLine(TileData tile)
    {
        pathCharacter.transform.position = new Vector3(tile.gridLocation.x, tile.gridLocation.y + 0.0001f, pathCharacter.transform.position.z);
        pathCharacter.GetComponent<SpriteRenderer>().sortingOrder = tile.tile.GetComponent<SpriteRenderer>().sortingOrder;
        pathCharacter.standingOnTile = tile;
    }

    private Card SelectCharacter()
    {
        List<Card> cards = CardManager.Instance.aiHand;
        
        Card selectedCard = cards[0];
        int maxpoints = 0;

        foreach (Card card in cards)
        {
            int points = card.health + (card.range * 10) + card.attack;
            if(points > maxpoints)
            {
                maxpoints = points;
                selectedCard = card;
            }
        }

        return selectedCard;
    }

    public void CharacterKilled(Character character)
    {
        if(characterInPlay.Contains(character))
        {
            hand.Remove(character.characterCard);
            characterInPlay.Remove(character);
        }
    }

    private void EndTurn()
    {
        GameManager.Instance.ChangeGameState(GameState.HeroesTurn);
    }
}
    