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
using System.Collections;

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

        [TabGroup("Cursor")] public Texture2D idleCursor;
        [TabGroup("Cursor")] public Texture2D clickCursor;
        [TabGroup("Cursor")] private Vector2 cursorHotspot = Vector2.zero; // Ponto de "clique" do cursor

        private bool isClickCursorActive = false;

        public List<Sprite> clientSprites;
        public List<string> clientNames;
        public List<int> areaIDs;
        private int currentAreaId = 0;

        public List<GameObject> clientModels;
        public GameObject pfNovoDia;
        public Sprite Money;
        public GameObject player;
        public Transform safePoint;
        public PlayerMovement playerMovement;

        [HideInInspector] public Sprite lastProductionSprite;

        [Header("Worker System")]
        [SerializeField] public List<Sprite> workerPortraits = new();

        [Header("Worker Type Icons")]
        [SerializeField] public Sprite baristaIcon;
        [SerializeField] public Sprite fazendeiroIcon;
        [SerializeField] public Sprite repositorIcon;

        [Header("Worker UI")]
        [SerializeField] public Sprite workerPauseIcon;
        [SerializeField] public Sprite workerWorkingIcon;
        [SerializeField] public Sprite workerIdleIcon;

        // Método para obter ícone por tipo de trabalhador
        public Sprite GetWorkerTypeIcon(WorkerType type)
        {
            return type switch
            {
                WorkerType.Barista => baristaIcon,
                WorkerType.Fazendeiro => fazendeiroIcon,
                WorkerType.Repositor => repositorIcon,
                _ => null
            };
        }

        // Método para obter retrato aleatório
        public Sprite GetRandomWorkerPortrait()
        {
            if (workerPortraits.Count == 0) return null;
            return workerPortraits[UnityEngine.Random.Range(0, workerPortraits.Count)];
        }

        // Método para obter ícone de estado
        public Sprite GetWorkerStateIcon(WorkerState state)
        {
            return state switch
            {
                WorkerState.Resting => workerPauseIcon,
                WorkerState.Working => workerWorkingIcon,
                WorkerState.Idle => workerIdleIcon,
                _ => workerIdleIcon
            };
        }

        public event Action OnChangeInteractionSprite;
        public CurrentInputType currentInputType = CurrentInputType.NONE;

        public PlugInputComponent inputComponent;

        // NOVA FUNCIONALIDADE: Detecção automática de input
        [Header("Input Detection")]
        [SerializeField] private float inputDetectionCooldown = 0.1f; // Evita spam de detecção
        private float lastInputDetectionTime = 0f;
        private CurrentInputType lastDetectedInputType = CurrentInputType.NONE;

        #region DEBUG MODE
        public bool isDebugMode = false;
        private KeyCode[] debugSequence = { KeyCode.D, KeyCode.E, KeyCode.B, KeyCode.U, KeyCode.G };
        private int currentSequenceIndex = 0;
        private float sequenceTimeout = 2f;
        private float lastKeyPressTime = 0f;
        private bool debugJustToggled = false;

        private void HandleDebugMode()
        {
            if (UnityEngine.Input.anyKeyDown)
            {
                if (UnityEngine.Input.GetKeyDown(debugSequence[currentSequenceIndex]))
                {
                    currentSequenceIndex++;
                    lastKeyPressTime = Time.time;

                    if (currentSequenceIndex == debugSequence.Length)
                    {
                        isDebugMode = !isDebugMode;
                        Debug.Log("Debug Mode " + (isDebugMode ? "ON" : "OFF"));
                        currentSequenceIndex = 0;
                        debugJustToggled = true; // Marca que acabou de mudar
                    }
                }
                else
                {
                    currentSequenceIndex = 0;
                }
            }

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
                GUI.Label(new Rect(250, 40, 200, 30), $"Input Type: {currentInputType}");
            }
        }
        #endregion


        private void Update()
        {
            HandleDebugMode();

            if (!debugJustToggled)
                DetectLastUsedInput();
            else
                debugJustToggled = false;

            if (currentInputType == CurrentInputType.PC)
            {
                Cursor.visible = true;
                HandleCursorSprites();
            }
            else
            {
                Cursor.visible = false;
            }
        }



        private void HandleCursorSprites()
        {
            // Detecta quando o botão esquerdo do mouse é pressionado
            if (Input.GetMouseButtonDown(0))
            {
                SetClickCursor();
            }

            // Detecta quando o botão é solto
            if (Input.GetMouseButtonUp(0))
            {
                StartCoroutine(WaitToIdleCursor());
            }
        }

        private void SetClickCursor()
        {
            if (clickCursor != null && !isClickCursorActive)
            {
                isClickCursorActive = true;
                Cursor.SetCursor(clickCursor, cursorHotspot, CursorMode.Auto);
            }
        }

        private void SetIdleCursor()
        {
            if (idleCursor != null && isClickCursorActive)
            {
                isClickCursorActive = false;
                Cursor.SetCursor(idleCursor, cursorHotspot, CursorMode.Auto);
            }
        }

        IEnumerator WaitToIdleCursor()
        {
            yield return new WaitForSeconds(0.1f);
            SetIdleCursor();
        }

        public void SetCustomCursor(Texture2D cursorSprite)
        {
            if (cursorSprite != null)
            {
                Cursor.SetCursor(cursorSprite, cursorHotspot, CursorMode.Auto);
            }
        }

        public void ResetToIdleCursor()
        {
            SetIdleCursor();
        }

        private void DetectLastUsedInput()
        {
            // Cooldown para evitar detecções muito frequentes
            if (Time.time - lastInputDetectionTime < inputDetectionCooldown)
                return;

            CurrentInputType detectedType = GetCurrentActiveInputType();

            // Se detectou um input diferente do atual, atualiza
            if (detectedType != CurrentInputType.NONE && detectedType != currentInputType)
            {
                currentInputType = detectedType;
                UpdateSpriteForInputType(detectedType);
                lastInputDetectionTime = Time.time;
                Debug.Log($"[INPUT CHANGED] Switched to: {detectedType}");
            }
        }

        private CurrentInputType GetCurrentActiveInputType()
        {
            // 1. Verifica input de teclado/mouse primeiro
            if (IsKeyboardMouseActive())
            {
                return CurrentInputType.PC;
            }

            // 2. Verifica gamepads conectados e ativos
            var gamepads = Gamepad.all;
            foreach (Gamepad gamepad in gamepads)
            {
                if (IsGamepadActive(gamepad))
                {
                    // PlayStation (DualShock/DualSense)
                    if (gamepad is DualShockGamepad)
                    {
                        return CurrentInputType.PLAYSTATION;
                    }
                    // Xbox
                    else if (gamepad is XInputController)
                    {
                        return CurrentInputType.XBOX;
                    }
                    else
                    {
                        // Gamepad genérico - assume PlayStation como padrão
                        return CurrentInputType.PLAYSTATION;
                    }
                }
            }

            return CurrentInputType.NONE;
        }

        private bool IsKeyboardMouseActive()
        {
            // Verifica teclas comuns
            if (Keyboard.current != null && Keyboard.current.anyKey.isPressed)
                return true;

            // Verifica mouse
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.isPressed ||
                    Mouse.current.rightButton.isPressed ||
                    Mouse.current.middleButton.isPressed ||
                    Mouse.current.delta.ReadValue().magnitude > 0.1f ||
                    Mouse.current.scroll.ReadValue().magnitude > 0.1f)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsGamepadActive(Gamepad gamepad)
        {
            if (gamepad == null) return false;

            // Verifica botões principais
            if (gamepad.aButton.isPressed || gamepad.bButton.isPressed ||
                gamepad.xButton.isPressed || gamepad.yButton.isPressed ||
                gamepad.startButton.isPressed || gamepad.selectButton.isPressed ||
                gamepad.leftShoulder.isPressed || gamepad.rightShoulder.isPressed)
            {
                return true;
            }

            // Verifica triggers
            if (gamepad.leftTrigger.ReadValue() > 0.1f || gamepad.rightTrigger.ReadValue() > 0.1f)
            {
                return true;
            }

            // Verifica sticks analógicos
            if (gamepad.leftStick.ReadValue().magnitude > 0.2f ||
                gamepad.rightStick.ReadValue().magnitude > 0.2f)
            {
                return true;
            }

            // Verifica D-pad
            if (gamepad.dpad.up.isPressed || gamepad.dpad.down.isPressed ||
                gamepad.dpad.left.isPressed || gamepad.dpad.right.isPressed)
            {
                return true;
            }

            return false;
        }

        private void UpdateSpriteForInputType(CurrentInputType inputType)
        {
            switch (inputType)
            {
                case CurrentInputType.PC:
                    sprInteraction = inputPC;
                    break;
                case CurrentInputType.XBOX:
                    sprInteraction = inputXBOX;
                    break;
                case CurrentInputType.PLAYSTATION:
                    sprInteraction = inputPLAYSTATION;
                    break;
                default:
                    return; // Não atualiza se for NONE
            }

            OnChangeInteractionSprite?.Invoke();
        }

        public override void Awake()
        {
            base.Awake();

            player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                playerMovement = player.GetComponent<PlayerMovement>();

            // Detecção inicial de input
            InitialInputDetection();
        }

        private void Start()
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.StartMission("tutorial00");

            if (idleCursor != null)
            {
                Cursor.SetCursor(idleCursor, cursorHotspot, CursorMode.Auto);
            }
        }

        // Detecção inicial mais inteligente
        private void InitialInputDetection()
        {
            // Primeiro, verifica se há gamepads conectados
            var gamepads = Gamepad.all;
            bool hasGamepad = gamepads.Count > 0;

            if (!hasGamepad)
            {
                // Sem gamepads = assume PC
                currentInputType = CurrentInputType.PC;
                UpdateSpriteForInputType(CurrentInputType.PC);
                Debug.Log("[INITIAL DETECTION] No gamepads found - defaulting to PC");
                return;
            }

            // Se há gamepad, verifica o tipo do primeiro encontrado
            foreach (Gamepad gamepad in gamepads)
            {
                if (gamepad is DualShockGamepad)
                {
                    currentInputType = CurrentInputType.PLAYSTATION;
                    UpdateSpriteForInputType(CurrentInputType.PLAYSTATION);
                    Debug.Log("[INITIAL DETECTION] DualShock detected - defaulting to PlayStation");
                    return;
                }
                else if (gamepad is XInputController)
                {
                    currentInputType = CurrentInputType.XBOX;
                    UpdateSpriteForInputType(CurrentInputType.XBOX);
                    Debug.Log("[INITIAL DETECTION] XInput detected - defaulting to Xbox");
                    return;
                }
            }

            // Gamepad genérico encontrado
            currentInputType = CurrentInputType.PLAYSTATION;
            UpdateSpriteForInputType(CurrentInputType.PLAYSTATION);
            Debug.Log("[INITIAL DETECTION] Generic gamepad detected - defaulting to PlayStation");
        }

        // Mantém a funcionalidade original como método público para casos específicos
        public void UpdateControlSprite()
        {
            InitialInputDetection();
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
                Debug.Log($"[DEVICE CHANGE] {device.name}: {change}");
                InitialInputDetection();
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

        public int GenerateAreaID()
        {
            var newID = currentAreaId;
            areaIDs.Add(currentAreaId);
            currentAreaId++;
            return newID;
        }

        // Método público para forçar mudança de input (útil para testes)
        [Button("Force PC Input")]
        public void ForcePC() => SetInputType(CurrentInputType.PC);

        [Button("Force Xbox Input")]
        public void ForceXbox() => SetInputType(CurrentInputType.XBOX);

        [Button("Force PlayStation Input")]
        public void ForcePlayStation() => SetInputType(CurrentInputType.PLAYSTATION);

        private void SetInputType(CurrentInputType type)
        {
            currentInputType = type;
            UpdateSpriteForInputType(type);
        }

        public void SetupLastIconMinigamePlants(Sprite productImage)
        {
            lastProductionSprite = productImage;
        }
    }
}