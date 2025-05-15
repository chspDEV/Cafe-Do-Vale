using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tcp4
{
    public class CollectableSpawner : BaseInteractable
    {
        [TitleGroup("Configurações de Spawn", "Configurações principais para geração de coletáveis")]
        [BoxGroup("Configurações de Spawn/Área")]
        [PropertyRange(1, 2000)]
        [SerializeField] private float areaRange = 1f;

        [BoxGroup("Configurações de Spawn/Quantidade")]
        [PropertyRange(1, 500)]
        [SerializeField] private int propsNumber = 1;

        [BoxGroup("Configurações de Spawn/Valores")]
        [PropertyRange(1, 500)]
        [SerializeField] private int propMaxPrice = 5;

        [BoxGroup("Configurações de Spawn/Camadas")]
        [SerializeField] private LayerMask avoidLayer;
        [SerializeField] private LayerMask groundLayer;

        [TitleGroup("Prefabs de Coletáveis", "Objetos coletáveis disponíveis")]
        [SerializeField] private List<GameObject> collectablesList;

        [TitleGroup("Debug", "Configurações de depuração")]
        [BoxGroup("Debug/Visualização")]
        [ToggleLeft]
        [SerializeField] private bool showSpawnArea = true;

        public override void Start()
        {
            base.Start();
            SpawnCollectables();
        }

        public void SpawnCollectables()
        {
            if (collectablesList.Count == 0)
            {
                Debug.LogError("Lista de coletaveis vazia!");
                return;
            }

            for (int i = 0; i < propsNumber; i++)
            {
                GameObject selectedPrefab = collectablesList[Random.Range(0, collectablesList.Count)];
                Vector3 spawnPosition = GetValidSpawnPosition();

                if (spawnPosition != Vector3.zero)
                {
                    GameObject collectableInstance = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
                    Collectable collectable = collectableInstance.GetComponent<Collectable>();
                    if (collectable != null)
                    {
                        collectable.money = Random.Range(1, propMaxPrice + 1);
                    }
                }
                else
                {
                    Debug.LogWarning("Falha ao spawnar coletavel.");
                }
            }
        }

        private Vector3 GetValidSpawnPosition(int maxAttempts = 30)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * areaRange;
                Vector3 raycastOrigin = transform.position + new Vector3(randomCircle.x, 100f, randomCircle.y);

                if (Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, 200f, groundLayer))
                {
                    Vector3 potentialPosition = hit.point;

                    Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.5f, avoidLayer);
                    if (colliders.Length == 0)
                    {
                        return potentialPosition;
                    }
                }
            }
            return Vector3.zero;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, areaRange);
        }
    }
}