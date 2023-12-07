using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public TileData standingOnTile;

    public Card characterCard;

    public TileData spawnTile;

    public MobType type;

    public PlayStyle style;

    public HealthBar healthBar;

    public GameObject attack;

    public SpriteRenderer spriteRenderer;

    public Sprite enenmySprite;

    public Character movingTowardsCharacter;
    public List<TileData> pathToHeroCharacter = new();

    private void Awake()
    {
        healthBar = GetComponentInChildren<HealthBar>(); // find child component of healthbar
    }

    private void Start()
    {
        healthBar.SetMaxHealth(characterCard.health); // when initialised, sets the healthbar's health to the characters health

        if (type == MobType.Enemy) // alternative sprite used for enenmy (AI) characters, (red outline to help user see which characters are enemies)
        {
            spriteRenderer.sprite = enenmySprite;
        }
    }

    // deals damage to the character
    public void TakeDamage(int damageAmount)
    {
        // firstly visual indiactor of attack is shown for .5 seconds
        Vector3 pos = new(transform.position.x, transform.position.y, -6);
        var attObject = Instantiate(attack, pos, Quaternion.identity);
        Destroy(attObject, .5f);

        // then updates both the characters current health and the healthbar's health
        characterCard.currHealth -= damageAmount;
        healthBar.SetHealth(characterCard.currHealth);

        // if character has died from the attack
        if (characterCard.currHealth <= 0)
        {
            // call both player managers to remove character (checks if its actually their character)
            if (type == MobType.Hero)
            {
                EventSystem.RaiseHeroDeath(this);
            } 
            else
            {
                EventSystem.RaiseEnemyDeath(this);
            }

            Destroy(this.gameObject); // remove the character
        }
    }
}
