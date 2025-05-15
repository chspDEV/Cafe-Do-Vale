using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tcp4
{
    public class CollectableSpawner : BaseInteractable
    {
        [TitleGroup("Configura��es de Spawn", "Configura��es principais para gera��o de colet�veis")]
        [BoxGroup("Configura��es de Spawn/�rea")]
        [PropertyRange(1, 2000)]
        [SerializeField] private float areaRange = 1f;

        [BoxGroup("Configura��es de Spawn/Quantidade")]
        [PropertyRange(1, 500)]
        [SerializeField] private int propsNumber = 1;

        [BoxGroup("Configura��es de Spawn/Valores")]
        [PropertyRange(1, 500)]
        [SerializeField] private int propMaxPrice = 5;

        [BoxGroup("Configura��es de Spawn/Camadas")]
        [SerializeField] private LayerMask avoidLayer;
        [SerializeField] private LayerMask groundLayer;

        [TitleGroup("Prefabs de Colet�veis", "Objetos colet�veis dispon�veis")]
        [SerializeField] private List<GameObject> collectablesList;

        [TitleGroup("Debug", "Configura��es de depura��o")]
        [BoxGroup("Debug/Visualiza��o")]
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