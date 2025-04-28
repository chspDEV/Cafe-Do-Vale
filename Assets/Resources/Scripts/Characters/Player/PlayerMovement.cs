using UnityEngine;
using UnityEngine.InputSystem;

namespace Tcp4
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private Transform modelTransform;
        private Rigidbody rb;
        private Vector2 movement;
        private Animator animator;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponentInChildren<Animator>();
        }

        public void SetMovement(InputAction.CallbackContext value)
        {
            movement = value.ReadValue<Vector2>();
        }

        private void HandleRotation()
        {
            if (movement != Vector2.zero)
            {
                Vector3 movementDirection = new Vector3(movement.x, 0, movement.y);
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        private void HandleAnimation()
        {
            var _speed = movement.magnitude;
            var _isRunning = _speed > 0.1f;
            animator.SetFloat("Speed", _speed);
            animator.SetBool("IsRunning", _isRunning);
        }

        void Update()
        {
            HandleRotation();
            HandleAnimation();
        }

        void FixedUpdate()
        {
            rb.linearVelocity = new Vector3(movement.x, 0, movement.y) * moveSpeed * Time.fixedDeltaTime;
        }
    }
}