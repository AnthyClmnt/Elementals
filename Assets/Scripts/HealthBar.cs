using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider; // slider component used to show the current health

    // initially sets the max health of the health bar
    public void SetMaxHealth(int health)
    {
        if (slider != null)
        {
            slider.maxValue = health;
            slider.value = health;
        }
    }

    // updates the health of the healthbar (when taken damage)
    public void SetHealth(int newHealth) 
    {
        slider.value = newHealth;
    }
}
