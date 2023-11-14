using UnityEngine;

public class Character : MonoBehaviour
{
    public TileData standingOnTile;

    public Card characterCard;

    public MobType type;

    public PlayStyle style;

    public HealthBar healthBar;

    public GameObject attack;

    private void Awake()
    {
        healthBar = GetComponentInChildren<HealthBar>();
    }

    private void Start()
    {
        healthBar.SetMaxHealth(characterCard.health);
    }

    public void TakeDamage(int damageAmount)
    {
        Vector3 pos = new(transform.position.x, transform.position.y, -6);
        var attObject = Instantiate(attack, pos, Quaternion.identity);
        Destroy(attObject, .5f);

        characterCard.currHealth -= damageAmount;
        healthBar.SetHealth(characterCard.currHealth);

        if (characterCard.currHealth <= 0)
        {
            AiManager2.Instance.CharacterKilled(this);
            InputManager.Instance.CharacterKilled(this);
            Destroy(this.gameObject);
        }
    }
}
