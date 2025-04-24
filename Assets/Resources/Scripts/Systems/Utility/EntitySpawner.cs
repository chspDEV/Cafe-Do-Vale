using UnityEngine;

namespace Tcp4
{
    public class EntitySpawner : MonoBehaviour
    {
        [Header("Configurações de Spawn")]
        [SerializeField] private GameObject entityPrefab;
        [SerializeField] private Transform spawnPoint;

        public void SpawnEntity()
        {
            if (entityPrefab != null && spawnPoint != null)
            {
                Instantiate(entityPrefab, spawnPoint.position, transform.rotation);
            }
        }
    }
}
