using UnityEngine;

public class Shrine : MonoBehaviour
{
    public ShrineData shrineData;

    public HealthBar healthBar;

    public GameObject attack;

    public void Awake()
    {
        healthBar = GetComponentInChildren<HealthBar>(); // when initialised, gets child component of healthBar
    }

    public void InitShineData(Tile tile, MobType shrineType) // when created, will set the healthBar to the shrines health and store the shrine data
    {
        shrineData = new ShrineData(tile, shrineType);
        healthBar.SetMaxHealth(shrineData.health);
    }

    // Applied a damage amount to the shrine when it is attacked
    public void TakeDamage(int damageAmount)
    {
        // spawns in attack indicator for .5 seconds 
        Vector3 pos = new(transform.position.x, transform.position.y, -6); 
        var attObject = Instantiate(attack, pos, Quaternion.identity);
        Destroy(attObject, .5f);

        // updates health of healthBar and the shrine 
        shrineData.currHealth -= damageAmount;
        healthBar.SetHealth(shrineData.currHealth);

        // if shrine has been destroyed from the attack, game is finished
        if (shrineData.currHealth <= 0)
        {
            // depending on which MobType the shrine is determines whos won 
            EventSystem.RaiseGameStateChange(shrineData.shrineType == MobType.Hero ? GameState.EnemyWin : GameState.HeroWin);
        }
    }
}
