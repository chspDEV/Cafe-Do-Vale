using UnityEngine;

public class NavigationArrow : MonoBehaviour
{
    public Transform target;
    public Transform player;
    public float heightOffset = 2f;
    public float rotationSpeed = 10f; // Aumentada para resposta mais rápida

    private void Update()
    {
        if (target == null || player == null) return;

        // Posiciona a seta acima do jogador
        transform.position = player.position + Vector3.up * heightOffset;

        // Calcula a direção ignorando diferenças de altura entre jogador e alvo
        Vector3 horizontalDirection = (target.position - player.position).normalized;
        horizontalDirection.y = 0;

        if (horizontalDirection != Vector3.zero)
        {
            // Rotação instantânea para melhor precisão
            Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
            transform.rotation = targetRotation;

            // Caso prefira suavização (mais lenta):
            // transform.rotation = Quaternion.Slerp(
            //     transform.rotation,
            //     targetRotation,
            //     rotationSpeed * Time.deltaTime
            // );
        }
    }
}