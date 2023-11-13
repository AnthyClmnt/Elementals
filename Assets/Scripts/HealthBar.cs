using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;   

    public void SetMaxHealth(int health)
    {
        if (slider != null)
        {
            slider.maxValue = health;
            slider.value = health;
        }
        else
        {
            Debug.Log("oh no");
        }
    }

    public void SetHealth(int newHealth)
    {
        slider.value = newHealth;
    }
}
