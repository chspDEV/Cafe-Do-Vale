using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Tcp4.Resources.Scripts.Core;

namespace Tcp4
{

    public class MovingPlatform : MonoBehaviour
    {
        public enum MovementType
        {
            PingPong,
            Loop,
            OneWay,
            Reverse,
            Random
        }

        [System.Serializable]
        public class Waypoint
        {
            public Vector3 relativePosition;
            public float waitTime;
            public UnityEngine.Events.UnityEvent onWaypointReached;
        }

        [Header("Movement Settings")]
        [SerializeField] private MovementType movementType = MovementType.PingPong;
        [SerializeField] private List<Waypoint> waypoints = new List<Waypoint>();
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private bool autoStart = true;
        [SerializeField] private LayerMask passengerLayers;

        [Header("Debug")]
        [SerializeField] private Color gizmoColor = Color.blue;
        [SerializeField] private float gizmoSphereRadius = 0.2f;

        private int currentWaypointIndex = 0;
        private int direction = 1;
        private float waitCounter;
        private bool isMoving;
        private List<Transform> passengers = new List<Transform>();
        private Vector3 initialPosition;

        public event Action<Vector3> OnMove;
        public event Action<Waypoint> OnWaypointReached;

        private IMovementStrategy currentMovementStrategy;
        private Dictionary<MovementType, IMovementStrategy> movementStrategies;

        private void Awake()
        {
            InitializeMovementStrategies();
            initialPosition = transform.position;
        }

        private void Start()
        {
            InitializeWaypoints();
            if (autoStart)
            {
                StartMoving();
            }
        }

        private void InitializeMovementStrategies()
        {
            movementStrategies = new Dictionary<MovementType, IMovementStrategy>
            {
                { MovementType.PingPong, new PingPongMovement() },
                { MovementType.Loop, new LoopMovement() },
                { MovementType.OneWay, new OneWayMovement() },
                { MovementType.Reverse, new ReverseMovement() },
                { MovementType.Random, new RandomMovement() }
            };
            UpdateMovementStrategy();
        }

        private void InitializeWaypoints()
        {
            if (waypoints.Count == 0)
            {
                waypoints.Add(new Waypoint { relativePosition = Vector3.zero, waitTime = 0f });
            }
        }

        private void FixedUpdate()
        {
            if (!isMoving || waypoints.Count < 2) return;

            MovePlatform();
            UpdatePassengers();
        }

        public void StartMoving()
        {
            isMoving = true;
        }

        public void StopMoving()
        {
            isMoving = false;
        }

        public void SetMovementType(MovementType newType)
        {
            movementType = newType;
            UpdateMovementStrategy();
        }

        private void UpdateMovementStrategy()
        {
            if (movementStrategies.TryGetValue(movementType, out var strategy))
            {
                currentMovementStrategy = strategy;
            }
        }

        private void MovePlatform()
        {
            if (waitCounter > 0)
            {
                waitCounter -= Time.fixedDeltaTime;
                return;
            }

            Vector3 oldPosition = transform.position;
            Vector3 targetPosition = initialPosition + waypoints[currentWaypointIndex].relativePosition;
            transform.position = Vector3.MoveTowards(oldPosition, targetPosition, moveSpeed * Time.fixedDeltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                OnWaypointReached?.Invoke(waypoints[currentWaypointIndex]);
                waypoints[currentWaypointIndex].onWaypointReached?.Invoke();
                waitCounter = waypoints[currentWaypointIndex].waitTime;
                ChooseNextWaypoint();
            }

            Vector3 movement = transform.position - oldPosition;
            if (movement != Vector3.zero)
            {
                OnMove?.Invoke(movement);
            }
        }

        private void ChooseNextWaypoint()
        {
            if (currentMovementStrategy != null)
            {
                currentWaypointIndex = currentMovementStrategy.GetNextWaypointIndex(currentWaypointIndex, waypoints.Count, ref direction);
            }
        }

        private void UpdatePassengers()
        {
            foreach (var passenger in passengers.ToList())
            {
                if (passenger != null)
                {
                    var entity = passenger.GetComponent<BaseEntity>();
                    if (entity != null)
                    {
                        ParentPassenger(passenger);
                    }
                }
                else
                {
                    passengers.Remove(passenger);
                }
            }
        }

