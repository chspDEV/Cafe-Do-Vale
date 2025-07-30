
using GameResources.Project.Scripts.Utilities.Audio;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using PlugInputPack;

public enum CurrentInputType
{ 
    PC,
    XBOX,
    PLAYSTATION,
    NONE
}

namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class GameAssets : Singleton<GameAssets>
    {
        [TabGroup("Refinamento")] public Sprite sprError;
        [TabGroup("Refinamento")] public Sprite sprProductionWait;
        [TabGroup("Refinamento")] public Sprite sprRefinamentWait;
        [TabGroup("Refinamento")] public Sprite sprSpoilingWarning;

        [TabGroup("Gerais")] public Sprite ready;
        [TabGroup("Gerais")] public Sprite transparent;

        [TabGroup("Interacao")] public Sprite sprInteraction;
        [TabGroup("Interacao")]
        [SerializeField] private Sprite inputXBOX;
        [TabGroup("Interacao")]
        [SerializeField] private Sprite inputPLAYSTATION;
        [TabGroup("Interacao")]
        [SerializeField] private Sprite inputPC;

        public List<Sprite> clientSprites;
        public List<string> clientNames;

        public List<GameObject> clientModels;
        public GameObject pfNovoDia;
        public Sprite Money;
        public GameObject player;
        public Transform safePoint;
        public PlayerMovement playerMovement;

        public event Action OnChangeInteractionSprite;
        public CurrentInputType currentInputType = CurrentInputType.NONE;

        public PlugInputComponent inputComponent;

        #region DEBUG MODE
        public bool isDebugMode = false;
        private KeyCode[] debugSequence = { KeyCode.D, KeyCode.E, KeyCode.B, KeyCode.U, KeyCode.G };
        private int currentSequenceIndex = 0;
        private float sequenceTimeout = 2f; 
        private float lastKeyPressTime = 0f;
        

        private void Update()
        {
            // Verifica se alguma tecla foi pressionada
            if (UnityEngine.Input.anyKeyDown)
            {
                // Verifica se a tecla pressionada é a próxima na sequência
                if (UnityEngine.Input.GetKeyDown(debugSequence[currentSequenceIndex]))
                {
                    currentSequenceIndex++;
                    lastKeyPressTime = Time.time;

                    // Se completou toda a sequência
                    if (currentSequenceIndex == debugSequence.Length)
                    {
                        isDebugMode = !isDebugMode; // Alterna o modo debug
                        Debug.Log("Debug Mode " + (isDebugMode ? "ON" : "OFF"));
                        currentSequenceIndex = 0; // Reseta a sequência
                    }
                }
                else
                {
                    // Tecla errada - reseta a sequência
                    currentSequenceIndex = 0;
                }
            }

            // Reseta a sequência se o tempo entre teclas for muito longo
            if (Time.time - lastKeyPressTime > sequenceTimeout && currentSequenceIndex > 0)
            {
                currentSequenceIndex = 0;
            }
        }

        private void OnGUI()
        {
            if (isDebugMode)
            {
                GUI.Label(new Rect(250, 10, 200, 30), "DEBUG MODE ATIVO");
            }
        }

        #endregion

        public override void Awake()
        {
            base.Awake();

            player = GameObject.FindGameObjectWithTag("Player");

            if(player != null)
                playerMovement = player.GetComponent<PlayerMovement>();

            UpdateControlSprite();
            
        }

        private void Start()
        {
            UpdateControlSprite();

            //Fazendo o request de ost
            SoundEventArgs ostArgs = new()
            {
                Category = SoundEventArgs.SoundCategory.Music,
                AudioID = "fazenda", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                VolumeScale = .1f // Escala de volume (opcional, padrão é 1f)
            };

            SoundEvent.RequestSound(ostArgs);

            if(QuestManager.Instance != null)
                QuestManager.Instance.StartMission("tutorial00");
        }

        public void UpdateControlSprite()
        {
            // String de debug
            string debugText = "Dispositivos conectados: ";

            // Verifica se há gamepads conectados
            var gamepads = Gamepad.all;
            bool hasGamepad = gamepads.Count > 0;

            // Debug: Lista todos os dispositivos
            foreach (InputDevice device in InputSystem.devices)
            {
                debugText += $"\n- {device.name} ({device.layout})";
            }
            //Debug.Log(debugText);

            // Caso não tenha gamepads conectados
            if (!hasGamepad)
            {
                sprInteraction = inputPC;
                OnChangeInteractionSprite?.Invoke();
                //Debug.Log("[SPRITE ATUALIZADO] MODELO COMPUTADOR (Teclado/Mouse)");
                currentInputType = CurrentInputType.PC;
                return;
            }

            // Verifica cada gamepad conectado
            foreach (Gamepad gamepad in gamepads)
            {
                // PlayStation (DualShock/DualSense)
                if (gamepad is DualShockGamepad )
                    
                {
                    sprInteraction = inputPLAYSTATION;
                    OnChangeInteractionSprite?.Invoke();
                    Debug.Log("[SPRITE ATUALIZADO] MODELO PLAYSTATION");
                    currentInputType = CurrentInputType.PLAYSTATION;
                    return;
                }
                // Xbox
                else if (gamepad is XInputController)
                {
                    sprInteraction = inputXBOX;
                    OnChangeInteractionSprite?.Invoke();
                    Debug.Log("[SPRITE ATUALIZADO] MODELO XBOX");
                    currentInputType = CurrentInputType.XBOX;
                    return;
                }
            }

            // Se chegou aqui, é um gamepad genérico
            sprInteraction = inputPLAYSTATION; 
            OnChangeInteractionSprite?.Invoke();
            Debug.Log("[SPRITE ATUALIZADO] MODELO GENÉRICO");
        }

        private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added ||
                change == InputDeviceChange.Removed ||
                change == InputDeviceChange.Reconnected || 
                change == InputDeviceChange.Disconnected ||
                change == InputDeviceChange.Enabled ||
                change == InputDeviceChange.Disabled ||
                change == InputDeviceChange.SoftReset ||
                change == InputDeviceChange.HardReset)
            {
                
                UpdateControlSprite();
            }
        }

        private void OnEnable()
        {
            InputSystem.onDeviceChange += OnDeviceChanged;
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChanged;
        }

        public static string GenerateID(int tamanho)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new System.Random();
            var result = new string(
                Enumerable.Repeat(chars, tamanho)
                        .Select(s => s[random.Next(s.Length)])
                        .ToArray());
            return result;
        }
    }
}