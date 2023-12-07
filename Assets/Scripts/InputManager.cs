using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public GameObject[] characterPrefabs;

    private Pathfinding pathfinding;
    private RangeFinding rangeFinding;
    private Utility utility;

    public GameObject container;

    private List<TileData> path;
    private List<TileData> rangeTiles;
    private List<TileData> rangeTilesIgnoreWalkable;
    private Character pathCharacter;

    public Character selectedCharacter;

    public List<Character> charactersInPlay = new();

    private bool showingRange = false;
    private TileData startPosTile;

    private bool isMoving = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        pathfinding = new Pathfinding();
        rangeFinding = new RangeFinding();
        utility = new Utility();

        path = new List<TileData>();
    }

    private void OnEnable()
    {
        EventSystem.OnHeroDeath += CharacterKilled;
    }

    private void OnDisable()
    {
        EventSystem.OnHeroDeath -= CharacterKilled;
    }

    private void LateUpdate() // runs once per frame
    {
        if (GameManager.Instance.gameState == GameState.HeroesTurn) // nothing is run if its not the user's turn
        {
            if (CardManager.Instance.userHand.Count == 0 && charactersInPlay.Count == 0 && path.Count == 0) // if no moves are available to the user
            {
                EndUserTurn(); // end their turn
            }

            TileData? nullableFocusedTile = GridManager.Instance.GetTileData(Camera.main.ScreenToWorldPoint(Input.mousePosition)); // gets tileData of the tile user is hovering over
            if (nullableFocusedTile.HasValue && !isMoving) // ensures hovered position is actually a tile and not moving
            {
                TileData focusedTile = nullableFocusedTile.Value;
                if (Input.GetMouseButtonDown(0)) // if user left clicks
                {
                    if (focusedTile.character != null && focusedTile.character.type == MobType.Hero) // tile clicked contains a hero character 
                    {
                        if (showingRange) // hide the range
                        {
                            HideRange();
                            selectedCharacter = null; // de-select character
                            UiManager.Instance.HandleCharacterStatsUI(focusedTile.character); // hide character stats UI
                            showingRange = false;
                        }
                        else // show the range
                        {
                            ShowRange(focusedTile, focusedTile.character);
                            selectedCharacter = focusedTile.character; // select character
                            UiManager.Instance.HandleCharacterStatsUI(focusedTile.character); // show character stats UI
                            showingRange = true;
                        }

                        startPosTile = focusedTile; // save the current clicked tile as the start position
                    }

                    else if (showingRange && rangeTilesIgnoreWalkable.Contains(focusedTile)) // if range showing and left click on tile which is within range of startPositon character
                    {
                        if (focusedTile.shrineLocation && focusedTile.shrine.shrineData.shrineType != MobType.Hero) // if tile is a shrine location, attack shrine
                        {
                            HideRange();
                            UiManager.Instance.HandleCharacterStatsUI(focusedTile.character);
                            focusedTile.shrine.TakeDamage(startPosTile.character.characterCard.attack);
                            EndUserTurn();
                        }
                        else if (focusedTile.character && focusedTile.character.type == MobType.Enemy) // if tile is a enemies character location, attack character
                        {
                            HideRange();
                            UiManager.Instance.HandleCharacterStatsUI(focusedTile.character);
                            focusedTile.character.TakeDamage(selectedCharacter.characterCard.attack);
                            EndUserTurn();
                        }

                    }
                    else // if left click but not to select character or attack enemy shrine/character
                    {
                        Card? selectedCard = CardManager.Instance.GetSelectedCard(); // get the current selected card
                        // if there is a selected card and the user isnt trying to spawn a character where the shrine is and there isnt already a character on the tile, spawn in new character
                        if (selectedCard.HasValue && focusedTile.gridLocation.x < 2 && !focusedTile.shrineLocation && focusedTile.character == null)
                        {
                            SpawnInCharacter(focusedTile, selectedCard.Value); // spawn in character
                            EndUserTurn();
                        }
                    }
                }

                if (Input.GetMouseButtonDown(1)) // if right click
                {
                    // if we have a starting character to move with, and the right clicked focused tile doesnt contain a character and isnt a shrine location and is within range, find the path to it
                    if (startPosTile.character != null && focusedTile.character == null && rangeTiles.Contains(focusedTile) && showingRange && !focusedTile.shrineLocation)
                    {
                        path = pathfinding.FindPath(startPosTile, focusedTile, startPosTile.character.characterCard.cardType); // gets the path
                        pathCharacter = startPosTile.character; // sets which character to move when moving along the path

                        if (path.Count > 0) // ensure the character isnt already at the destination
                        {
                            GridManager.Instance.RemoveCharacterFromTile(pathCharacter.standingOnTile.gridLocation); // remove from tileData reference of character standing on tile
                            UiManager.Instance.HandleCharacterStatsUI(focusedTile.character);
                        }
                    }
                }
            }

            if (path.Count > 0) // aftter all above it executed if a path has been made, ending the user go will wait until it has moved along the path
            {
                isMoving = true; // path exists so a move is occuring
                HideRange();

                path = utility.MoveAlongPath(path, pathCharacter); // utility function for moving along path
                if (path.Count == 0) // if the returned path now finished
                {
                    EventSystem.RaiseHeroCharacterMove(pathCharacter);
                    isMoving = false; // were not moving 
                    EndUserTurn(); // and go has ended
                }
            }
        }
    }

    private void EndUserTurn() // end the users go
    {
        if (GameManager.Instance.gameState != GameState.EnemyWin && GameManager.Instance.gameState != GameState.HeroWin)
        {
            GameManager.Instance.ChangeGameState(GameState.EnemiesTurn);
        }
        
    }
    
    // highlights all tiles within range of character
    private void ShowRange(TileData tile, Character character)
    {
        rangeTiles = rangeFinding.GetRangeTiles(tile, character.characterCard.range, character.characterCard.cardType); // get the tiles within range 
        rangeTilesIgnoreWalkable = rangeFinding.GetRangeTiles(tile, character.characterCard.range, character.characterCard.cardType, true); // get the tiles within range and ignore walkable (for attacks)

        foreach (TileData rangeTile in rangeTiles) // for each tile, set the highlight sprite to active
        {
            rangeTile.tile.ShowRangeTile();
        }

        showingRange = true; 
    }

    // de-highlights all tiles within range of character
    private void HideRange()
    {
        foreach(TileData rangeTile in rangeTiles) // already have the range tiles, just go through all of them 
        {
            rangeTile.tile.HideRangeTile(); // de-activate the highlight the tile sprite
        }

        showingRange = false;
    }

    // removes character from play when killed
    public void CharacterKilled(Character character)
    {
        if (charactersInPlay.Contains(character)) // ensures character killed is a hero's character
        {
            charactersInPlay.Remove(character); // if so removes it from in play
        }
    }

    // spawns in the character chosen 
    private void SpawnInCharacter(TileData tile, Card card)
    {
        Character character = CardManager.Instance.SpawnInCharacter(tile, card); // spaen in character
        charactersInPlay.Add(character); // adds character to in play list
    }
}
