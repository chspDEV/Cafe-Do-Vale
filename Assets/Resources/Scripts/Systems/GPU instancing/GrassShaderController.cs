using UnityEngine;

[System.Serializable]
public class WindSettings
{
    [Header("Wind Parameters")]
    [Range(0f, 10f)]
    public float windSpeed = 2f;

    [Range(0f, 2f)]
    public float windStrength = 0.5f;

    [Range(0.1f, 5f)]
    public float windFrequency = 1f;

    [Range(0.1f, 10f)]
    public float windNoiseScale = 1f;
}

[System.Serializable]
public class PlayerInteractionSettings
{
    [Header("Player Interaction")]
    [Range(0f, 20f)]
    public float influenceRadius = 5f;

    [Range(0f, 3f)]
    public float influenceStrength = 1f;

    [Range(0.1f, 5f)]
    public float recoverySpeed = 1f;

    [Range(0f, 10f)]
    public float innerRadius = 2f;
}

[System.Serializable]
public class GrassVisualSettings
{
    [Header("Grass Appearance")]
    public Color topColor = new Color(0.6f, 0.8f, 0.3f, 1f);
    public Color bottomColor = new Color(0.2f, 0.4f, 0.1f, 1f);

    [Range(0f, 1f)]
    public float shadowIntensity = 0.3f;

    [Range(0.1f, 3f)]
    public float grassHeight = 1f;

    [Range(0.1f, 2f)]
    public float bendStiffness = 1f;
}

/// <summary>
/// Controlador para facilitar a configuração do shader de grama interativa.
/// Deve ser anexado ao objeto que contém o GrassManager.
/// </summary>
[RequireComponent(typeof(GrassManager))]
public class GrassShaderController : MonoBehaviour
{
    [Header("Shader Settings")]
    public WindSettings windSettings = new WindSettings();
    public PlayerInteractionSettings playerInteraction = new PlayerInteractionSettings();
    public GrassVisualSettings visualSettings = new GrassVisualSettings();

    [Header("Performance")]
    [Tooltip("Frequência de atualização das propriedades do shader (em Hz). Menor valor = melhor performance.")]
    [Range(10f, 60f)]
    public float updateFrequency = 30f;

    [Header("Debug")]
    public bool showInfluenceRadius = false;

    private GrassManager grassManager;
    private Material grassMaterial;
    private float lastUpdateTime;
    private float updateInterval;

    // IDs de propriedades para otimização
    private static readonly int WindSpeedID = Shader.PropertyToID("_WindSpeed");
    private static readonly int WindStrengthID = Shader.PropertyToID("_WindStrength");
    private static readonly int WindFrequencyID = Shader.PropertyToID("_WindFrequency");
    private static readonly int WindNoiseScaleID = Shader.PropertyToID("_WindNoiseScale");

    private static readonly int PlayerInfluenceRadiusID = Shader.PropertyToID("_PlayerInfluenceRadius");
    private static readonly int PlayerInfluenceStrengthID = Shader.PropertyToID("_PlayerInfluenceStrength");
    private static readonly int RecoverySpeedID = Shader.PropertyToID("_RecoverySpeed");
    private static readonly int InnerRadiusID = Shader.PropertyToID("_InnerRadius");

    private static readonly int TopColorID = Shader.PropertyToID("_TopColor");
    private static readonly int BottomColorID = Shader.PropertyToID("_BottomColor");
    private static readonly int ShadowIntensityID = Shader.PropertyToID("_ShadowIntensity");
    private static readonly int GrassHeightID = Shader.PropertyToID("_GrassHeight");
    private static readonly int BendStiffnessID = Shader.PropertyToID("_BendStiffness");

    private void Start()
    {
        grassManager = GetComponent<GrassManager>();
        if (grassManager == null)
        {
            Debug.LogError("GrassShaderController precisa estar no mesmo GameObject que o GrassManager!");
            enabled = false;
            return;
        }

        grassMaterial = grassManager.grassMaterial;
        if (grassMaterial == null)
        {
            Debug.LogError("Material de grama não encontrado no GrassManager!");
            enabled = false;
            return;
        }

        updateInterval = 1f / updateFrequency;

        // Aplicar configurações iniciais
        UpdateShaderProperties();
    }

