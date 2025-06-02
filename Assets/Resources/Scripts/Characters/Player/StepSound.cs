using GameResources.Project.Scripts.Utilities.Audio;
using Tcp4.Assets.Resources.Scripts.Managers;
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
        private GameAssets gameAssets;
        private void Start()
        {
            gameAssets = GameAssets.Instance;
        }


        private void Update()
        {
            stepTimer -= Time.deltaTime;

            float inputMagnitude = movementInput.magnitude;

            if (inputMagnitude > deadzone)
            {
                if (stepTimer <= 0 && GameAssets.Instance.playerMovement.CanMove())
                {
                    //Fazendo o request de sfx
                    SoundEventArgs sfxArgs = new()
                    {
                        Category = SoundEventArgs.SoundCategory.SFX,
                        AudioID = "passos", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                        Position = gameAssets.player.transform.position, // Posição para o som 3D
                        VolumeScale = .1f, // Escala de volume (opcional, padrão é 1f)
                        Pitch = Random.Range(0.7f, 1.4f)

                    };
                    SoundEvent.RequestSound(sfxArgs);

                    //ANTIGO!
                    //SoundManager.PlaySound(SoundType.passos, 0.2f);
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