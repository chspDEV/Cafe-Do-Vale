using System;
using Tcp4;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;

public class CreationArea : BaseInteractable
{
    [SerializeField] bool isTimeImageActive = false;
    [SerializeField] float cTime = 0f;
    public event Action OnReachMaxTime;
    public int index = -1;

    [SerializeField] private ImageToFill timeImage;

    public override void Start()
    {
        base.Start();

        timeImage = UIManager.Instance.PlaceFillImage(transform);
        timeImage.ChangeSprite(GameAssets.Instance.sprRefinamentWait);
        timeImage.SetFillMethod(UnityEngine.UI.Image.FillMethod.Radial360);

        timeImage.ChangeSize(new(0.7f, .7f, .7f));
        timeImage.transform.position = new(timeImage.transform.position.x, 
            timeImage.transform.position.y, timeImage.transform.position.z + 1f);

        OnReachMaxTime += CreationManager.Instance.FinishDrink;
    }

    public override void Update()
    {
        base.Update();

        timeImage.ControlVisibility(isTimeImageActive);

        if (isTimeImageActive)
        {
            timeImage.UpdateFill(cTime);
            cTime += Time.deltaTime;

            if (cTime >= timeImage.GetMaxTime())
            {
                ResetPrepare();
                OnReachMaxTime?.Invoke();
            }
        }
    }

    public void StartPrepare(float maxTime)
    {
        cTime = 0f;
        timeImage.SetupMaxTime(maxTime);
        timeImage.UpdateFill(0);
        isTimeImageActive = true;
        DisableInteraction();
    }

    public void ResetPrepare()
    {
        cTime = 0f;
        timeImage.UpdateFill(0);
        isTimeImageActive = false;
        EnableInteraction();
    }


    public override void OnInteract()
    {
        base.OnInteract();
        UIManager.Instance.UpdateCreationView();
        CreationManager.Instance.lastIdInteracted = index;
        ControlMenu(true);
    }

    public override void OnLostFocus()
    {
        base.OnLostFocus();
        ControlMenu(false);
    }

    public void ControlMenu(bool isActive)
    {
        UIManager.Instance.ControlCreationMenu(isActive);
    }
}
