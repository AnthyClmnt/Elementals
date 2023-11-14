using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AiManager2 : MonoBehaviour
{
    public static AiManager2 Instance;

    private int gridWith;
    private int gridHeight;

    private Pathfinding pathfinding;
    private RangeFinding rangeFinding;

    private List<Character> charactersInPlay = new();

    private List<Card> hand = new();

    private Character pathCharacter;
    private List<TileData> path = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        pathfinding = new Pathfinding();
        rangeFinding = new RangeFinding();

        gridWith = GridManager.Instance.gridWidth;
        gridHeight = GridManager.Instance.gridHeight;
    }

    private void LateUpdate()
    {
        if (GameManager.Instance.GameState == GameState.EnemiesTurn)
        {
            if (path.Count == 0)
            {
                hand = CardManager.Instance.aiHand;

                if (hand.Count == 0 && charactersInPlay.Count == 0) // no available moves, end ai go
                {
                    EndTurn();
                    return;
                }

                else if ((charactersInPlay.Count <= 1 || Random.value > 85) && hand.Count > 0) // no characters in play, ai must spawn in character CHANGE TO CORRECT VALUE
                {
                    Character character = CardManager.Instance.SpawnInCharacter(GetSpawnTile(), SelectCard(), true);
                    charactersInPlay.Add(character);

                    EndTurn();
                }

                else
                {
                    CalculateMove();
                }
            }

            else if(path.Count > 0)
            {
                MoveAlongPath();
            }
        }
    }

    private void CalculateMove()
    {
        Character chosenCharacter = ChooseCharacter();

        switch (chosenCharacter.style)
        {
            case PlayStyle.Aggressor:
                AggressorMove(chosenCharacter);
                break;

            case PlayStyle.Roamer:
                RoamerMove();
                break;

            case PlayStyle.Defender:
                DefenderMove(chosenCharacter);
                break;

            case PlayStyle.Scared:
                ScaredMove(chosenCharacter, charactersInPlay.Count == 1 && hand.Count == 0);
                break;

            case PlayStyle.Default:
                break;
        }
    }

    private void AggressorMove(Character aggressor)
    {
        var rangeTiles = rangeFinding.GetRangeTiles(aggressor.standingOnTile, aggressor.characterCard.range, aggressor.characterCard.cardType, true);
        TileData shrineLocation = GridManager.Instance.heroShineTileData;

        if (rangeTiles.Contains(GridManager.Instance.heroShineTileData))
        {
            Character characterToAttack = null;
            foreach (TileData tile in rangeTiles)
            {
                if (tile.character && tile.character.type == MobType.Hero)
                {
                    if (tile.character.characterCard.currHealth < characterToAttack.characterCard.currHealth)
                    {
                        characterToAttack = tile.character;
                    }
                }
            }

            if (characterToAttack != null)
            {
                CharacterAttack(characterToAttack, aggressor);
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
            GetValidPath(aggressor.standingOnTile, shrineLocation, aggressor, true);
        }
    }

    private void RoamerMove()
    {
        // calc roamer move;
        EndTurn();
    }

    private void DefenderMove(Character defender) 
    {
        var enemyCharacterTiles = rangeFinding.GetDefenderTiles();

        if (enemyCharacterTiles.Count != 0)
        {
            var rangeTiles = rangeFinding.GetRangeTiles(defender.standingOnTile, defender.characterCard.range, defender.characterCard.cardType, true);

            Character characterToAttack = GetWeakestCharacter(enemyCharacterTiles);

            if (rangeTiles.Contains(GridManager.Instance.GetTileData(characterToAttack.standingOnTile.gridLocation).Value))
            {
                CharacterAttack(characterToAttack, defender);
                EndTurn();
            }
            else
            {
                GetValidPath(defender.standingOnTile, characterToAttack.standingOnTile, defender, true);
            }
        } 

        else
        {
            EndTurn();
        }
    }

    private void ScaredMove(Character scared, bool forceAction)
    {
        if (forceAction)
        {
            DefenderMove(scared);
        } 
        
        else
        {
            Character closestCharacter = FindClosestCharacter(scared);
            var rangeTiles = rangeFinding.GetRangeTiles(closestCharacter.standingOnTile, closestCharacter.characterCard.range, closestCharacter.characterCard.cardType, false);

            TileData scaredTile = GridManager.Instance.GetTileData(scared.standingOnTile.gridLocation).Value;

            if (rangeTiles.Contains(scaredTile))
            {
                RunAway(scaredTile);
            } else
            {
                EndTurn();
            }
        }
    }

    private void CharacterAttack(Character victim, Character attacker)
    {
        victim.TakeDamage(attacker.characterCard.attack);
    }

    private void ShrineAttack(TileData shrineTile, int damage)
    {
        shrineTile.shrine.TakeDamage(damage);
    }

    private void GetValidPath(TileData start, TileData destination, Character character, bool attackDestination = false)
    {
        var path = pathfinding.FindPath(start, destination, character.characterCard.cardType, character.characterCard.range);

        if (attackDestination)
        {
            path.RemoveAt(path.Count - 1);
        }

        for(int i = path.Count - 1; i >= 0; i--)
        {
            if (path[i].character == null)
            {
                break;
            } 
            else
            {
                path.RemoveAt(i);
            }
        }

        this.path = path;
        pathCharacter = character;
    }

    private void RunAway(TileData scaredTile)
    {
        Vector2Int topCorner = new(gridWith - 1, gridHeight - 1);
        Vector2Int bottomCorner = new(gridWith - 1, 0);

        var topCornerPath = pathfinding.FindPath(scaredTile, GridManager.Instance.GetTileData(topCorner).Value, scaredTile.character.characterCard.cardType, scaredTile.character.characterCard.range);
        var bottomCornerPath = pathfinding.FindPath(scaredTile, GridManager.Instance.GetTileData(bottomCorner).Value, scaredTile.character.characterCard.cardType, scaredTile.character.characterCard.range);

        if (topCornerPath.Count < bottomCornerPath.Count)
        {
            GetValidPath(scaredTile, GridManager.Instance.GetTileData(bottomCorner).Value, scaredTile.character);
        } 
        else
        {
            GetValidPath(scaredTile, GridManager.Instance.GetTileData(topCorner).Value, scaredTile.character);
        }
    }

    private Character FindClosestCharacter(Character character)
    {
        List<Character> playerCharacters = InputManager.Instance.charactersInPlay;

        Character? closestCharacter = null;
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

        return closestCharacter == null ? null : closestCharacter;
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
            EndTurn();
        }
    }

    private Character GetWeakestCharacter(List<TileData> charactersTileData)
    {
        if (charactersTileData.Count == 0)
        {
            return new();
        }

        if (charactersTileData.Count == 1)
        {
            return charactersTileData[0].character;
        }

        Character selected = null;
        int weakest = 9999;
        foreach (TileData tileData in charactersTileData)
        {
            if (tileData.character.characterCard.currHealth < weakest)
            {
                selected = tileData.character;
                weakest = tileData.character.characterCard.currHealth;
            }
        }

        return selected;
    }

    private void PositionCharacterOnLine(TileData tile)
    {
        pathCharacter.transform.position = new Vector3(tile.gridLocation.x, tile.gridLocation.y + 0.0001f, pathCharacter.transform.position.z);
        pathCharacter.GetComponent<SpriteRenderer>().sortingOrder = tile.tile.GetComponent<SpriteRenderer>().sortingOrder;
        pathCharacter.standingOnTile = tile;
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

    private Character ChooseCharacter()
    {
        foreach (Character character in charactersInPlay)
        {
            // check if character in range of hero character
        }
        return charactersInPlay[0];
    }

    private Card SelectCard()
    {
        Card selectedCard = hand[0];
        int maxpoints = 0;

        foreach (Card card in hand)
        {
            int points = card.health + (card.range * 10) + card.attack;
            if (points > maxpoints)
            {
                maxpoints = points;
                selectedCard = card;
            }
        }

        return selectedCard;
    }
    public void CharacterKilled(Character character)
    {
        if (charactersInPlay.Contains(character))
        {
            charactersInPlay.Remove(character);
        }
    }

    private void EndTurn()
    {
        GameManager.Instance.ChangeGameState(GameState.HeroesTurn);
    }
}
