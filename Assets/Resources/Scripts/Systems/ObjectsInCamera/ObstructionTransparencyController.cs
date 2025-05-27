// obstruction_transparency_controller_urp.cs
using UnityEngine;

public class ObstructionTransparencyController : MonoBehaviour // Renomeie o arquivo para ObstructionTransparencyController.cs se usar este
{
    public Transform playerTarget;
    public Camera mainCamera;
    public float maxDistance = 10f;
    public LayerMask obstructionLayer;
    [Range(0f, 1f)]
    public float transparencyAmount = 0.3f; // Valor padrao para a transparencia

    private Renderer lastObstructedRenderer;

    // Variaveis para guardar o estado original do material URP/Lit
    private Color originalBaseColor;
    private float originalSurfaceType; // 0 = Opaque, 1 = Transparent
    private float originalSrcBlend;
    private float originalDstBlend;
    private int originalZWrite;
    private bool originalAlphaClip;
    private string[] originalShaderKeywords;
    private int originalRenderQueue;
    private bool originalMaterialStateSaved = false;

    void Update()
    {
        if (playerTarget == null || mainCamera == null) return;

        Vector3 directionToTarget = playerTarget.position - mainCamera.transform.position;
        float distanceToTarget = directionToTarget.magnitude;

        RaycastHit hit;
        if (Physics.Raycast(mainCamera.transform.position, directionToTarget.normalized, out hit, Mathf.Min(distanceToTarget, maxDistance), obstructionLayer))
        {
            if (hit.collider.transform != playerTarget)
            {
                Renderer hitRenderer = hit.collider.GetComponent<Renderer>();
                if (hitRenderer != null)
                {
                    if (lastObstructedRenderer != hitRenderer)
                    {
                        RestoreLastObstructedObject(); // Restaura o anterior

                        lastObstructedRenderer = hitRenderer;
                        Material instancedMaterial = hitRenderer.material; // Pega/Cria instancia do material

                        // Salva o estado original do material da instancia ANTES de qualquer modificacao
                        // Isso garante que se o mesmo objeto for obstruido multiplas vezes,
                        // comecamos do seu estado "real" antes da nossa primeira intervencao.
                        // Se quisermos sempre restaurar ao estado do *asset* original, precisariamos de logica adicional
                        // ou garantir que 'originalSharedMaterial' seja usado para ler esses valores.
                        // Para este exemplo, vamos assumir que o estado atual do 'instancedMaterial' antes da primeira
                        // modificacao por este script eh o que queremos salvar.
                        if (!originalMaterialStateSaved) // Apenas salva se ainda nao temos um estado salvo
                        {
                            // Para URP/Lit, a cor principal e frequentemente '_BaseColor'
                            if (instancedMaterial.HasProperty("_BaseColor"))
                            {
                                originalBaseColor = instancedMaterial.GetColor("_BaseColor");
                            }
                            else
                            {
                                originalBaseColor = instancedMaterial.color; // Fallback para .color
                            }

                            if (instancedMaterial.HasProperty("_Surface"))
                                originalSurfaceType = instancedMaterial.GetFloat("_Surface");

                            if (instancedMaterial.HasProperty("_SrcBlend"))
                                originalSrcBlend = instancedMaterial.GetFloat("_SrcBlend");

                            if (instancedMaterial.HasProperty("_DstBlend"))
                                originalDstBlend = instancedMaterial.GetFloat("_DstBlend");

                            if (instancedMaterial.HasProperty("_ZWrite"))
                                originalZWrite = instancedMaterial.GetInt("_ZWrite");
                            else
                                originalZWrite = 1; // Default para opaco

                            if (instancedMaterial.HasProperty("_AlphaClip"))
                                originalAlphaClip = instancedMaterial.GetFloat("_AlphaClip") > 0.5f;

                            originalShaderKeywords = instancedMaterial.shaderKeywords; // Salva todos os keywords atuais
                            originalRenderQueue = instancedMaterial.renderQueue;
                            originalMaterialStateSaved = true;
                        }


                        // Aplica configuracoes para transparencia no URP/Lit
                        if (instancedMaterial.HasProperty("_Surface"))
                            instancedMaterial.SetFloat("_Surface", 1.0f); // 1.0f para Transparent

                        if (instancedMaterial.HasProperty("_SrcBlend"))
                            instancedMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);

                        if (instancedMaterial.HasProperty("_DstBlend"))
                            instancedMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                        instancedMaterial.SetInt("_ZWrite", 0); // Para transparencia, ZWrite geralmente eh off

                        // Keywords importantes para URP/Lit transparente
                        instancedMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                        instancedMaterial.DisableKeyword("_SURFACE_TYPE_OPAQUE");
                        instancedMaterial.DisableKeyword("_ALPHATEST_ON"); // Desabilitar Alpha Clipping
                        instancedMaterial.EnableKeyword("_ALPHABLEND_ON"); // Habilitar Alpha Blending
                        // instancedMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON"); // Alternativa se o seu shader usa pre-multiplied alpha

                        instancedMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                        Color transparentColor;
                        if (instancedMaterial.HasProperty("_BaseColor"))
                        {
                            transparentColor = instancedMaterial.GetColor("_BaseColor");
                        }
                        else
                        {
                            transparentColor = instancedMaterial.color;
                        }
                        transparentColor.a = transparencyAmount;

                        if (instancedMaterial.HasProperty("_BaseColor"))
                        {
                            instancedMaterial.SetColor("_BaseColor", transparentColor);
                        }
                        else
                        {
                            instancedMaterial.color = transparentColor;
                        }

                        // Debug
                        // Debug.Log($"[URP Transparency] Applied to {hitRenderer.gameObject.name}. Target Alpha: {transparencyAmount}, Actual Alpha: {transparentColor.a}");

                    }
                    return; // Retorna para nao restaurar imediatamente se o mesmo objeto continua sendo o hit
                }
            }
        }
        // Se nenhum objeto (diferente do player) esta obstruindo, ou o raio nao atingiu nada relevante
        RestoreLastObstructedObject();
    }

    void RestoreLastObstructedObject()
    {
        if (lastObstructedRenderer != null && originalMaterialStateSaved)
        {
            Material matInstance = lastObstructedRenderer.material;

            // Restaura a cor base com seu alfa original
            if (matInstance.HasProperty("_BaseColor"))
            {
                matInstance.SetColor("_BaseColor", originalBaseColor);
            }
            else
            {
                matInstance.color = originalBaseColor;
            }

            // Restaura configuracoes de superficie e blending do URP
            if (matInstance.HasProperty("_Surface"))
                matInstance.SetFloat("_Surface", originalSurfaceType);

            if (matInstance.HasProperty("_SrcBlend"))
                matInstance.SetFloat("_SrcBlend", originalSrcBlend);

            if (matInstance.HasProperty("_DstBlend"))
                matInstance.SetFloat("_DstBlend", originalDstBlend);

            matInstance.SetInt("_ZWrite", originalZWrite);

            if (matInstance.HasProperty("_AlphaClip"))
                matInstance.SetFloat("_AlphaClip", originalAlphaClip ? 1.0f : 0.0f);

            matInstance.shaderKeywords = originalShaderKeywords; // Restaura todos os keywords originais
            matInstance.renderQueue = originalRenderQueue;

            lastObstructedRenderer = null;
            originalMaterialStateSaved = false; // Permite salvar o estado do proximo objeto
            Debug.Log("[URP Transparency] Restored material for " + matInstance.name);
        }
    }

    void OnDisable()
    {
        RestoreLastObstructedObject();
    }

    void OnDestroy()
    {
        RestoreLastObstructedObject();
    }
}