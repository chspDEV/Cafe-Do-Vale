using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class ImageToFill : MonoBehaviour
{
    public Image image;
    public Image bgImage;
    public RectTransform rect;
    private float maxTime = 1f;
    private Billboard billboard;

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

    public void ChangeSize(Vector3 newSize)
    { 
        transform.localScale = newSize;

    }

    //gambiarra
    // Para FillMethod.Vertical (0 = Bottom, 1 = Top)
    public void ChangeFillStart(int originIndex)
    {
        // Garante que o valor seja 0 ou 1
        originIndex = Mathf.Clamp(originIndex, 0, 1);

        image.fillOrigin = originIndex;
        image.SetAllDirty(); // Atualiza visualmente
    }

    public void ChangeBillboard(Vector3 _positionOffset, Vector3 _rotationOffset)
    {
        billboard.positionOffset = _positionOffset;
        billboard.rotationOffset = _rotationOffset;
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
        billboard = GetComponent<Billboard>();
            
    }

}
