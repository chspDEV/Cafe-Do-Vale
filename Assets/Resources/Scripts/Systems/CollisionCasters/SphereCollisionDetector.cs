using Tcp4.Resources.Scripts.Core;
using Tcp4.Resources.Scripts.Interfaces;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.CollisionCasters
{
    public class SphereCollisionDetector : ICollisionDetector
    {
        private static readonly Collider[] colliderBuffer = new Collider[8]; 

        public ICollisionResult Detect(Vector3 position, CollisionCheck check, Vector3 adjustedDirection)
        {
            Vector3 scaledRadius = new Vector3(
                check.radius * check.scale.x,
                check.radius * check.scale.y,
                check.radius * check.scale.z
            );
            int hitCount = Physics.OverlapSphereNonAlloc(
                position,
                Mathf.Max(scaledRadius.x, scaledRadius.y, scaledRadius.z),
                colliderBuffer,
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
            Gizmos.DrawWireSphere(position, Mathf.Max(check.scale.x, check.scale.y, check.scale.z) * check.radius);
        }
    }
}