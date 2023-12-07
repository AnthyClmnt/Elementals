using UnityEngine;
using UnityEngine.UI;

public class DifficultyManager : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private void Start()
    {
        slider.onValueChanged.AddListener((value) =>
        {
            SetDifficulty((int)value);
        });
    }

    public void OnDestroy()
    {
        slider.onValueChanged.RemoveListener((value) => SetDifficulty((int)value));
    }

    public void SetDifficulty(int newDifficulty)
    {
        PlayerPrefs.SetInt("SelectedDifficulty", newDifficulty);
    }
}
