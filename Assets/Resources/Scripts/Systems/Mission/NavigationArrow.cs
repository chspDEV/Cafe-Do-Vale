using UnityEngine;

public class NavigationArrow : MonoBehaviour
{
    public Transform target;
    public Transform player;
    public float heightOffset = 2f;
    public float rotationSpeed = 5f;

    private void Update()
    {
        if (target == null || player == null) return;

        // Posiciona a seta acima do jogador
        transform.position = player.position + Vector3.up * heightOffset;

        // Rotaciona para apontar para o alvo
        Vector3 direction = target.position - transform.position;
        direction.y = 0; // Mantém a seta nivelada

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}