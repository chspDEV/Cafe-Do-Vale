using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    //Offset de rotação
    public Vector3 rotationOffset;

    //Offset de posição
    public Vector3 positionOffset;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        Vector3 targetPosition = transform.position + positionOffset;

        transform.LookAt(targetPosition + mainCamera.transform.forward);
        transform.Rotate(rotationOffset);
    }
}