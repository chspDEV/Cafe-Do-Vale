using System;
using Tcp4;
using Unity.Cinemachine;
using UnityEngine;

public class UnlockWithReputation : MonoBehaviour
{
    public enum UnlockParameter { Activate, Destroy }

    public int requiredReputation = 1;
    public float cameraTransitionDuration = 1f;
    public GameObject activeGameObject;
    private bool isUnlocked;
    public CinemachineCamera myCamera;
    public UnlockParameter unlockParameter;

    private void Start()
    {
        if (UnlockManager.Instance != null)
        {
            UnlockManager.Instance.OnReputationChanged += CheckUnlockStatus;
        }

        if (myCamera != null)
        {
            myCamera.gameObject.SetActive(false);
        }

        // Verifica��o inicial para caso a reputa��o j� seja suficiente no in�cio
        CheckUnlockStatus();
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
        // Adicionando uma verifica��o de seguran�a
        if (activeGameObject == null)
        {
            // Isso o avisar� no console do Editor se a refer�ncia n�o foi definida
            Debug.LogWarning($"Active GameObject n�o foi definido no Inspector para o objeto '{this.gameObject.name}'.", this.gameObject);
            return; // Interrompe a execu��o para evitar erros
        }

        isUnlocked = true;

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

}