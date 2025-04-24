using UnityEngine;
using UnityEngine.InputSystem;

namespace Tcp4
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float runMultiplier = 1.5f;
        [SerializeField] private Transform modelTransform;

        [Header("Inputs")]
        [SerializeField] private InputActionAsset inputActions;
        private InputActionMap playerActionMap;
        private InputAction moveAction;
        private InputAction runAction;
        private InputAction interactAction;

        private Rigidbody rb;
        private Vector2 moveInput;
        private bool isRunning;

        [Header("Visual configs")]
        [SerializeField] private float rotationSpeed = 10f;
        private StepSound stepSound;
        private Animator animator;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            stepSound = GetComponent<StepSound>();


            // Input setup
            playerActionMap = inputActions.FindActionMap("Player");
            moveAction = playerActionMap.FindAction("Movement");
            runAction = playerActionMap.FindAction("Running");
            interactAction = playerActionMap.FindAction("Interaction");

            // Animator
            animator = GetComponentInChildren<Animator>();

            // Callbacks
            moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            moveAction.performed += ctx => stepSound.SetMovementInput(moveInput);
            moveAction.canceled += ctx => moveInput = Vector2.zero;
            moveAction.canceled += ctx => stepSound.SetMovementInput(Vector2.zero);

            moveAction.performed += ctx => isRunning = true;
            moveAction.canceled += ctx => isRunning = false;

            interactAction.performed += OnInteract;


        }

        private void Update()
        {
            // Atualizar parâmetros do Animator
            float speed = moveInput.magnitude * (isRunning ? runMultiplier : 1f);
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsRunning", isRunning);

            HandleRotation();
            Debug.Log(Gamepad.current);
        }

        private void FixedUpdate()
        {
            // Calcular velocidade
            Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized * moveSpeed;
            rb.linearVelocity = movement;
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            // logica de interacao
            Debug.Log("Interagiu!");
        }

        private void HandleRotation()
        {
            if (moveInput != Vector2.zero)
            {
                Vector3 movementDirection = new Vector3(moveInput.x, 0, moveInput.y);
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        private void OnEnable()
        {
            playerActionMap.Enable();
            moveAction.Enable();
            runAction.Enable();
            interactAction.Enable();
        }

        private void OnDisable()
        {
            moveAction.Disable();
            runAction.Disable();
            interactAction.Disable();
            playerActionMap.Disable();
        }
    }
}