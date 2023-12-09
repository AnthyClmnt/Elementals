using TMPro;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public TMP_Text damageText;

    public void SetText(int damage)
    {
        RandomisePosition();
        damageText.text = $"{damage}";
    }

    private void RandomisePosition()
    {
        RectTransform canvasRectTransform = damageText.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector3 randomPosition = new Vector3(Random.Range(-1f, 1f) * canvasRectTransform.rect.width, Random.Range(-1f, 1f) * canvasRectTransform.rect.height, 0f);
        damageText.rectTransform.localPosition = randomPosition;
    }
}
