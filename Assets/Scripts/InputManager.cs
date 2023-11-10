using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public GameObject[] characterPrefabs;

    private Pathfinding pathfinding;
    private RangeFinding rangeFinding;

    public GameObject container;

    private List<TileData> path;
    private List<TileData> rangeTiles;
    private Character pathCharacter;

    private bool showingRange = false;
    private TileData startPosTile;

    private bool isMoving = false;

    private void Start()
    {
        pathfinding = new Pathfinding();
        rangeFinding = new RangeFinding();
        path = new List<TileData>();
    }

    private void LateUpdate()
    {
        TileData? nullableFocusedTile = GridManager.Instance.GetTileData(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (nullableFocusedTile.HasValue && !isMoving && GameManager.Instance.GameState == GameState.HeroesTurn)
        {
            TileData focusedTile = nullableFocusedTile.Value;

            if (Input.GetMouseButtonDown(0))
            {
                if (focusedTile.character != null)
                {
                    if (showingRange)
                    {
                        HideRange();
                        showingRange = false;
                    }
                    else
                    {
                        ShowRange(focusedTile, focusedTile.character);
                        showingRange = true;
                    }

                    startPosTile = focusedTile;
                }
                else
                {
                    Card? selectedCard = CardManager.Instance.GetSelectedCard();
                    if (selectedCard.HasValue && focusedTile.gridLocation.x < 2)
                    {
                        SpawnInCharacter(focusedTile, selectedCard.Value);
                        EndUserTurn();
                    }
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (startPosTile.character != null && focusedTile.character == null && rangeTiles.Contains(focusedTile))
                {
                    path = pathfinding.FindPath(startPosTile, focusedTile);
                    pathCharacter = startPosTile.character;

                    GridManager.Instance.RemoveCharacterFromTile(pathCharacter.standingOnTile.gridLocation);
                    EndUserTurn();
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
        GameManager.Instance.ChangeGameState(GameState.EnemiesTurn);
    }

    private void ShowRange(TileData tile, Character character)
    {
        rangeTiles = rangeFinding.GetRangeTiles(tile, character.characterCard.range);

        Debug.Log(character.characterCard.name);
        Debug.Log(character.characterCard.description);
        Debug.Log("----------");
        Debug.Log(character.characterCard.attack);
        Debug.Log(character.characterCard.range);
        Debug.Log(character.characterCard.health);

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

    private void SpawnInCharacter(TileData tile, Card card)
    {
        Character character = Instantiate(characterPrefabs[(int)card.cardType]).GetComponent<Character>();
        character.transform.position = new Vector3(tile.gridLocation.x, tile.gridLocation.y, -2f);

        character.standingOnTile = tile;
        character.characterCard = card;

        character.transform.SetParent(container.transform);
        character.name = character.characterCard.name;

        GridManager.Instance.SetCharacterOnTile(character, tile.gridLocation);
        CardManager.Instance.PlayCard(card);
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
        }
    }

    private void PositionCharacterOnLine(TileData tile)
    {
        pathCharacter.transform.position = new Vector3(tile.gridLocation.x, tile.gridLocation.y + 0.0001f, pathCharacter.transform.position.z);
        pathCharacter.GetComponent<SpriteRenderer>().sortingOrder = tile.tile.GetComponent<SpriteRenderer>().sortingOrder;
        pathCharacter.standingOnTile = tile;
    }
}
