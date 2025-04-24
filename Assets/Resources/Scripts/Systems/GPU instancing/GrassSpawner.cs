using UnityEngine;

public class GrassSpawner : MonoBehaviour {
    public GameObject pfGrass;
    public int instances = 1000;
    public float areaSize = 50f;
    public LayerMask groundLayer; 
    public LayerMask excludedLayers; 
    public float checkRadius = 0.5f;

    private Matrix4x4[] matrices;
    private MaterialPropertyBlock props;

    void Start() {
        matrices = new Matrix4x4[instances];
        props = new MaterialPropertyBlock();

        // Encontrando todas pos de gramas
        for (int i = 0; i < instances; i++) {
            Vector3 randomPos = new Vector3(
                Random.Range(-areaSize/2, areaSize/2),
                0,
                Random.Range(-areaSize/2, areaSize/2)
            );

            // ENCONTRANDO ALTURA DO CHAO
            Ray ray = new Ray(new Vector3(randomPos.x, 10f, randomPos.z), Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit groundHit, Mathf.Infinity, groundLayer)) {
                Vector3 spawnPos = groundHit.point;

                // Vendo se esta bloqueado
                bool isBlocked = Physics.CheckSphere(
                    spawnPos,
                    checkRadius,
                    excludedLayers
                );

                if (!isBlocked) {
                    matrices[i] = Matrix4x4.TRS(
                        spawnPos,
                        Quaternion.Euler(0, Random.Range(0, 360), 0),
                        Vector3.one * Random.Range(0.8f, 1.2f)
                    );
                }
            }
        }
    }

    void Update() {

        //RENDERIZANDO MINHAS GRAMAS
        Graphics.DrawMeshInstanced(
            pfGrass.GetComponent<MeshFilter>().sharedMesh,
            0,
            pfGrass.GetComponent<MeshRenderer>().sharedMaterial,
            matrices,
            instances,
            props
        );
    }

    // Debug das posições bloqueadas (opcional)
    void OnDrawGizmos() {
        if (matrices == null) return;
        
        Gizmos.color = Color.green;
        foreach (var matrix in matrices) {
            Vector3 pos = matrix.GetPosition();
            Gizmos.DrawWireSphere(pos, checkRadius);
        }
    }
}