    private void Update()
    {
        // Atualização otimizada baseada em frequência
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateShaderProperties();
            lastUpdateTime = Time.time;
        }
    }

    private void UpdateShaderProperties()
    {
        if (grassMaterial == null) return;

        // Wind Settings
        grassMaterial.SetFloat(WindSpeedID, windSettings.windSpeed);
        grassMaterial.SetFloat(WindStrengthID, windSettings.windStrength);
        grassMaterial.SetFloat(WindFrequencyID, windSettings.windFrequency);
        grassMaterial.SetFloat(WindNoiseScaleID, windSettings.windNoiseScale);

        // Player Interaction
        grassMaterial.SetFloat(PlayerInfluenceRadiusID, playerInteraction.influenceRadius);
        grassMaterial.SetFloat(PlayerInfluenceStrengthID, playerInteraction.influenceStrength);
        grassMaterial.SetFloat(RecoverySpeedID, playerInteraction.recoverySpeed);
        grassMaterial.SetFloat(InnerRadiusID, playerInteraction.innerRadius);

        // Visual Settings
        grassMaterial.SetColor(TopColorID, visualSettings.topColor);
        grassMaterial.SetColor(BottomColorID, visualSettings.bottomColor);
        grassMaterial.SetFloat(ShadowIntensityID, visualSettings.shadowIntensity);
        grassMaterial.SetFloat(GrassHeightID, visualSettings.grassHeight);
        grassMaterial.SetFloat(BendStiffnessID, visualSettings.bendStiffness);
    }

    /// <summary>
    /// Aplica um impulso de vento em uma direção específica
    /// </summary>
    public void ApplyWindBurst(Vector2 direction, float intensity, float duration = 1f)
    {
        StartCoroutine(WindBurstCoroutine(direction, intensity, duration));
    }

    private System.Collections.IEnumerator WindBurstCoroutine(Vector2 direction, float intensity, float duration)
    {
        float originalStrength = windSettings.windStrength;
        float originalSpeed = windSettings.windSpeed;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float burstIntensity = intensity * (1f - t); // Decai com o tempo

            windSettings.windStrength = originalStrength + burstIntensity;
            windSettings.windSpeed = originalSpeed + burstIntensity * 2f;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restaurar valores originais
        windSettings.windStrength = originalStrength;
        windSettings.windSpeed = originalSpeed;
    }

    /// <summary>
    /// Simula uma rajada de vento aleatória
    /// </summary>
    public void RandomWindBurst()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomIntensity = Random.Range(0.5f, 1.5f);
        ApplyWindBurst(randomDirection, randomIntensity);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showInfluenceRadius || grassManager?.playerTransform == null) return;

        Vector3 playerPos = grassManager.playerTransform.position;

        // Raio interno (influência forte)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerPos, playerInteraction.innerRadius);

        // Raio externo (influência fraca)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerPos, playerInteraction.influenceRadius);
    }

    private void OnValidate()
    {
        // Garantir que o raio interno não seja maior que o externo
        if (playerInteraction.innerRadius > playerInteraction.influenceRadius)
        {
            playerInteraction.innerRadius = playerInteraction.influenceRadius;
        }

        // Aplicar mudanças em tempo real no editor
        if (Application.isPlaying && grassMaterial != null)
        {
            UpdateShaderProperties();
        }
    }
}

/// <summary>
/// Editor customizado para melhor organização no Inspector
/// </summary>
#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(GrassShaderController))]
public class GrassShaderControllerEditor : UnityEditor.Editor
{
    private bool windFoldout = true;
    private bool interactionFoldout = true;
    private bool visualFoldout = true;
    private bool performanceFoldout = false;
    private bool debugFoldout = false;

    public override void OnInspectorGUI()
    {
        GrassShaderController controller = (GrassShaderController)target;

        UnityEditor.EditorGUILayout.Space();

        // Wind Settings
        windFoldout = UnityEditor.EditorGUILayout.Foldout(windFoldout, "Wind Settings", true);
        if (windFoldout)
        {
            UnityEditor.EditorGUI.indentLevel++;
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("windSettings"));

            UnityEditor.EditorGUILayout.Space();
            if (GUILayout.Button("Random Wind Burst"))
            {
                controller.RandomWindBurst();
            }
            UnityEditor.EditorGUI.indentLevel--;
        }

        UnityEditor.EditorGUILayout.Space();

        // Player Interaction
        interactionFoldout = UnityEditor.EditorGUILayout.Foldout(interactionFoldout, "Player Interaction", true);
        if (interactionFoldout)
        {
            UnityEditor.EditorGUI.indentLevel++;
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("playerInteraction"));
            UnityEditor.EditorGUI.indentLevel--;
        }

        UnityEditor.EditorGUILayout.Space();

        // Visual Settings
        visualFoldout = UnityEditor.EditorGUILayout.Foldout(visualFoldout, "Visual Settings", true);
        if (visualFoldout)
        {
            UnityEditor.EditorGUI.indentLevel++;
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("visualSettings"));
            UnityEditor.EditorGUI.indentLevel--;
        }

        UnityEditor.EditorGUILayout.Space();

        // Performance
        performanceFoldout = UnityEditor.EditorGUILayout.Foldout(performanceFoldout, "Performance", true);
        if (performanceFoldout)
        {
            UnityEditor.EditorGUI.indentLevel++;
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("updateFrequency"));
            UnityEditor.EditorGUI.indentLevel--;
        }

        UnityEditor.EditorGUILayout.Space();

        // Debug
        debugFoldout = UnityEditor.EditorGUILayout.Foldout(debugFoldout, "Debug", true);
        if (debugFoldout)
        {
            UnityEditor.EditorGUI.indentLevel++;
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("showInfluenceRadius"));
            UnityEditor.EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif