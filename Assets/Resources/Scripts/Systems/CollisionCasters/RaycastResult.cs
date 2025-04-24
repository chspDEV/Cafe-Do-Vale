using Tcp4.Resources.Scripts.Interfaces;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.CollisionCasters
{
    public class RaycastResult : ICollisionResult
    {
        public RaycastHit Hit { get; }
        public float Distance => Hit.distance;
        public RaycastResult(RaycastHit hit) => Hit = hit;
    }
}