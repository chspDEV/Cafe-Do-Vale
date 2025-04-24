using UnityEngine;
using UnityEngine.InputSystem;

namespace Tcp4
{
    public class StepSound : MonoBehaviour
    {
        public float stepInterval = 0.4f;
        public float deadzone = 0.1f; 
        private float stepTimer;
        private Vector2 movementInput;

        private void Update()
        {
            stepTimer -= Time.deltaTime;

            float inputMagnitude = movementInput.magnitude;

            if (inputMagnitude > deadzone)
            {
                if (stepTimer <= 0)
                {
                    SoundManager.PlaySound(SoundType.passos, 0.2f);
                    stepTimer = stepInterval;
                }
            }
            else
            {
                stepTimer = stepInterval; 
            }
        }
        public void SetMovementInput(Vector2 input)
        {
            movementInput = input;
        }
    }
}