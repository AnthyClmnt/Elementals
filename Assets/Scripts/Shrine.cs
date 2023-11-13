using UnityEngine;

public class Shrine : MonoBehaviour
{
    public ShrineData shrineData;

    public HealthBar healthBar;

    public GameObject attack;

    public void Awake()
    {
        healthBar = GetComponentInChildren<HealthBar>();
    }

    public void InitShineData(Tile tile, MobType shrineType)
    {
        shrineData = new ShrineData(tile, shrineType);
        healthBar.SetMaxHealth(shrineData.health);
    }

    public void TakeDamage(int damageAmount)
    {
        Vector3 pos = new(transform.position.x, transform.position.y, -6);
        var attObject = Instantiate(attack, pos, Quaternion.identity);
        Destroy(attObject, .5f);

        shrineData.currHealth -= damageAmount;
        healthBar.SetHealth(shrineData.currHealth);

        if (shrineData.currHealth <= 0)
        {
            GameManager.Instance.ChangeGameState(shrineData.shrineType == MobType.Hero ? GameState.EnemyWin : GameState.HeroWin);
        }
    }
}
