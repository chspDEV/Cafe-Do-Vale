using System.Collections;
using UnityEngine;

public class GrassCell
{
    public bool IsActive { get; private set; }

    private GrassManager manager;
    private Vector2Int cellCoordinate;
    private Matrix4x4[] matrices;
    private bool isInitialized = false;
    private Coroutine generationCoroutine;

    public GrassCell(GrassManager manager, Vector2Int cellCoordinate)
    {
        this.manager = manager;
        this.cellCoordinate = cellCoordinate;
        this.matrices = new Matrix4x4[manager.instancesPerCell];
    }

    public void Activate()
    {
        IsActive = true;
        if (!isInitialized)
        {
            generationCoroutine = manager.StartCoroutine(GenerateGrassCoroutine());
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        if (generationCoroutine != null)
        {
            manager.StopCoroutine(generationCoroutine);
            generationCoroutine = null;
        }
    }

    private IEnumerator GenerateGrassCoroutine()
    {
        isInitialized = true;
        int spawnedCount = 0;
        int instancesPerFrame = 100;

        while (spawnedCount < manager.instancesPerCell)
        {
            int limit = Mathf.Min(spawnedCount + instancesPerFrame, manager.instancesPerCell);
            for (int i = spawnedCount; i < limit; i++)
            {
                float randomX = (cellCoordinate.x + Random.value) * manager.cellSize;
                float randomZ = (cellCoordinate.y + Random.value) * manager.cellSize;

                Vector3 origin = new Vector3(randomX, 1000f, randomZ);

                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 2000f, manager.groundLayer))
                {
                    if (!Physics.CheckSphere(hit.point, manager.checkRadius, manager.excludedLayers))
                    {
                        Vector3 position = hit.point;
                        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                        Quaternion tiltRotation = Quaternion.Euler(
                            Random.Range(-manager.maxTiltAngle, manager.maxTiltAngle),
                            0,
                            Random.Range(-manager.maxTiltAngle, manager.maxTiltAngle)
                        );

                        Quaternion rotation = manager.alignToGroundNormal
                            ? Quaternion.FromToRotation(Vector3.up, hit.normal) * randomRotation * tiltRotation
                            : randomRotation * tiltRotation;

                        Vector3 scale = new Vector3(
                            Random.Range(manager.widthScaleRange.x, manager.widthScaleRange.y),
                            Random.Range(manager.heightScaleRange.x, manager.heightScaleRange.y),
                            Random.Range(manager.widthScaleRange.x, manager.widthScaleRange.y)
                        );

                        matrices[i] = Matrix4x4.TRS(position, rotation, scale);
                    }
                    else
                    {
                        matrices[i] = Matrix4x4.zero; // Esconde a grama se estiver em uma área excluída
                    }
                }
                else
                {
                    matrices[i] = Matrix4x4.zero; // Esconde a grama se não encontrar o chão
                }
            }
            spawnedCount = limit;
            yield return null;
        }
        generationCoroutine = null;
    }

    public void Draw(Camera camera)
    {
        if (!isInitialized || matrices == null || matrices.Length == 0) return;

        Vector3 cellCenter = new Vector3(
            (cellCoordinate.x + 0.5f) * manager.cellSize,
            camera.transform.position.y,
            (cellCoordinate.y + 0.5f) * manager.cellSize
        );
        float distance = Vector3.Distance(camera.transform.position, cellCenter);

        int lodIndex = GetLODIndex(distance);
        if (lodIndex < 0) return; // Não renderiza se estiver além da distância máxima

        Mesh meshToDraw = (manager.lodMeshes != null && lodIndex < manager.lodMeshes.Length && manager.lodMeshes[lodIndex] != null)
            ? manager.lodMeshes[lodIndex]
            : manager.grassMesh;

        float instanceRatio = (manager.lodInstanceRatios != null && lodIndex < manager.lodInstanceRatios.Length)
            ? manager.lodInstanceRatios[lodIndex]
            : 1.0f;

        int instancesToDraw = (int)(matrices.Length * instanceRatio);

        if (instancesToDraw > 0)
        {
            Graphics.DrawMeshInstanced(
                meshToDraw, 0, manager.grassMaterial, matrices, instancesToDraw,
                null, UnityEngine.Rendering.ShadowCastingMode.Off, false
            );
        }
    }

    private int GetLODIndex(float distance)
    {
        for (int i = 0; i < manager.lodDistances.Length; i++)
        {
            if (distance <= manager.lodDistances[i])
            {
                return i;
            }
        }
        // Retorna -1 se a distância for maior que todas as distâncias de LOD
        return -1;
    }
}