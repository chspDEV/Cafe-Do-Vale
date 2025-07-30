using UnityEngine;
using UnityEngine.UIElements;

public class Bee : MonoBehaviour
{
    [Header("Collision Detection")]
    [SerializeField] private float collisionDistance = 50f;


    [SerializeField] private GameObject path;

    private float speed;
    private Vector2 direction;
    private RectTransform rectTransform;
    private RectTransform playerHands;
    private bool hasStung = false;

    public void Initialize(float beeSpeed, Vector2 beeDirection, RectTransform _playerHand)
    {
        speed = beeSpeed;
        direction = beeDirection;
        rectTransform = GetComponent<RectTransform>();
        playerHands = _playerHand;

        //inverte a sprite da abelha se ela estiver indo para a esquerda
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        Destroy(path, 1.3f);
    }

    private void Update()
    {
        //move a abelha
        rectTransform.localPosition += (Vector3)direction * speed * Time.deltaTime;

        // Verifica colis�o com o jogador por dist�ncia
        CheckPlayerCollision();

        //destroi a abelha se ela sair da tela para evitar acumulo de objetos
        if (!Screen.safeArea.Contains(transform.position))
        {
            //pequena margem para ter certeza que saiu da tela
            if (Vector3.Distance(transform.position, Vector3.zero) > 2000)
            {
                Destroy(gameObject);
            }
        }
    }

    private void CheckPlayerCollision()
    {
        if (hasStung || playerHands == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerHands.position);

        if (distanceToPlayer <= collisionDistance)
        {
            hasStung = true;

            // Dispara o evento de ferroada
            PlayerHand.PlayerTakeDamage();

            // Destroi a abelha ap�s colidir
            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Desenha o c�rculo de colis�o da abelha
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionDistance);

        // Desenha uma linha at� o jogador se ele existir
        if (playerHands != null)
        {
            float distance = Vector3.Distance(transform.position, playerHands.position);

            // Muda a cor baseado na dist�ncia
            if (distance <= collisionDistance)
            {
                Gizmos.color = Color.red; // Colidindo
            }
            else
            {
                Gizmos.color = Color.yellow; // Pr�ximo
            }

            Gizmos.DrawLine(transform.position, playerHands.position);

            // Desenha texto com a dist�ncia (s� funciona no Scene View)
            UnityEditor.Handles.Label(transform.position + Vector3.up * 30, $"Dist: {distance:F1}");
        }
    }

#endif
}