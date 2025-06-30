using Tcp4.Assets.Resources.Scripts.Managers;
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
        private bool canMove = true;
        private StepSound stepSound;

        public bool forwardPressed, backwardPressed, leftPressed, rightPressed;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            stepSound = GetComponent<StepSound>();
            animator = GetComponentInChildren<Animator>();
            canMove = true;
        }

        public void SetMovement(InputAction.CallbackContext value)
        {
            movement = value.ReadValue<Vector2>();

            //Checagens para booleanas de tutoriais
            forwardPressed = movement.y     >= 0.1f;
            backwardPressed = movement.y    <= -0.1f;
            leftPressed = movement.x        <= -0.1f;
            rightPressed = movement.x       >= 0.1f;

        }

        public bool CanMove() => canMove;

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
            stepSound.SetMovementInput(movement);
            var _isRunning = _speed > 0.1f;
            animator.SetFloat("Speed", _speed);
            animator.SetBool("IsRunning", _isRunning);
        }

        void Update()
        {
            if (!canMove) return;

            HandleRotation();
            HandleAnimation();

            if (GameAssets.Instance.isDebugMode)
            {
                if (Input.GetKeyDown(KeyCode.K))
                {
                    moveSpeed += 50;
                }

                if (Input.GetKeyDown(KeyCode.L))
                {
                    moveSpeed -= 50;
                }
            }
        }

        void FixedUpdate()
        {
            if (canMove)
            {
                rb.linearVelocity = new Vector3(movement.x, 0, movement.y) * moveSpeed * Time.fixedDeltaTime;
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
            }
                
        }

        public void Active()
        {
            canMove = true;
        }

        public void Deactive()
        { 
            canMove = false; 
        }

        public void ToggleMovement(bool state)
        {
            if (state) Active();
            else Deactive();
        }
    }
}