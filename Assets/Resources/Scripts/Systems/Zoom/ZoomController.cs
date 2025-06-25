using UnityEngine;
using Unity.Cinemachine;
using Tcp4.Assets.Resources.Scripts.Managers;

[RequireComponent(typeof(CinemachineCamera))]
public class ZoomController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minFOV = 25f;
    [SerializeField] private float maxFOV = 45f;
    [SerializeField] private float smoothTime = 0.2f;

    private float currentZoomVelocity;
    private float targetFOV;

    private void Awake()
    {
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineCamera>();
        }

        targetFOV = virtualCamera.Lens.FieldOfView;
    }

    private void Update()
    {
        if (virtualCamera == null || GameAssets.Instance.currentInputType != CurrentInputType.PC) return;

        float scrollInput = Input.mouseScrollDelta.y * zoomSpeed;

        if (Mathf.Abs(scrollInput) > Mathf.Epsilon)
        {
            targetFOV -= scrollInput;
            targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
        }

        virtualCamera.Lens.FieldOfView = Mathf.SmoothDamp(
            virtualCamera.Lens.FieldOfView,
            targetFOV,
            ref currentZoomVelocity,
            smoothTime
        );
    }

    private void OnValidate()
    {
        if (virtualCamera != null)
        {
            virtualCamera.Lens.FieldOfView = Mathf.Clamp(
                virtualCamera.Lens.FieldOfView,
                minFOV,
                maxFOV
            );
        }
    }
}