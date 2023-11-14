using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    public static AiManager Instance;

    private Pathfinding pathfinding;
    private RangeFinding rangeFinding;

    private List<TileData> path = new();
    private Character pathCharacter;
    private Character chasingCharacter;

    private List<Character> charactersInPlay = new();
    private List<Card> hand;


    private bool isMoving = false;

    private int gridWith;
    private int gridHeight;

    private void Awake()
    {
        Instance = this;
        gridWith = GridManager.Instance.gridWidth;
        gridHeight = GridManager.Instance.gridHeight;
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

            if (hand.Count == 0 && charactersInPlay.Count == 0) // no available moves, end ai go
            {
                EndTurn();
                return;
            }

            else if ((charactersInPlay.Count == 0 || Random.value > .85) && !isMoving) // no characters in play, ai must spawn in character
            {
                Character character = CardManager.Instance.SpawnInCharacter(GetSpawnTile(), SelectCard(), true);
                charactersInPlay.Add(character);

                EndTurn();
            }


/*            else if (!isMoving && chasingCharacter != null) // if aggressor character if chasing player character
            {
                RoamingChasing(chasingCharacter);
            }*/

            if (!isMoving && chasingCharacter == null && path.Count == 0) // no specific moves to make, ai will decide 
            {
                Character chosenCharacter = ChooseCharacter();

                chosenCharacter.style = PlayStyle.Aggressor;

                switch (chosenCharacter.style)
                {
                    case PlayStyle.Aggressor:
                        AggressorMove(chosenCharacter);
                        break;

                    case PlayStyle.Roamer:
                        //RoamerMove(chosenCharacter);
                        EndTurn();
                        break;

                    case PlayStyle.Defender:
                        break;

                    case PlayStyle.Scared:
                        break;

                    case PlayStyle.Default:
                        break;
                }
            }

            if (path.Count > 0) // is character still moving along path, keep updating and running until path reached
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

    // Path moving code

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

    // End

    // moves code 

    /*private void RoamerMove(Character chosenCharacter)
    {
        bool moveExecuted = RoamingChasing(chosenCharacter, false);
        if (moveExecuted)
        {
            return;
        }

        AttemptShrineAttack(chosenCharacter);
    }*/

    // End
    
    // Aggressor code 

    private void AggressorMove(Character aggressor)
    {
        var rangeTiles = rangeFinding.GetRangeTiles(aggressor.standingOnTile, aggressor.characterCard.range, aggressor.characterCard.cardType);
        TileData shrineLocation = GridManager.Instance.heroShineTileData;

        if (rangeTiles.Contains(GridManager.Instance.heroShineTileData))
        {
            Character characterToAttack = null;
            foreach (TileData tile in rangeTiles)
            {
                if (tile.character && tile.character.type == MobType.Hero)
                {
                    characterToAttack = tile.character;
                    break;
                }
            }

            if (characterToAttack != null)
            {
                AttackPlayerCharacter(characterToAttack, aggressor);
                EndTurn();
            }

            else
            {
                ShrineAttack(shrineLocation, aggressor.characterCard.attack);
                EndTurn();
            }
        }
        
        else
        {
            pathCharacter = aggressor;
            path = pathfinding.FindPath(aggressor.standingOnTile, shrineLocation, aggressor.characterCard.cardType, aggressor.characterCard.range);
        }
    }

    // End

    // Roaming code

    private bool RoamingChasing(Character roamer, bool chasing = true)
    {
        Character closestCharacter = FindClosestCharacter(roamer);

        if (closestCharacter != null)
        {
            chasingCharacter = roamer;

            var rangeTiles = rangeFinding.GetRangeTiles(roamer.standingOnTile, roamer.characterCard.range, roamer.characterCard.cardType);

            Character characterToAttack = null;
            foreach (TileData tile in rangeTiles)
            {
                if (tile.character == closestCharacter && tile.character.type == MobType.Hero)
                {
                    characterToAttack = tile.character;
                    break;
                }
            }

            if (characterToAttack != null)
            {
                AttackPlayerCharacter(closestCharacter, roamer);
                return true;
            }
            else
            {
                pathCharacter = roamer;
                path = pathfinding.FindPath(roamer.standingOnTile, closestCharacter.standingOnTile, roamer.characterCard.cardType, roamer.characterCard.range);
                return false;
            }
        }
        else
        {
            if (chasing)
            {
                chasingCharacter = null;
                return false;
            }
            else
            {
                return false;
            }
        }
    }

    private Character FindClosestCharacter(Character character)
    {
        List<Character> playerCharacters = InputManager.Instance.charactersInPlay;

        Character closestCharacter = new();
        int closestDistance = 9999;

        foreach (Character chara in playerCharacters)
        {
            int distance = pathfinding.FindPath(character.standingOnTile, chara.standingOnTile, character.characterCard.cardType).Count;
            if (distance < closestDistance)
            {
                closestCharacter = chara;
                closestDistance = distance;
            }
        }

        return closestCharacter;
    }

    private void AttackPlayerCharacter(Character victim, Character attacker)
    {
        victim.TakeDamage(attacker.characterCard.attack);
        EndTurn();
    }

    // End

    // Selection code

    private Character ChooseCharacter()
    {
        return charactersInPlay[0];
    }

    private Card SelectCard()
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

    // End

    // Misc Functions

    private bool AttemptShrineAttack(Character attacker)
    {
        var rangeTiles = rangeFinding.GetRangeTiles(attacker.standingOnTile, attacker.characterCard.range, attacker.characterCard.cardType);
        var shrineDestination = GridManager.Instance.heroShineTileData;

        if (rangeTiles.Contains(shrineDestination))
        {
            ShrineAttack(shrineDestination, attacker.characterCard.attack);
            return true;
        } 

        else
        {
            path = pathfinding.FindPath(attacker.standingOnTile, shrineDestination, attacker.characterCard.cardType, attacker.characterCard.range);

            if (path[path.Count - 1].shrineLocation)
            {
                path.RemoveAt(path.Count - 1);
            }

            return true;
        }
    }

    private void ShrineAttack(TileData shrineTile, int damage)
    {
        shrineTile.shrine.TakeDamage(damage);
        EndTurn();
    }

    private bool GetCharacterInRange(Character character)
    {
        var rangeTiles = rangeFinding.GetRangeTiles(character.standingOnTile, character.characterCard.range, character.characterCard.cardType);

        Character characterToAttack = null;
        foreach (TileData tile in rangeTiles)
        {
            if (tile.character && tile.character.type == MobType.Hero)
            {
                characterToAttack = tile.character;
                break;
            }
        }

        if (characterToAttack != null)
        {
            AttackPlayerCharacter(characterToAttack, character);
            return true;
        }

        return false;
    }

    private TileData GetSpawnTile()
    {
        var tileData = GridManager.Instance.tileData;

        Vector2Int pos = new(Random.Range(gridWith - 2, gridWith - 1), Random.Range(0, gridHeight - 1));

        if (tileData[pos].character != null)
        {
            GetSpawnTile();
        }

        return tileData[pos];
    }

    public void CharacterKilled(Character character)
    {
        if(charactersInPlay.Contains(character))
        {
            charactersInPlay.Remove(character);
        }
    }

    private void EndTurn()
    {
        GameManager.Instance.ChangeGameState(GameState.HeroesTurn);
    }
}
    