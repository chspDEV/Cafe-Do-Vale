using System.Collections;
using UnityEngine;

public class GrassCell
{
    public bool IsActive { get; private set; }

    private GrassManager manager;
    private Vector2Int gridPosition;
    private Vector3 worldPosition;
    private Matrix4x4[] matrices;
    private bool isSpawned = false;
    private Coroutine spawnCoroutine;
    private MaterialPropertyBlock props;

    public GrassCell(GrassManager manager, Vector2Int gridPosition)
    {
        this.manager = manager;
        this.gridPosition = gridPosition;
        this.worldPosition = new Vector3(gridPosition.x * manager.cellSize, 0, gridPosition.y * manager.cellSize);
        this.matrices = new Matrix4x4[manager.instancesPerCell];
        this.props = new MaterialPropertyBlock();
    }

    public void Activate()
    {
        IsActive = true;
        if (!isSpawned)
        {
            spawnCoroutine = manager.StartCoroutine(SpawnGrassCoroutine());
        }
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private IEnumerator SpawnGrassCoroutine()
    {
        isSpawned = true;
        int spawnedCount = 0;
        int instancesPerFrame = 100;

        while (spawnedCount < manager.instancesPerCell)
        {
            int limit = Mathf.Min(spawnedCount + instancesPerFrame, manager.instancesPerCell);
            for (int i = spawnedCount; i < limit; i++)
            {
                float halfCell = manager.cellSize / 2f;
                Vector3 randomPos = worldPosition + new Vector3(
                    Random.Range(-halfCell, halfCell),
                    0,
                    Random.Range(-halfCell, halfCell)
                );

                if (Physics.Raycast(randomPos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f, manager.groundLayer))
                {
                    if (!Physics.CheckSphere(hit.point, manager.checkRadius, manager.excludedLayers))
                    {
                        Vector3 position = hit.point;

                        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                        if (manager.alignToGroundNormal)
                        {
                            rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * rotation;
                        }

                        float tiltX = Random.Range(-manager.maxTiltAngle, manager.maxTiltAngle);
                        float tiltZ = Random.Range(-manager.maxTiltAngle, manager.maxTiltAngle);
                        rotation *= Quaternion.Euler(tiltX, 0, tiltZ);

                        Vector3 scale = new Vector3(
                            Random.Range(manager.widthScaleRange.x, manager.widthScaleRange.y),
                            Random.Range(manager.heightScaleRange.x, manager.heightScaleRange.y),
                            Random.Range(manager.widthScaleRange.x, manager.widthScaleRange.y)
                        );

                        matrices[i] = Matrix4x4.TRS(position, rotation, scale);
                    }
                }
            }
            spawnedCount = limit;
            yield return null;
        }
        spawnCoroutine = null;
    }

    public void Draw(Camera cam)
    {
        if (!isSpawned || matrices == null || matrices.Length == 0) return;

        float distance = Vector3.Distance(cam.transform.position, worldPosition);
        int lodIndex = GetLODIndex(distance);

        if (lodIndex >= manager.lodDistances.Length) return;

        int instancesToRender = (int)(manager.instancesPerCell * manager.lodInstanceRatios[lodIndex]);
        if (instancesToRender == 0) return;

        Mesh meshToRender = manager.grassMesh;
        if (manager.lodMeshes != null && lodIndex < manager.lodMeshes.Length && manager.lodMeshes[lodIndex] != null)
        {
            meshToRender = manager.lodMeshes[lodIndex];
        }

        Graphics.DrawMeshInstanced(
            meshToRender, 0, manager.grassMaterial,
            matrices, instancesToRender, props,
            UnityEngine.Rendering.ShadowCastingMode.Off,
            false, manager.gameObject.layer
        );
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
        return manager.lodDistances.Length;
    }
}