        private void ParentPassenger(Transform passenger)
        {
            passenger.SetParent(transform, true);
        }

        private void UnparentPassenger(Transform passenger)
        {
            passenger.SetParent(null);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & passengerLayers) != 0)
            {
                var passenger = collision.transform;
                if (!passengers.Contains(passenger))
                {
                    ContactPoint[] contactPoints = collision.contacts;
                    bool isOnTop = false;

                    foreach (var contact in contactPoints)
                    {
                        if (contact.point.y > transform.position.y)
                        {
                            isOnTop = true;
                            break;
                        }
                    }

                    if (isOnTop)
                    {
                        passengers.Add(passenger);
                        ParentPassenger(passenger);
                    }
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & passengerLayers) != 0)
            {
                var passenger = collision.transform;
                ContactPoint[] contactPoints = collision.contacts;
                bool isOnTop = false;

                foreach (var contact in contactPoints)
                {
                    if (contact.point.y > transform.position.y)
                    {
                        isOnTop = true;
                        break;
                    }
                }

                if (!isOnTop && passengers.Contains(passenger))
                {
                    UnparentPassenger(passenger);
                    passengers.Remove(passenger);
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & passengerLayers) != 0)
            {
                var passenger = collision.transform;
                if (passengers.Remove(passenger))
                {
                    UnparentPassenger(passenger);
                }
            }
        }


        private void OnDisable()
        {
            foreach (var passenger in passengers)
            {
                if (passenger != null)
                {
                    UnparentPassenger(passenger);
                }
            }
            passengers.Clear();
        }
        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Count == 0) return;

            Gizmos.color = gizmoColor;

            Vector3 basePosition = Application.isPlaying ? initialPosition : transform.position;

            for (int i = 0; i < waypoints.Count; i++)
            {
                Vector3 position = basePosition + waypoints[i].relativePosition;
                Gizmos.DrawSphere(position, gizmoSphereRadius);

                if (i < waypoints.Count - 1)
                {
                    Gizmos.DrawLine(position, basePosition + waypoints[i + 1].relativePosition);
                }
                else if (movementType != MovementType.OneWay)
                {
                    Gizmos.DrawLine(position, basePosition + waypoints[0].relativePosition);
                }
            }
        }

        public void UpdateWaypointPosition(int index, Vector3 newRelativePosition)
        {
            if (index >= 0 && index < waypoints.Count)
            {
                waypoints[index].relativePosition = newRelativePosition;
            }
        }
    }
    public interface IMovementStrategy
    {
        int GetNextWaypointIndex(int currentIndex, int waypointCount, ref int direction);
    }

    public class PingPongMovement : IMovementStrategy
    {
        public int GetNextWaypointIndex(int currentIndex, int waypointCount, ref int direction)
        {
            if (currentIndex == 0 || currentIndex == waypointCount - 1)
            {
                direction *= -1;
            }
            return currentIndex + direction;
        }
    }

    public class LoopMovement : IMovementStrategy
    {
        public int GetNextWaypointIndex(int currentIndex, int waypointCount, ref int direction)
        {
            return (currentIndex + 1) % waypointCount;
        }
    }

    public class OneWayMovement : IMovementStrategy
    {
        public int GetNextWaypointIndex(int currentIndex, int waypointCount, ref int direction)
        {
            return currentIndex < waypointCount - 1 ? currentIndex + 1 : currentIndex;
        }
    }

    public class ReverseMovement : IMovementStrategy
    {
        public int GetNextWaypointIndex(int currentIndex, int waypointCount, ref int direction)
        {
            return currentIndex > 0 ? currentIndex - 1 : currentIndex;
        }
    }

    public class RandomMovement : IMovementStrategy
    {
        public int GetNextWaypointIndex(int currentIndex, int waypointCount, ref int direction)
        {
            int nextIndex;
            do
            {
                nextIndex = UnityEngine.Random.Range(0, waypointCount);
            } while (nextIndex == currentIndex && waypointCount > 1);
            return nextIndex;
        }
    }

    // Extension para BaseEntity
    public static class BaseEntityExtensions
    {
        public static void UpdatePlatformMovement(this BaseEntity entity, Vector3 movement)
        {
            // Implemente a l�gica espec�fica para mover a entidade junto com a plataforma
            entity.transform.position += movement;
        }
    }
}