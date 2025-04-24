using Tcp4.Resources.Scripts.Systems.CollisionCasters;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Interfaces
{
    public interface ICollisionDetector
    {
        ICollisionResult Detect(Vector3 position, CollisionCheck check, Vector3 adjustedDirection);
        void DrawGizmos(CollisionCheck check, Vector3 position, Vector3 facingDirection);
    }
}