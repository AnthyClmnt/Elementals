using TMPro;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public TMP_Text damageText;

    public void SetText(int damage) // sets the text to show the damage caused by the opponent
    {
        RandomisePosition();
        damageText.text = $"{damage}";
    }

    private void RandomisePosition() // in order for the text to not always be in the same place, randomise (with the text's bounding box), the position of the damage text
    {
        RectTransform canvasRectTransform = damageText.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector3 randomPosition = new Vector3(Random.Range(-1f, 1f) * canvasRectTransform.rect.width, Random.Range(-1f, 1f) * canvasRectTransform.rect.height, 0f);
        damageText.rectTransform.localPosition = randomPosition;
    }
}
