using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    public static AiManager Instance;

    private int gridWith;
    private int gridHeight;

    private Pathfinding pathfinding;
    private RangeFinding rangeFinding;
    private Utility utility;

    private List<Character> charactersInPlay = new();
    private List<Card> hand = new();

    private Character pathCharacter;
    private List<TileData> path = new();

    private Character roamerChasing;

    private bool coroutineRunning = false;

    private Character latestHeroCharacterAttack;
    private Character latestCharacterToAttack;
    private float blockChance = .1f;

    private void Awake()
    {
        Instance = this; 
    }

    private void Start()
    {
        pathfinding = new Pathfinding();
        rangeFinding = new RangeFinding();
        utility = new Utility();

        gridWith = GridManager.Instance.gridWidth;
        gridHeight = GridManager.Instance.gridHeight;
    }

    private void OnEnable() // subscribe to any needed events and delegate them to the related methods
    {
        EventSystem.OnEnemyDeath += CharacterKilled;
        EventSystem.OnHeroCharacterMove += CharacterMoved;
    }

    private void OnDisable() // unsubscribing from all events when script becomes in-active
    {
        EventSystem.OnEnemyDeath -= CharacterKilled;
        EventSystem.OnHeroCharacterMove -= CharacterMoved;
    }

    private void LateUpdate() // Runs once per frame
    {
        if (GameManager.Instance.gameState == GameState.EnemiesTurn) // do nothing if it isn't AI's turn
        {
            if (path.Count > 0 && pathCharacter != null) // if character is moving along path continue moving along path
            {
                path = utility.MoveAlongPath(path, pathCharacter); // utility function to move along path
                if (path.Count == 0) // if returned path now finished
                {
                    utility.EndTurn(MobType.Enemy); // end the AI's turn, when end of path reached
                }
            }
            else if (coroutineRunning == false) // prevent multiple turns for AI
            {
                coroutineRunning = true;
                StartCoroutine(MakeDelayedMove());
            }   
        }
    }

    IEnumerator MakeDelayedMove() // used to delay the AI's move choice
    {
        yield return new WaitForSeconds(.5f);

        MakeMove();
    }

    private void MakeMove()
    {
        coroutineRunning = false;
        hand = CardManager.Instance.aiHand;

        if (roamerChasing != null)
        {
            // if a romaer character is currently chasing a hero character this will be the AI's move
            RoamerMove(roamerChasing);
        }

        else if (hand.Count == 0 && charactersInPlay.Count == 0) // no available moves, end AI's turn
        {
            utility.EndTurn(MobType.Enemy);
        }

        else if (charactersInPlay.Count == 0 && hand.Count > 0) // no characters in play, AI must spawn in character
        {
            SpawnCharacter();
        }

        else
        {
            CalculateMove(); // AI need to decide on which move to take
        }
    }

    private void CalculateMove()
    {
        Character chosenCharacter = ChooseCharacter(); // selects character for move

        if (chosenCharacter != null) // not definate a Character will be returned, e.g. could decide to spawn in new character
        {
            switch (chosenCharacter.style) // based on chosen characters PlayStyle make appropiate move
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
                    // if only scared character left must force it to not run away 
                    ScaredMove(chosenCharacter, charactersInPlay.Count == 1 && hand.Count == 0); 
                    break;

                case PlayStyle.Default:
                    DefaultMove(chosenCharacter);
                    break;
            }
        } else
        {
            utility.EndTurn(MobType.Enemy);
        }
    }

    /*
     * Aggressor characters have very good attack damange. They prioritise (most to least):
     * 1. moving towards hero's shrine
     * 2. attacking weakest character within range
     * 3. attacking the hero's shrine
    */
    private void AggressorMove(Character aggressor)
    {
        var rangeTiles = rangeFinding.GetRangeTiles(aggressor.standingOnTile, aggressor.characterCard.range, aggressor.characterCard.cardType, true); // get list of tiles which are within range
        TileData shrineLocation = GridManager.Instance.heroShineTileData; 

        if (rangeTiles.Contains(shrineLocation)) // check if the hero's shrine is within range
        {
            Character characterToAttack = null;
            foreach (TileData tile in rangeTiles)
            {
                if (tile.character != null && tile.character.type == MobType.Hero) // tile contains character and is Hero character 
                {
                    if (characterToAttack == null)
                    {
                        characterToAttack = tile.character;
                    }
                    else if (tile.character.characterCard.currHealth < characterToAttack.characterCard.currHealth) // find weaskest character to attack
                    {
                        characterToAttack = tile.character;
                    }
                }
            }

            if (characterToAttack != null) // attack character if one within range
            {
                CharacterAttack(characterToAttack, aggressor);
                utility.EndTurn(MobType.Enemy);
            }

            else // otherwise attack shrine
            {
                ShrineAttack(shrineLocation.shrine, aggressor.characterCard.attack);
                utility.EndTurn(MobType.Enemy);
            }
        }

        else // hero shrine not in range of character, pathfind to hero's shrine
        {
            GetValidPath(aggressor.standingOnTile, shrineLocation, aggressor, true);
        }
    }

    /*
     * Roamer characters have very good range. They prioritise (most to least):
     * 1. attacking cloest hero 
     * 2. moving towards cloest character
     * 3. attacking hero's shrine
     * 4. moving towards hero's shrine
    */
    private void RoamerMove(Character roamer)
    {
        Character cloestCharacter = FindClosestCharacter(roamer); // closest hero character to roamer
        var rangeTiles = rangeFinding.GetRangeTiles(roamer.standingOnTile, roamer.characterCard.range, roamer.characterCard.cardType, true);

        if (roamerChasing && roamerChasing != cloestCharacter) // if roamer is currently chasing but cloest isnt the character its chasing, attacks will instead focus on cloest hero character
        {
            roamerChasing = null;
        }

        if(cloestCharacter == null) // there are no hero characters on the board
        {
            var shrineLocation = GridManager.Instance.heroShineTileData; 
            if (rangeTiles.Contains(shrineLocation)) // if within range of hero's shrine
            {
                ShrineAttack(shrineLocation.shrine, roamer.characterCard.attack);
                utility.EndTurn(MobType.Enemy);
            }
            else
            {
                GetValidPath(roamer.standingOnTile, shrineLocation, roamer); // not in range of shrine, move towards
            }
        }

        else if (rangeTiles.Contains(GridManager.Instance.GetTileData(cloestCharacter.standingOnTile.gridLocation).Value)) // if within range of cloest character, then attack
        {
            CharacterAttack(cloestCharacter, roamer);
            utility.EndTurn(MobType.Enemy);
        }

        else // otherwise move towards cloest character
        {
            if (roamer.pathToHeroCharacter.Count > 0 && InputManager.Instance.charactersInPlay.Contains(roamer.movingTowardsCharacter)) // if pre-determined path has been calcualted from event emmit of hero character movement (ensures it still exists, i.e. not been killed)
            {
                path = roamer.pathToHeroCharacter; // set the path to the pre-determined 
            }
            else
            {
                // otherwise use path finding to find path to character and set the chosen roamer character to listen for this hero character
                roamer.movingTowardsCharacter = cloestCharacter; 
                GetValidPath(roamer.standingOnTile, cloestCharacter.standingOnTile, roamer);
            }
        }
    }

    /*
     * Defender characters have very good health. They dont want to leave their half of the grid. They prioritise (most to least):
     * 1. attacking weakest hero character within range
     * 2. moving towards weakest hero character (as long as within enemies side)
     * 3. moving back to spawn location
*/
    private void DefenderMove(Character defender) 
    {
        var enemyCharacterTiles = rangeFinding.GetDefenderTilesWithHeroCharacters(); // tiles in enemies side of the grid, which a hero character is standing on

        if (enemyCharacterTiles.Count != 0)
        {   
            var rangeTiles = rangeFinding.GetRangeTiles(defender.standingOnTile, defender.characterCard.range, defender.characterCard.cardType, true);

            Character characterToAttack = GetWeakestCharacter(enemyCharacterTiles); // get weakest hero character 

            if (rangeTiles.Contains(GridManager.Instance.GetTileData(characterToAttack.standingOnTile.gridLocation).Value)) // attack if in range of hero character
            {
                CharacterAttack(characterToAttack, defender);
                utility.EndTurn(MobType.Enemy);
            }
            else // otherwise move towards hero character
            {
                if (defender.pathToHeroCharacter.Count > 0)
                {
                    path = defender.pathToHeroCharacter;
                }
                else
                {
                    defender.movingTowardsCharacter = characterToAttack;
                    GetValidPath(defender.standingOnTile, characterToAttack.standingOnTile, defender);
                    defender.pathToHeroCharacter = path;
                }
            }
        } 

        else // if no hero characters in enemies half of grid either 
        {
            if (defender.spawnTile.gridLocation == defender.standingOnTile.gridLocation) // if already at spawn point end turn
            {
                DefenderMoveTowardsCharacter(defender); 
            } else
            {
                GetValidPath(defender.standingOnTile, defender.spawnTile, defender); // return to spawn point
            }
        }
    }

    /*
     * Scared characters are rubbish at everything!
     * Only aim is to run away from hero's characters 
     * Playstyle is forced to be a defender if they are the only playstyles left to play with
    */
    private void ScaredMove(Character scared, bool forceAction)
    {
        if (forceAction) // if forcing scared to do something apart from run away
        {
            DefenderMove(scared);
        } 
        
        else
        {
            Character closestCharacter = FindClosestCharacter(scared); // find cloest hero character
            var rangeTiles = rangeFinding.GetRangeTiles(closestCharacter.standingOnTile, closestCharacter.characterCard.range, closestCharacter.characterCard.cardType, false);

            TileData scaredTile = GridManager.Instance.GetTileData(scared.standingOnTile.gridLocation).Value;

            if (rangeTiles.Contains(scaredTile)) // if scared character is within range of hero's cloest character - run away 
            {
                RunAway(scaredTile);
            } else // otherwise end turn (rarely called as AI will avoid playing with scared characters)
            {
                utility.EndTurn(MobType.Enemy);
            }
        }
    }

    /*
     * Default characters dont meet the criteria for any other playStlyes. They prioritise (most to least):
     * 1. attacking any hero characters within range
     * 2. attacking hero's shrine
     * 3. moving towards hero's shrine
*/
    private void DefaultMove(Character balanced)
    {
        var rangeTiles = rangeFinding.GetRangeTiles(balanced.standingOnTile, balanced.characterCard.range, balanced.characterCard.cardType, true);

        Character closestInRange = FindClosestCharacter(balanced, balanced.characterCard.range); // find cloest hero character (in range)

        if (closestInRange != null) // if hero character in range - attack
        {
            CharacterAttack(closestInRange, balanced);
            utility.EndTurn(MobType.Enemy);
        }

        else 
        {
            var shrineLocation = GridManager.Instance.heroShineTileData;
            if (rangeTiles.Contains(shrineLocation)) // if within range of hero's shrine - attack shrine
            {
                ShrineAttack(shrineLocation.shrine, balanced.characterCard.attack);
                utility.EndTurn(MobType.Enemy);
            }
            else // otherwise move towards hero's shrine
            {
                GetValidPath(balanced.standingOnTile, shrineLocation, balanced, true);
            }
        }
    }

    // Deals damage to hero's character, and determines if attack is blocked
    private void CharacterAttack(Character victim, Character attacker)
    {
        if (victim == latestHeroCharacterAttack && attacker == latestCharacterToAttack) // if the latest Ai character to attack is the attacker and the latest hero character to be attacked is the victim, the block multiplier must grow
        {
            blockChance = Mathf.Min(.7f, blockChance * Random.Range(1.05f, 1.25f)); // ensure the block chance never goes above 70%, and increases the block chance randomly between 5% and 25%
        } else
        {
            // if its not a repeated attack, updates the latest
            latestHeroCharacterAttack = victim;
            latestCharacterToAttack = attacker;

            blockChance = .1f; // and resets the block chance
        }

        victim.TakeDamage(Random.value < blockChance ? 0 : attacker.characterCard.attack); // randomly decide if the attack will be blocked, if so attack is 0 otherwise attack attribute is used
    }

    // Deals damage to hero's shrine
    private void ShrineAttack(Shrine shrine, int damage)
    {
        shrine.TakeDamage(damage);
    }

    /* Uses pathfinding to get path from start -> destination 
     * will ensure detination is valid (doesn't already have a character standing on it)
    */
    private void GetValidPath(TileData start, TileData destination, Character character, bool avoidOpponent = false)
    {
        var path = pathfinding.FindPath(start, destination, character.characterCard.cardType, character.characterCard.range, avoidOpponent); // get path to destination, truncates path if exceeds characters range
    
        if (path.Count > 0)
        {
            // loop through path, back to front, to find first tile which is a valid tile to end on 
            for (int i = path.Count - 1; i >= 0; i--) 
            {
                if (path[i].character == null && !path[i].shrineLocation) // if no character on tile and tile isnt a shrine location 
                {
                    break; // the wanted destination is valid and can be reached
                }
                else // otherwise remove from the path
                {
                    path.RemoveAt(i);
                }
            }

            if (path.Count == 0) // if after removing invalid destinations, path count = 0; end turn
            {
                utility.EndTurn(MobType.Enemy);
            }

            pathCharacter = character; // sets which character will be moved
        } 
        else
        {
            utility.EndTurn(MobType.Enemy);
        }
        
        GridManager.Instance.RemoveCharacterFromTile(pathCharacter.standingOnTile.gridLocation); // remove character from start tile
        
        this.path = path; // set path to path (will use this for lateUpdate to move the character)
    }

    // logic for scared characters to run away from hero characters
    private void RunAway(TileData scaredTile)
    {
        Vector2Int topCorner = new(gridWith - 1, gridHeight - 1);
        Vector2Int bottomCorner = new(gridWith - 1, 0);

        var topCornerPath = pathfinding.FindPath(scaredTile, GridManager.Instance.GetTileData(topCorner).Value, scaredTile.character.characterCard.cardType, scaredTile.character.characterCard.range);
        var bottomCornerPath = pathfinding.FindPath(scaredTile, GridManager.Instance.GetTileData(bottomCorner).Value, scaredTile.character.characterCard.cardType, scaredTile.character.characterCard.range);

        if (topCornerPath.Count < bottomCornerPath.Count) // if bottom corner is closer, move to bottom corner
        {
            GetValidPath(scaredTile, GridManager.Instance.GetTileData(bottomCorner).Value, scaredTile.character);
        } 
        else // otherwise move to top corner
        {
            GetValidPath(scaredTile, GridManager.Instance.GetTileData(topCorner).Value, scaredTile.character);
        }
    }

    private void DefenderMoveTowardsCharacter(Character defender)
    {
        Character cloestCharacter = FindClosestCharacter(defender);
        GetValidPath(defender.standingOnTile, cloestCharacter.standingOnTile, defender);
    }

    /* Finds cloest hero character to character, optional range parameter which ensures cloest character search is fixed within characters range 
    */
    private Character FindClosestCharacter(Character character, int range = int.MaxValue)
    {
        List<Character> playerCharacters = InputManager.Instance.charactersInPlay; // list of hero characters in play

        Character? closestCharacter = null;
        int closestDistance = int.MaxValue; // default to impossible cloest in order for first character to be considered

        foreach (Character chara in playerCharacters) 
        {
            int distance = pathfinding.FindPath(character.standingOnTile, chara.standingOnTile, character.characterCard.cardType).Count; // get distance of path to hero character
            if (distance < closestDistance && distance <= range) // distance is smallest found (and optionally within range)
            {
                closestCharacter = chara;
                closestDistance = distance;
            }
        }

        return closestCharacter == null ? null : closestCharacter;
    }

    // returs the character from list of characters which has the lowest health
    private Character GetWeakestCharacter(List<TileData> charactersTileData)
    {
        if (charactersTileData.Count == 0) // potentially empty to just return null
        {
            return null;
        }

        if (charactersTileData.Count == 1) // if only 1, no need to loop so just retuns this character
        {
            return charactersTileData[0].character;
        }

        Character selected = null;
        int weakest = int.MaxValue; // impossible current health weakest
        foreach (TileData tileData in charactersTileData)
        {
            if (tileData.character.characterCard.currHealth < weakest) // if current character has lower health than previous lowest
            {
                // set this as selected character
                selected = tileData.character; 
                weakest = tileData.character.characterCard.currHealth;
            }
        }

        return selected;
    }

    // Randomly chooses a spawn tile for AI's characters
    private TileData GetSpawnTile()
    {
        var tileData = GridManager.Instance.tileData; // all map tiles data

        bool validTileFound = false; 
        TileData validTile = new();
        // impossible that no tiles are valid (doesn't cause infinite loop)
        while (!validTileFound) // loop through all valid spawn tiles (last 2 columns to the right)
        {
            Vector2Int pos = new(Random.Range(gridWith - 2, gridWith - 1), Random.Range(0, gridHeight - 1));

            if (tileData[pos].character == null) // if no character is already on the randomly chosen tile
            {
                validTileFound = true; //break loop
                validTile = tileData[pos];
            }
        }

        return validTile; // return the valid spawn tile
    }

    // used for sorting of character preferences
    private bool HasEnemiesWithinRange(Character character)
    {
        if (character.style == PlayStyle.Scared) // if character playStyle is scared, we dont want to consider if a hero character is within range, as it wont attack the character
        {
            return false;
        }
        else if (character.style == PlayStyle.Aggressor) 
        {
            var rangeTiles = rangeFinding.GetRangeTiles(character.standingOnTile, character.characterCard.range, character.characterCard.cardType, true); // get list of tiles which are within range
            TileData shrineLocation = GridManager.Instance.heroShineTileData;

            if (!rangeTiles.Contains(shrineLocation)) // agressor characters wont attack characters until its within range of the hero's shrine
            {
                return false;
            }
        }
        // returns bool value if a character is within range
        return FindClosestCharacter(character, character.characterCard.range) != null;
    }

    private Character ChooseCharacter()
    {
        // if user has been defeated, only thing to do is destroy shrine
        if (InputManager.Instance.charactersInPlay.Count == 0 && CardManager.Instance.userHand.Count == 0 && charactersInPlay.Count > 0)
        {
            Character chosenCharacter = charactersInPlay.OrderBy(character => character.standingOnTile.gridLocation.x).First(); // Gets the character in play with the lowest x value 

            chosenCharacter.style = PlayStyle.Aggressor; // change playstyle to Aggressor (primary aim is to attack shrine)

            charactersInPlay[0] = chosenCharacter; // override character 
            return charactersInPlay[0]; // return chosen character
        }

        if (Random.value < .2 && hand.Count > 0) // 20% of time AI will spawn a new character as long as it has card(s) in hand
        {
            SpawnCharacter(); // spawn chaeacter
            return null;
        }

        else
        {
            // order the characters in a list from most preferred to selct to least preferred
            List<Character> orderedCharacters = charactersInPlay
                .OrderByDescending(c => HasEnemiesWithinRange(c)) // firstly order by characters who have hero in range
                .ThenBy(c => (int)c.style) // then by prefered playStyles
                .ThenByDescending(c => c.characterCard.currHealth) // finally, by the current health
                .ToList();

            // if the most preferred character is of type Default, Defender or Scared, Ai will prefer to spawn in another character (as long as it can)
            if ((orderedCharacters[0].style == PlayStyle.Default || orderedCharacters[0].style == PlayStyle.Defender || orderedCharacters[0].style == PlayStyle.Scared) && hand.Count > 0)
            {
                SpawnCharacter();
                return null;
            }

            /* Ai will choose 2nd most preffered character if the current blockChance is greater than half the max block chance, and the most preffered character is the latest character to attack 
               (also some random, 40% of the time will take their chances and risk a blocked attack)
            */
            if (blockChance >= .35f && Random.value > .4f && orderedCharacters[0] == latestCharacterToAttack) 
            {
                orderedCharacters.RemoveAt(0); // remove the most preffered character from this move choice
            }

            // will return the most preffered character to play with
            return orderedCharacters[0];
        }
    }

    // Spawn character into map
    private void SpawnCharacter()
    {
        Character character = CardManager.Instance.SpawnInCharacter(GetSpawnTile(), SelectCard(), true);
        charactersInPlay.Add(character);

        utility.EndTurn(MobType.Enemy);
    }

    // Selects a card to play with, (chooses best cards first)
    private Card SelectCard()
    {
        Card selectedCard = hand[0]; // intially assume first index is the best
        int maxpoints = 0;

        foreach (Card card in hand) // loop through AI's hand 
        {
            int points = card.health + (card.range * 10) + (card.attack * 2); // calcualte its points 
            if (points > maxpoints)
            {
                maxpoints = points; // if points is greater than maxpoints, this becomes max points and the chosen card
                selectedCard = card;
            }
        }

        return selectedCard;
    }

    // if character's current health is <= 0, remove character from play
    private void CharacterKilled(Character character)
    {
        if (charactersInPlay.Contains(character)) // check to ensrue the killed character is of type AI
        {
            charactersInPlay.Remove(character);
        }
    }

    // used to update path stored in AI's characters when hero character chasing has been moved
    private void CharacterMoved(Character heroCharacter)
    {
        // go through all characters in play
        foreach (Character character in charactersInPlay)
        {
            // if the character has a focus on the moved character, we update its path to follow rather than re-calculating each time
            if (character.movingTowardsCharacter == heroCharacter)
            {
                var path = pathfinding.FindPath(character.standingOnTile, heroCharacter.standingOnTile, character.characterCard.cardType, character.characterCard.range); // find the path

                if (path.Count > 0)
                {
                    // loop through path, back to front, to find first tile which is a valid tile to end on 
                    for (int i = path.Count - 1; i >= 0; i--)
                    {
                        if (path[i].character == null && !path[i].shrineLocation) // if no character on tile and tile isnt a shrine location 
                        {
                            break; // the wanted destination is valid and can be reached
                        }
                        else // otherwise remove from the path
                        {
                            path.RemoveAt(i);
                        }
                    }
                }

                // update the path on the AI's character which will be used to pathfind towards when chosen
                character.pathToHeroCharacter = path;
            }
        }
    }
}
