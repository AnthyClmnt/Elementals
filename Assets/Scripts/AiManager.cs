using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class AiManager : MonoBehaviour
{
    public static AiManager Instance;

    private int gridWith;
    private int gridHeight;

    private Pathfinding pathfinding;
    private RangeFinding rangeFinding;

    private List<Character> charactersInPlay = new();

    private List<Card> hand = new();

    private Character pathCharacter;
    private List<TileData> path = new();

    private Character roamerChasing; 

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

                if (roamerChasing != null)
                {
                    RoamerMove(roamerChasing);
                }

                else if (hand.Count == 0 && charactersInPlay.Count == 0) // no available moves, end ai go
                {
                    EndTurn();
                    return;
                }

                else if (charactersInPlay.Count == 0 && hand.Count > 0) // no characters in play, ai must spawn in character CHANGE TO CORRECT VALUE
                {
                    SpawnCharacter();
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

        if (chosenCharacter != null)
        {
            Debug.Log(chosenCharacter.style);

            switch (chosenCharacter.style)
            {
                case PlayStyle.Aggressor:
                    AggressorMove(chosenCharacter);
                    break;

                case PlayStyle.Roamer:
                    RoamerMove(chosenCharacter);
                    break;

                case PlayStyle.Defender:
                    DefenderMove(chosenCharacter);
                    break;

                case PlayStyle.Scared:
                    ScaredMove(chosenCharacter, charactersInPlay.Count == 1 && hand.Count == 0);
                    break;

                case PlayStyle.Default:
                    DefaultMove(chosenCharacter);
                    break;
            }
        } else
        {
            Debug.Log("character returned was null");
            EndTurn();
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
                if (tile.character != null && tile.character.type == MobType.Hero)
                {
                    if (characterToAttack == null)
                    {
                        characterToAttack = tile.character;
                    }
                    else if (tile.character.characterCard.currHealth < characterToAttack.characterCard.currHealth) 
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

    private void RoamerMove(Character roamer)
    {
        Character cloestCharacter = FindClosestCharacter(roamer);
        var rangeTiles = rangeFinding.GetRangeTiles(roamer.standingOnTile, roamer.characterCard.range, roamer.characterCard.cardType, true);

        if (roamerChasing && roamerChasing != cloestCharacter)
        {
            roamerChasing = null;
        }

        if(cloestCharacter == null)
        {
            var shrineLocation = GridManager.Instance.heroShineTileData;
            if (rangeTiles.Contains(shrineLocation))
            {
                ShrineAttack(shrineLocation, roamer.characterCard.attack);
                EndTurn();
            }
            else
            {
                GetValidPath(roamer.standingOnTile, shrineLocation, roamer, true);
            }
        }

        else if (rangeTiles.Contains(GridManager.Instance.GetTileData(cloestCharacter.standingOnTile.gridLocation).Value))
        {
            CharacterAttack(cloestCharacter, roamer);
            EndTurn();
        }

        else
        {
            GetValidPath(roamer.standingOnTile, cloestCharacter.standingOnTile, roamer, true);
        }
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
            Debug.Log("no move for defender");
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

    private void DefaultMove(Character balanced)
    {
        var rangeTiles = rangeFinding.GetRangeTiles(balanced.standingOnTile, balanced.characterCard.range, balanced.characterCard.cardType, true);

        Character closestInRange = null;
        int smallestDistance = 999;
        foreach (var tile in rangeTiles)
        {
            if (tile.character != null)
            {
                var path = pathfinding.FindPath(balanced.standingOnTile, tile, balanced.characterCard.cardType, balanced.characterCard.range);

                if (path.Count < smallestDistance)
                {
                    smallestDistance = path.Count;
                    closestInRange = tile.character;
                }
            }
        }

        if (closestInRange != null)
        {
            CharacterAttack(closestInRange, balanced);
            EndTurn();
        }

        else
        {
            var shrineLocation = GridManager.Instance.heroShineTileData;
            if (rangeTiles.Contains(shrineLocation))
            {
                ShrineAttack(shrineLocation, balanced.characterCard.attack);
                EndTurn();
            }
            else
            {
                GetValidPath(balanced.standingOnTile, shrineLocation, balanced, true);
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
    
        if (path.Count > 0)
        {
            if (attackDestination)
            {
                path.RemoveAt(path.Count - 1);
            }

            for (int i = path.Count - 1; i >= 0; i--)
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

            pathCharacter = character;
        }

        this.path = path;
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

    private List<Character> FindEnemyWithinShrineRange()
    {
        List<Character> playerCharacters = InputManager.Instance.charactersInPlay;
        TileData shrineTileData = GridManager.Instance.enemyShineTileData;

        List<Character> attackShrineCharacters = new();

        foreach (Character chara in playerCharacters)
        {
            int distance = pathfinding.FindPath(chara.standingOnTile, shrineTileData, chara.characterCard.cardType, 0).Count;
            if (distance <= chara.characterCard.range)
            {
                attackShrineCharacters.Add(chara);
            }
        }

        if (attackShrineCharacters.Count <= 1)
        {
            return attackShrineCharacters;
        }

        int lowestHealth = 999;
        foreach (Character attackCharacter in attackShrineCharacters)
        {
            if (attackCharacter.characterCard.currHealth < lowestHealth)
            {
                lowestHealth = attackCharacter.characterCard.currHealth;
            } else
            {
                attackShrineCharacters.Remove(attackCharacter);
            }
        }

        return attackShrineCharacters;
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

        bool validTileFound = false;
        TileData validTile = new();
        while (!validTileFound)
        {
            Vector2Int pos = new(UnityEngine.Random.Range(gridWith - 2, gridWith - 1), UnityEngine.Random.Range(0, gridHeight - 1));

            if (tileData[pos].character == null)
            {
                validTileFound = true;
                validTile = tileData[pos];
            }
        }

        return validTile;
    }

    private Character ChooseCharacter()
    {
        List<Character> heroesInShrineRange = FindEnemyWithinShrineRange();
        if (heroesInShrineRange.Count > 0)
        {
            Debug.Log("oops");
            return charactersInPlay[0];
            // should be defender move, use defender if one exists, otherwise use character in play with greatest x value
        }

        if (UnityEngine.Random.value < .2 && hand.Count > 0)
        {
            SpawnCharacter();
            return null;
        }

        else
        {
            List<Character> preferredCharacters = new();
            foreach (Character character in charactersInPlay)
            {
                if (character.style == PlayStyle.Aggressor || character.style == PlayStyle.Roamer || character.style == PlayStyle.Default)
                {
                    preferredCharacters.Add(character);
                }
            }

            if (preferredCharacters.Count == 0)
            {
                if (hand.Count != 0)
                {
                    SpawnCharacter();
                    return null;
                }
                
                return charactersInPlay[UnityEngine.Random.Range(0, preferredCharacters.Count - 1)];
            }
            else
            {
                return preferredCharacters[UnityEngine.Random.Range(0, preferredCharacters.Count - 1)];
            }
        }
    }

    private void SpawnCharacter()
    {
        Character character = CardManager.Instance.SpawnInCharacter(GetSpawnTile(), SelectCard(), true);
        charactersInPlay.Add(character);

        EndTurn();
    }

    private Card SelectCard()
    {
        Card selectedCard = hand[0];
        int maxpoints = 0;

        foreach (Card card in hand)
        {
            int points = card.health + (card.range * 10) + (card.attack * 2);
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
