using UnityEngine;
using System.Collections.Generic;

public class ObstructionManager : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform playerTarget;
    public Camera mainCamera;
    public float maxDistance = 10f;
    public LayerMask obstructionLayer = -1;

    [Header("X-Ray Shader Settings")]
    public Shader xrayShader;
    public Color xrayColor = new Color(0.2f, 1f, 1f, 0.8f);
    [Range(0.1f, 8f)]
    public float fresnelPower = 2.0f;
    [Range(0f, 1f)]
    public float thickness = 0.6f;
    [Range(0.01f, 1f)]
    public float edgeSoftness = 0.15f;
    [Range(0f, 0.1f)]
    public float depthBias = 0.001f;
    [Range(0f, 2f)]
    public float xrayIntensity = 1.0f;

    [Header("Performance")]
    public int maxRaycastHits = 20;
    [Range(0.01f, 0.5f)]
    public float updateFrequency = 0.1f;

    private Dictionary<Renderer, MaterialData> rendererData = new Dictionary<Renderer, MaterialData>();
    private HashSet<Renderer> currentObstructed = new HashSet<Renderer>();
    private float lastUpdateTime;

    [System.Serializable]
    private struct MaterialData
    {
        public Material[] originalMaterials;
        public Material[] xrayMaterials;
    }

    void Start()
    {

        if (xrayShader == null)
            xrayShader = Shader.Find("Custom/XRayURP");

        if (xrayShader == null)
        {
            Debug.LogError("X-Ray shader not found! Make sure 'Custom/XRayURP' shader is in your project.");
            enabled = false;
        }
    }

    void Update()
    {
        if (Time.time - lastUpdateTime < updateFrequency)
            return;

        lastUpdateTime = Time.time;

        if (!ValidateComponents()) return;

        UpdateXRayEffect();
    }

    bool ValidateComponents()
    {
        return playerTarget != null && mainCamera != null && xrayShader != null;
    }

    void UpdateXRayEffect()
    {
        HashSet<Renderer> newObstructed = new HashSet<Renderer>();

        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 targetPos = playerTarget.position;
        Vector3 direction = (targetPos - cameraPos).normalized;
        float distance = Vector3.Distance(cameraPos, targetPos);

        // Use RaycastAll to find obstructing objects
        RaycastHit[] hits = Physics.RaycastAll(cameraPos, direction,
            Mathf.Min(distance, maxDistance), obstructionLayer);

        // Limit hits for performance
        int hitCount = Mathf.Min(hits.Length, maxRaycastHits);

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hits[i];

            // Skip if it's the target itself
            if (hit.collider.transform == playerTarget)
                continue;

            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && renderer.enabled)
            {
                newObstructed.Add(renderer);

                // Apply X-Ray effect if not already applied
                if (!rendererData.ContainsKey(renderer))
                {
                    ApplyXRayEffect(renderer);
                }
                else
                {
                    // Update X-Ray properties
                    UpdateXRayProperties(renderer);
                }
            }
        }

        // Restore objects that are no longer obstructed
        List<Renderer> toRestore = new List<Renderer>();
        foreach (var renderer in currentObstructed)
        {
            if (!newObstructed.Contains(renderer))
            {
                toRestore.Add(renderer);
            }
        }

        foreach (var renderer in toRestore)
        {
            RestoreOriginalMaterials(renderer);
        }

        currentObstructed = newObstructed;
    }

    void ApplyXRayEffect(Renderer renderer)
    {
        if (renderer == null || renderer.sharedMaterials == null)
            return;

        Material[] originalMaterials = renderer.sharedMaterials;
        Material[] xrayMaterials = new Material[originalMaterials.Length];

        for (int i = 0; i < originalMaterials.Length; i++)
        {
            Material original = originalMaterials[i];
            Material xrayMaterial = new Material(xrayShader);

            CopyMaterialProperties(original, xrayMaterial);
            SetXRayProperties(xrayMaterial);

            xrayMaterials[i] = xrayMaterial;
        }

        MaterialData data = new MaterialData
        {
            originalMaterials = originalMaterials,
            xrayMaterials = xrayMaterials
        };

        rendererData[renderer] = data;
        renderer.materials = xrayMaterials;
    }

    void CopyMaterialProperties(Material source, Material target)
    {
        if (source == null) return;

        // Copy base properties
        CopyTextureProperty(source, target, "_BaseMap", "_MainTex");
        CopyColorProperty(source, target, "_BaseColor", "_Color");

        // Copy texture properties
        CopyTextureProperty(source, target, "_NormalMap", "_BumpMap");
        CopyTextureProperty(source, target, "_MetallicGlossMap");
        CopyTextureProperty(source, target, "_OcclusionMap");
        CopyTextureProperty(source, target, "_EmissionMap");

        // Copy color properties
        CopyColorProperty(source, target, "_EmissionColor");

        // Copy float properties
        CopyFloatProperty(source, target, "_Metallic");
        CopyFloatProperty(source, target, "_Smoothness");
        CopyFloatProperty(source, target, "_Glossiness", "_Smoothness");
    }

    void CopyTextureProperty(Material source, Material target, string targetProp, string fallbackProp = null)
    {
        if (source.HasProperty(targetProp))
        {
            target.SetTexture(targetProp, source.GetTexture(targetProp));
        }
        else if (!string.IsNullOrEmpty(fallbackProp) && source.HasProperty(fallbackProp))
        {
            target.SetTexture(targetProp, source.GetTexture(fallbackProp));
        }
    }

    void CopyColorProperty(Material source, Material target, string targetProp, string fallbackProp = null)
    {
        if (source.HasProperty(targetProp))
        {
            target.SetColor(targetProp, source.GetColor(targetProp));
        }
        else if (!string.IsNullOrEmpty(fallbackProp) && source.HasProperty(fallbackProp))
        {
            target.SetColor(targetProp, source.GetColor(fallbackProp));
        }
    }

    void CopyFloatProperty(Material source, Material target, string targetProp, string fallbackProp = null)
    {
        if (source.HasProperty(targetProp))
        {
            target.SetFloat(targetProp, source.GetFloat(targetProp));
        }
        else if (!string.IsNullOrEmpty(fallbackProp) && source.HasProperty(fallbackProp))
        {
            target.SetFloat(targetProp, source.GetFloat(fallbackProp));
        }
    }

    void SetXRayProperties(Material material)
    {
        material.SetColor("_XRayColor", xrayColor);
        material.SetFloat("_FresnelPower", fresnelPower);
        material.SetFloat("_Thickness", thickness);
        material.SetFloat("_EdgeSoftness", edgeSoftness);
        material.SetFloat("_DepthBias", depthBias);
        material.SetFloat("_XRayIntensity", xrayIntensity);
    }

    void UpdateXRayProperties(Renderer renderer)
    {
        if (!rendererData.TryGetValue(renderer, out MaterialData data))
            return;

        foreach (Material material in data.xrayMaterials)
        {
            if (material != null)
            {
                SetXRayProperties(material);
            }
        }
    }

    void RestoreOriginalMaterials(Renderer renderer)
    {
        if (!rendererData.TryGetValue(renderer, out MaterialData data))
            return;

        // Restore original materials
        if (renderer != null && data.originalMaterials != null)
        {
            renderer.materials = data.originalMaterials;
        }

        // Clean up X-Ray materials
        if (data.xrayMaterials != null)
        {
            foreach (Material material in data.xrayMaterials)
            {
                if (material != null)
                {
                    DestroyImmediate(material);
                }
            }
        }

        rendererData.Remove(renderer);
    }

    void OnDisable()
    {
        RestoreAllMaterials();
    }

    void OnDestroy()
    {
        RestoreAllMaterials();
    }

    void RestoreAllMaterials()
    {
        foreach (var renderer in new List<Renderer>(rendererData.Keys))
        {
            RestoreOriginalMaterials(renderer);
        }

        rendererData.Clear();
        currentObstructed.Clear();
    }

    // Gizmos for debugging
    void OnDrawGizmosSelected()
    {
        if (playerTarget != null && mainCamera != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 start = mainCamera.transform.position;
            Vector3 end = playerTarget.position;
            Gizmos.DrawLine(start, end);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(end, 0.2f);
        }
    }
}