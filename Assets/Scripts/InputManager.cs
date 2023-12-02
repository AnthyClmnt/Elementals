using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.XR;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public GameObject[] characterPrefabs;

    private Pathfinding pathfinding;
    private RangeFinding rangeFinding;

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

        path = new List<TileData>();
    }

    private void LateUpdate()
    {
        if (CardManager.Instance.userHand.Count == 0 && charactersInPlay.Count == 0 && path.Count == 0)
        {
            EndUserTurn();
        }

        TileData? nullableFocusedTile = GridManager.Instance.GetTileData(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (nullableFocusedTile.HasValue && !isMoving && GameManager.Instance.gameState == GameState.HeroesTurn)
        {
            TileData focusedTile = nullableFocusedTile.Value;

            if (Input.GetMouseButtonDown(0))
            {
                if (focusedTile.character != null && focusedTile.character.type == MobType.Hero)
                {
                    if (showingRange)
                    {
                        HideRange();
                        selectedCharacter = null;
                        UiManager.Instance.HandleCharacterStatsUI(focusedTile.character);
                        showingRange = false;
                    }
                    else
                    {
                        ShowRange(focusedTile, focusedTile.character);
                        selectedCharacter = focusedTile.character;
                        UiManager.Instance.HandleCharacterStatsUI(focusedTile.character);
                        showingRange = true;
                    }

                    startPosTile = focusedTile;
                }

                else if (showingRange && rangeTilesIgnoreWalkable.Contains(focusedTile))
                {
                    if (focusedTile.shrineLocation && focusedTile.gridLocation.x != 0)
                    {
                        HideRange();
                        UiManager.Instance.HandleCharacterStatsUI(focusedTile.character);
                        focusedTile.shrine.TakeDamage(startPosTile.character.characterCard.attack);
                        EndUserTurn();
                    }
                    else if (focusedTile.character && focusedTile.character.type == MobType.Enemy)
                    {
                        HideRange();
                        UiManager.Instance.HandleCharacterStatsUI(focusedTile.character);
                        focusedTile.character.TakeDamage(selectedCharacter.characterCard.attack);
                        EndUserTurn();
                    }

                }
                else
                {
                    Card? selectedCard = CardManager.Instance.GetSelectedCard();
                    if (selectedCard.HasValue && focusedTile.gridLocation.x < 2 && !focusedTile.shrineLocation && focusedTile.character == null)
                    {
                        SpawnInCharacter(focusedTile, selectedCard.Value);
                        EndUserTurn();
                    }
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (startPosTile.character != null && focusedTile.character == null && rangeTiles.Contains(focusedTile) && showingRange && !focusedTile.shrineLocation)
                {
                    path = pathfinding.FindPath(startPosTile, focusedTile, startPosTile.character.characterCard.cardType);
                    pathCharacter = startPosTile.character;

                    if (path.Count > 0)
                    {
                        GridManager.Instance.RemoveCharacterFromTile(pathCharacter.standingOnTile.gridLocation);
                        UiManager.Instance.HandleCharacterStatsUI(focusedTile.character);
                    }
                }
            }
        }

        if (path.Count > 0)
        {
            isMoving = true;
            HideRange();
            MoveAlongPath();
        }
    }

    private void EndUserTurn()
    {
        if (GameManager.Instance.gameState != GameState.EnemyWin && GameManager.Instance.gameState != GameState.HeroWin)
        {
            GameManager.Instance.ChangeGameState(GameState.EnemiesTurn);
        }
        
    }

    private void ShowRange(TileData tile, Character character)
    {
        rangeTiles = rangeFinding.GetRangeTiles(tile, character.characterCard.range, character.characterCard.cardType);
        rangeTilesIgnoreWalkable = rangeFinding.GetRangeTiles(tile, character.characterCard.range, character.characterCard.cardType, true);

        foreach (TileData rangeTile in rangeTiles)
        {
            rangeTile.tile.ShowRangeTile();
        }

        showingRange = true;
    }

    private void HideRange()
    {
        foreach(TileData rangeTile in rangeTiles)
        {
            rangeTile.tile.HideRangeTile();
        }

        showingRange = false;
    }

    public void CharacterKilled(Character character)
    {
        if (charactersInPlay.Contains(character))
        {
            charactersInPlay.Remove(character);
        }
    }

    private void SpawnInCharacter(TileData tile, Card card)
    {
        Character character = CardManager.Instance.SpawnInCharacter(tile, card);
        charactersInPlay.Add(character);
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
            EndUserTurn();
        }
    }

    private void PositionCharacterOnLine(TileData tile)
    {
        pathCharacter.transform.position = new Vector3(tile.gridLocation.x, tile.gridLocation.y + 0.0001f, pathCharacter.transform.position.z);
        pathCharacter.GetComponent<SpriteRenderer>().sortingOrder = tile.tile.GetComponent<SpriteRenderer>().sortingOrder;
        pathCharacter.standingOnTile = tile;
    }
}
