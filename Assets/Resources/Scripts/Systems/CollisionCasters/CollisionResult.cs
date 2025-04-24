using Tcp4.Resources.Scripts.Interfaces;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.CollisionCasters
{
    public class CollisionResult : ICollisionResult
    {
        private Collider collider;
        public Collider Collider => collider;

        public void SetCollider(Collider c)
        {
            this.collider = c;
        }

        public void Reset()
        {
            collider = null;
        }
    }
}