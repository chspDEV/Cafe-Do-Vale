using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataSlot: MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI amount, slotName;

    public void Setup(Sprite newSprite, int newAmount)
    {
        image.sprite = newSprite;
        amount.text = "x" + newAmount.ToString();
    }

    public void Setup(Sprite newSprite, string newName)
    {
        image.sprite = newSprite;
        slotName.text = newName.ToString();
    }

}