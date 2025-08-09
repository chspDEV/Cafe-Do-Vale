// Script UnlockWithReputation.cs (com uma pequena alteração na chamada)
using System;
using Tcp4;
using Unity.Cinemachine;
using UnityEngine;

public class UnlockWithReputation : MonoBehaviour
{
    
    public enum UnlockParameter { Activate, Destroy}

    public int requiredReputation = 1;
    public float cameraTransitionDuration = 1f;
    public GameObject activeGameObject;
    private bool isUnlocked;
    public CinemachineCamera myCamera;
    public UnlockParameter unlockParameter;

    private void Start()
    {
        UnlockManager.Instance.OnReputationChanged += CheckUnlockStatus;
        if (myCamera != null)
            myCamera.gameObject.SetActive(false);
    }

    private void CheckUnlockStatus()
    {
        if (UnlockManager.Instance.CurrentReputationLevel >= requiredReputation && !isUnlocked)
        {
            Unlock();
        }
    }

    private void Unlock()
    {
        isUnlocked = true;

        // ALTERAÇÃO AQUI: Chamando o novo método mais seguro
        if (myCamera != null)
        {
            CameraManager.Instance.ShowTemporaryCamera(myCamera, cameraTransitionDuration);
        }

        switch (unlockParameter)
        {
            case UnlockParameter.Destroy:
                Destroy(activeGameObject, cameraTransitionDuration - (cameraTransitionDuration * 0.25f));
                break;
            case UnlockParameter.Activate:
                activeGameObject.SetActive(true);
                break;
        }
    }

    private void OnDestroy()
    {
        if (UnlockManager.Instance != null)
        {
            UnlockManager.Instance.OnReputationChanged -= CheckUnlockStatus;
        }
    }
}