using System.Collections.Generic;
using GameResources.Resources.Scripts.Systems.Interaction;
using Tcp4.Resources.Scripts.Interfaces;
using Tcp4.Resources.Scripts.Systems.Interaction;
using Tcp4.Resources.Scripts.Types;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.CollisionCasters
{
    public class CollisionComponent : MonoBehaviour
    {
        [SerializeField] private List<CollisionCheck> collisionChecks = new List<CollisionCheck>();
        public List<CollisionCheck> GetCollisionChecks() => collisionChecks;

        private Dictionary<CollisionType, ICollisionDetector> collisionDetectors;
        private Vector3 FacingDirection => transform.forward;

        private void Awake()
        {
            InitializeCollisionDetectors();
        }
        
        private void Start()
        {
            foreach (var check in collisionChecks)
            {
                if (check.name == "Interact")
                {
                    ConfigureInteractions(check);
                }
            }
        }
        
        private void InitializeCollisionDetectors()
        {
            collisionDetectors = new Dictionary<CollisionType, ICollisionDetector>
            {
                { CollisionType.Sphere, new SphereCollisionDetector() },
                { CollisionType.Raycast, new RaycastCollisionDetector() },
                { CollisionType.Box, new BoxCollisionDetector() }
            };
        }

        private void FixedUpdate()
        {
            UpdateCollisionStates();
        }

        private void UpdateCollisionStates()
        {
            Vector3 currentFacingDirection = FacingDirection;
            
            foreach (var check in collisionChecks)
            {
                if (collisionDetectors.TryGetValue(check.collisionType, out var detector))
                {
                    Vector3 checkPosition = check.GetCheckPosition(transform, currentFacingDirection);
                    Vector3 adjustedDirection = check.GetAdjustedDirection(currentFacingDirection);
                    check.CollisionResult = detector.Detect(checkPosition, check, adjustedDirection);
                    check.isColliding = check.CollisionResult != null;
                    check.NotifyCollisionChange();
                }
            }
        }

        public bool IsColliding<T>(string checkName, out T result) where T : class, ICollisionResult
        {
            result = null;
            var check = collisionChecks.Find(c => c.name == checkName);
            if (check != null && check.CollisionResult is T typedResult)
            {
                result = typedResult;
                return check.isColliding;
            }
            return false;
        }

        public float CalculateFallDistance()
        {
            var groundCheck = collisionChecks.Find(c => c.name == "GroundDistance");
            if (groundCheck == null) return 0f;

            Vector3 checkPosition = groundCheck.GetCheckPosition(transform, FacingDirection);
            Vector3 adjustedDirection = groundCheck.GetAdjustedDirection(FacingDirection);
            
            if (Physics.Raycast(checkPosition, adjustedDirection, out RaycastHit hit, Mathf.Infinity, groundCheck.layers))
            {
                return hit.distance;
            }
            return Mathf.Infinity;
        }
        
        private void ConfigureInteractions(CollisionCheck interactionCheck)
        {
            interactionCheck.onCollisionEnter.AddListener((result) =>
            {
                if (result is EntityCollisionResult entityResult)
                {
                    var interactable = entityResult.Entity.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        InteractionEvents.TriggerInteractionAvailable(interactable);
                    }
                }
            });

            interactionCheck.onCollisionExit.AddListener(() =>
            {
                InteractionEvents.TriggerInteractionUnavailable();
            });
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Vector3 currentFacingDirection = FacingDirection;
            foreach (var check in collisionChecks)
            {
                if (collisionDetectors.TryGetValue(check.collisionType, out var detector))
                {
                    Vector3 checkPosition = check.GetCheckPosition(transform, currentFacingDirection);
                    detector.DrawGizmos(check, checkPosition, currentFacingDirection);
                }
            }
        }
    }
}