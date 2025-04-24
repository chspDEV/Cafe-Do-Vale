using Tcp4.Resources.Scripts.Core;
using Tcp4.Resources.Scripts.Interfaces;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.CollisionCasters
{
    public class BoxCollisionDetector : ICollisionDetector
    {
        private static readonly Collider[] colliderBuffer = new Collider[8]; // Buffer reutilizável

        public ICollisionResult Detect(Vector3 position, CollisionCheck check, Vector3 adjustedDirection)
        {
            int hitCount = Physics.OverlapBoxNonAlloc(
                position,
                check.scale * 0.5f,
                colliderBuffer,
                Quaternion.identity,
                check.layers
            );

            if (hitCount > 0)
            {
                var entity = colliderBuffer[0].GetComponent<BaseEntity>();
                return entity != null ? CollisionResultPool.Get(entity) : CollisionResultPool.Get(colliderBuffer[0]);
            }
            return null;
        }

        public void DrawGizmos(CollisionCheck check, Vector3 position, Vector3 facingDirection)
        {
            Gizmos.color = check.isColliding ? check.collisionColor : check.noCollisionColor;
            Gizmos.DrawWireCube(position, check.scale);
        }
    }
}