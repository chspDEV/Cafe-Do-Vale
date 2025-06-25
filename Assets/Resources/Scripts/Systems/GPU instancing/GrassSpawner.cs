using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Profiling;


[System.Serializable]
public struct LODLevel
{
    public float maxFOV; // FOV máximo para este LOD (ex: 30 = LOD0 ativa até FOV 30)
    public int instances; // Quantidade de matos neste LOD
    public Mesh mesh; // Mesh de detalhe (opcional: trocar mesh para LOD baixo)
    public Material material; // Material simplificado (opcional)
}



[RequireComponent(typeof(LODGroup))]
public class GrassSpawner : MonoBehaviour
{
    [Header("Grass Settings")]
    public Mesh grassMesh;
    public Material grassMaterial;
    public int maxInstances = 1000;
    public float areaSize = 50f;
    public LayerMask groundLayer;
    public LayerMask excludedLayers;
    public float checkRadius = 0.5f;
    public int framesToSpawn = 10; // Quadros para espalhar o spawn

    [Header("LOD by FOV")]
    public float[] lodFOVThresholds = { 30f, 40f, 45f }; // FOVs para LOD0, LOD1, LOD2
    public int[] lodInstanceCounts = { 1000, 500, 200 };  // Matos por LOD
    public Mesh[] lodMeshes;                             // Meshes simplificados (opcional)

    private Matrix4x4[] allMatrices;
    private Matrix4x4[] lodMatrices;
    private MaterialPropertyBlock props;
    [SerializeField] private CinemachineCamera cam;
    private int currentLOD = 0;
    private int spawnedInstances = 0; // Contador de matos já spawnados

    void Start()
    {
        if (cam == null) return;
        props = new MaterialPropertyBlock();
        allMatrices = new Matrix4x4[maxInstances];
    }

    void Update()
    {
        if (cam == null) return;

        // Spawn progressivo (se ainda não terminou)
        if (spawnedInstances < maxInstances)
        {
            SpawnGrassOverFrames();
        }

        // Atualiza LOD baseado no FOV
        UpdateLOD();

        // Renderiza os matos ativos
        RenderGrass();
    }

    void SpawnGrassOverFrames()
    {
        int instancesPerFrame = Mathf.CeilToInt((float)maxInstances / framesToSpawn);
        int endIndex = Mathf.Min(spawnedInstances + instancesPerFrame, maxInstances);

        for (int i = spawnedInstances; i < endIndex; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-areaSize / 2, areaSize / 2),
                0,
                Random.Range(-areaSize / 2, areaSize / 2)
            );

            if (Physics.Raycast(randomPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                if (!Physics.CheckSphere(hit.point, checkRadius, excludedLayers))
                {
                    allMatrices[i] = Matrix4x4.TRS(
                        hit.point,
                        Quaternion.Euler(0, Random.Range(0, 360f), 0),
                        Vector3.one * Random.Range(0.8f, 1.2f)
                    );
                }
            }
        }
        spawnedInstances = endIndex;
    }

    void UpdateLOD()
    {
        if (cam == null) return;

        float currentFOV = cam.Lens.FieldOfView;
        for (int i = 0; i < lodFOVThresholds.Length; i++)
        {
            if (currentFOV <= lodFOVThresholds[i])
            {
                currentLOD = i;
                break;
            }
        }

        // Pega um subconjunto das matrizes (baseado no LOD)
        int instancesToRender = Mathf.Min(spawnedInstances, lodInstanceCounts[currentLOD]);
        lodMatrices = new Matrix4x4[instancesToRender];
        System.Array.Copy(allMatrices, lodMatrices, instancesToRender);
    }

    void RenderGrass()
    {
        if (lodMatrices == null || lodMatrices.Length == 0) return;

        Mesh meshToRender = lodMeshes != null && currentLOD < lodMeshes.Length ? lodMeshes[currentLOD] : grassMesh;
        Graphics.DrawMeshInstanced(
            meshToRender,
            0,
            grassMaterial, // Mantém o material original (ou use lodMaterials se tiver)
            lodMatrices,
            lodMatrices.Length,
            props,
            UnityEngine.Rendering.ShadowCastingMode.Off,
            false,
            gameObject.layer
        );
    }
}