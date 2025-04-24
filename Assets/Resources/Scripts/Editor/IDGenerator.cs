using System.Collections.Generic;
using System.Linq;
using Tcp4.Resources.Scripts.Core;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Editor
{
    [CreateAssetMenu(menuName = "Entity/ID Generator")]
    public class IDGenerator : ScriptableObject
    {
        [SerializeField] private List<BaseEntitySO> allEntities = new List<BaseEntitySO>();
        
        public IReadOnlyList<BaseEntitySO> AllEntities => allEntities;

        public int GenerateID(BaseEntitySO entity)
        {
            if (entity == null) return -1;

            if (!allEntities.Contains(entity))
            {
                int newId = allEntities.Count > 0 ? 
                    allEntities.Max(e => e.Id) + 1 : 1;
                
                entity.Id = (byte)newId;
                allEntities.Add(entity);
            }

            return entity.Id;
        }

        public void SwapIDs(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= allEntities.Count || 
                indexB < 0 || indexB >= allEntities.Count)
                return;

            int tempId = allEntities[indexA].Id;
            allEntities[indexA].Id = allEntities[indexB].Id;
            allEntities[indexB].Id = (byte)tempId;

            var temp = allEntities[indexA];
            allEntities[indexA] = allEntities[indexB];
            allEntities[indexB] = temp;
        }

        public void RemoveEntity(BaseEntitySO entity)
        {
            allEntities.Remove(entity);
        }

        public void Clear()
        {
            allEntities.Clear();
        }
    }
}