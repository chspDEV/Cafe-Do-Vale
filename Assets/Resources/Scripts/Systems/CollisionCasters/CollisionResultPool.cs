using Tcp4.Resources.Scripts.Core;
using Tcp4.Resources.Scripts.Interfaces;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.CollisionCasters
{
    public static class CollisionResultPool
    {
        private static readonly ObjectPool<EntityCollisionResult> entityResultPool = 
            new ObjectPool<EntityCollisionResult>(() => new EntityCollisionResult());
        
        private static readonly ObjectPool<CollisionResult> collisionResultPool = 
            new ObjectPool<CollisionResult>(() => new CollisionResult());

        public static EntityCollisionResult Get(BaseEntity entity)
        {
            var result = entityResultPool.Get();
            result.SetEntity(entity);
            return result;
        }

        public static CollisionResult Get(Collider collider)
        {
            var result = collisionResultPool.Get();
            result.SetCollider(collider);
            return result;
        }

        public static void Release(ICollisionResult result)
        {
            switch (result)
            {
                case EntityCollisionResult entityResult:
                    entityResultPool.Release(entityResult);
                    break;
                case CollisionResult collisionResult:
                    collisionResultPool.Release(collisionResult);
                    break;
            }
        }
    }
}