using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    public Vector3 rotationOffset;
    public Vector3 positionOffset;

    private Vector3 initialLocalPosition;

    void Start()
    {
        mainCamera = Camera.main;

        // Guarda a posição local original para aplicar o offset sem acumular
        initialLocalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        // Calcula a rotação com base apenas na posição real
        Vector3 direction = mainCamera.transform.position - transform.position;
        direction.y = 0; // Para topdown

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(rotationOffset);
        }

        // Aplica o offset de posição localmente (visualmente)
        transform.localPosition = initialLocalPosition + positionOffset;
    }
}
