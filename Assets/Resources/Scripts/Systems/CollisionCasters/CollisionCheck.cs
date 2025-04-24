using System;
using Tcp4.Resources.Scripts.Interfaces;
using Tcp4.Resources.Scripts.Types;
using UnityEngine;
using UnityEngine.Events;

namespace Tcp4.Resources.Scripts.Systems.CollisionCasters
{
    [Serializable]
    public class CollisionCheck
    {
        public string name;
        public float radius = 0.5f;
        public Vector3 offset = Vector3.zero;
        public Vector3 direction = Vector3.forward;
        public Vector3 scale = Vector3.one;
        public LayerMask layers;
        public Color collisionColor = Color.green;
        public Color noCollisionColor = Color.red;
        public CollisionType collisionType = CollisionType.Sphere;
        public float raycastDistance = 1.0f;
        public bool flipWithSprite;
        
        [HideInInspector] public bool isColliding;
        [HideInInspector] public ICollisionResult CollisionResult;

        [SerializeField] private bool useEvents;
        public UnityEvent<ICollisionResult> onCollisionEnter;
        public UnityEvent onCollisionExit;
        private bool lastCollisionState;
        
        public Vector3 GetCheckPosition(Transform transform, Vector3 facingDirection)
        {
            return transform.position +
                   transform.right * offset.x +
                   Vector3.up * offset.y +
                   transform.forward * offset.z;
        }
    
        public Vector3 GetAdjustedDirection(Vector3 facingDirection)
        {
            return direction.normalized;
        }
        
        public void NotifyCollisionChange()
        {
            if(!useEvents) return;
            if (isColliding && !lastCollisionState)
            {
                onCollisionEnter?.Invoke(CollisionResult);
            }
            else if (!isColliding && lastCollisionState)
            {
                onCollisionExit?.Invoke();
            }
            lastCollisionState = isColliding;
        }
    }
}