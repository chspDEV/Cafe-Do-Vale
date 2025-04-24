using Tcp4.Resources.Scripts.Core;
using Tcp4.Resources.Scripts.Interfaces;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.CollisionCasters
{
    public class RaycastCollisionDetector : ICollisionDetector
    {
        public ICollisionResult Detect(Vector3 position, CollisionCheck check, Vector3 adjustedDirection)
        {
            Vector3 rayDirection = check.GetAdjustedDirection(adjustedDirection);
            if (Physics.Raycast(position, rayDirection, out RaycastHit hit, check.raycastDistance, check.layers))
            {
                var entity = hit.collider.GetComponent<BaseEntity>();
                return entity != null ? CollisionResultPool.Get(entity) : new RaycastResult(hit);
            }
            return null;
        }

        public void DrawGizmos(CollisionCheck check, Vector3 position, Vector3 facingDirection)
        {
            Vector3 rayDirection = check.GetAdjustedDirection(facingDirection);
            Gizmos.color = check.isColliding ? check.collisionColor : check.noCollisionColor;
            Gizmos.DrawLine(position, position + rayDirection * check.raycastDistance);
        }
    }
}