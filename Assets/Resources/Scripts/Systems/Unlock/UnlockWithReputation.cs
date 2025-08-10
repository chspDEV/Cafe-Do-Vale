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

        // Verificação inicial para caso a reputação já seja suficiente no início
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
        // Adicionando uma verificação de segurança
        if (activeGameObject == null)
        {
            // Isso o avisará no console do Editor se a referência não foi definida
            Debug.LogWarning($"Active GameObject não foi definido no Inspector para o objeto '{this.gameObject.name}'.", this.gameObject);
            return; // Interrompe a execução para evitar erros
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