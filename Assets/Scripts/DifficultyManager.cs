using UnityEngine;
using UnityEngine.UI;

public class DifficultyManager : MonoBehaviour
{
    [SerializeField] private Slider slider; // slider component to control difficulty

    private void Start()
    {
        int difficulty = PlayerPrefs.GetInt("SelectedDifficulty", -1); // gets players chosen difficulty (if non saved will default to -1)

        if (difficulty != -1) // if a difficulty is set this will be the sliders value
        {
            slider.value = difficulty;
        }

        // add event listener for slider changes
        slider.onValueChanged.AddListener((value) =>
        {
            SetDifficulty((int)value); // update the game difficulty 
        });
    }

    public void OnDestroy()
    {
        slider.onValueChanged.RemoveListener((value) => SetDifficulty((int)value)); // remove listener
    }

    public void SetDifficulty(int newDifficulty)
    {
        PlayerPrefs.SetInt("SelectedDifficulty", newDifficulty); // sets the new difficulty in player prefs 
    }
}
