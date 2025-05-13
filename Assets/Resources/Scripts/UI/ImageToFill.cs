using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class ImageToFill : MonoBehaviour
{
    public Image image;
    public Image bgImage;
    public RectTransform rect;
    private float maxTime = 1f;

    public void Start()
    {
        SetupSettings();
    }

    public void SetupMaxTime(float time)
    {
        maxTime = time;
    }

    public void UpdateFill(float cTime)
    {
        if (image == null) return;

        image.fillAmount = cTime / maxTime;
    }

    public void ChangeSprite(Sprite newSprite)
    {
        if (newSprite == null) return;

        image.sprite = newSprite;
        bgImage.sprite = newSprite;
        bgImage.color = new Color(0, 0, 0, 0.6f);
        SetupSettings();
    }

    public RectTransform GetRectTransform()
    {
        return rect;
    }

    public void SetFillMethod(Image.FillMethod method)
    { 
        image.fillMethod = method;
    }

    public void SetupSettings()
    {
        if (image == null) return;
            
        image.type = Image.Type.Filled;
        image.raycastTarget = false;
        image.fillMethod = Image.FillMethod.Vertical;
            
    }

}
