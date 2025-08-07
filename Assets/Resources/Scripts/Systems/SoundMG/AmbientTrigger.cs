// Componente para trigger automático de mudança de ambiente
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AmbientTrigger : MonoBehaviour
{
    [Header("Configuração do Trigger")]
    [SerializeField] private AmbientType ambientType = AmbientType.Fazenda;
    [SerializeField] private bool triggerOnEnter = true;
    [SerializeField] private bool triggerOnExit = false;
    [SerializeField] private AmbientType exitAmbientType = AmbientType.None;

    [Header("Configurações do Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        // Garante que o collider está configurado como trigger
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            if (showDebugLogs)
                Debug.LogWarning($"AmbientTrigger em {gameObject.name}: Collider não estava marcado como Trigger. Corrigido automaticamente.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnEnter && other.CompareTag(playerTag))
        {
            if (showDebugLogs)
                Debug.Log($"Player entrou na área: {ambientType}");

            AmbientSoundManager.Instance.ChangeAmbient(ambientType);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (triggerOnExit && other.CompareTag(playerTag))
        {
            if (showDebugLogs)
                Debug.Log($"Player saiu da área: {ambientType} -> {exitAmbientType}");

            AmbientSoundManager.Instance.ChangeAmbient(exitAmbientType);
        }
    }

    /// <summary>
    /// Força o trigger manualmente
    /// </summary>
    public void TriggerAmbient()
    {
        AmbientSoundManager.Instance.ChangeAmbient(ambientType);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Desenha o trigger na cena
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Desenha informações quando selecionado
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
    }
#endif
}