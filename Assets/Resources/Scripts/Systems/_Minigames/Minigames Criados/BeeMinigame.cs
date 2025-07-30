using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Tcp4.Assets.Resources.Scripts.Managers;
using static Codice.Client.BaseCommands.BranchExplorer.Layout.BrExLayout;

public class BeeMinigame : BaseMinigame
{
    private enum PlayerHandState
    {
        Idle, //nao esta carregando o favo
        CarryingHoney //esta carregando o favo de volta para a cesta
    }

    [Header("Setup")]
    [SerializeField] private RectTransform playerHands;
    [SerializeField] private RectTransform honeycombTarget;
    [SerializeField] private RectTransform honeycombSpawnPoint;
    [SerializeField] private GameObject lifeContainer;
    [SerializeField] private GameObject lifeIconPrefab;
    [SerializeField] private GameObject beePrefab;
    [SerializeField] private Transform beeContainer;
    [SerializeField] private TextMeshProUGUI favoCounter;
    [SerializeField] private Vector2 startPosition = new Vector2(0, -300); //posicao inicial em pixels

    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 300f;
    [SerializeField] private Vector2 playAreaSize = new Vector2(500, 700); //limites da area em pixels

    [Header("Game Rules")]
    [SerializeField] private int maxStings = 2;
    [SerializeField] private float interactionDistance = 60f; //distancia para poder pegar/entregar o favo

    [Header("Difficulty Scaling")]
    [SerializeField] private float initialBeeSpeed = 250f;
    [SerializeField] private float beeSpeedIncrease = 30f;
    [SerializeField] private float initialSpawnRate = 1.5f;
    [SerializeField] private float minSpawnRate = 0.5f;
    [SerializeField] private float spawnRateReduction = 0.1f;
    [SerializeField] private RectTransform[] beeSpawnPaths;

    [Header("Input")]
    [SerializeField] private GameObject inputToPress;
    private Animator animInput;


    //estado do jogo
    private int stings;
    private int honeycombsCollected;
    private float currentBeeSpeed;
    private float currentSpawnRate;
    private bool isGameActive = false;
    private bool isInvincible = false;

    private PlayerHandState currentHandState;
    private Vector2 startWorldPosition;

    private PlugInputPack.InputAccessor moveInput;
    private PlugInputPack.InputAccessor interactInput;
    private Coroutine beeSpawnerCoroutine;
    private List<GameObject> lifeIcons = new List<GameObject>();


    private void Start()
    {
        moveInput = GameAssets.Instance.inputComponent["Movement"];
        interactInput = GameAssets.Instance.inputComponent["Interact"];
        UpdateCounterText();
    }

    private void OnEnable()
    {
        PlayerHand.OnPlayerStung += HandleSting;
    }

    private void OnDisable()
    {
        PlayerHand.OnPlayerStung -= HandleSting;
    }

    public override void StartMinigame()
    {
        StartCoroutine(nameof(LoadingMinigame));
    }

    private IEnumerator LoadingMinigame()
    {
        stings = 0;
        honeycombsCollected = 0;
        currentBeeSpeed = initialBeeSpeed;
        currentSpawnRate = initialSpawnRate;
        isGameActive = true;
        isInvincible = false;
        currentHandState = PlayerHandState.Idle;

        startWorldPosition = startPosition;
        playerHands.localPosition = startWorldPosition;

        honeycombTarget.SetParent(transform, true);
        honeycombTarget.gameObject.SetActive(true);

        UpdateLifeUI();
        beeSpawnerCoroutine = StartCoroutine(BeeSpawnerCoroutine());

        yield return new WaitForSeconds(currentSpawnRate * 3f);
    }

    private void Update()
    {
        if (!isGameActive) return;

        HandleMovementInput();
        HandleInteractionInput();
        HandleFavos();
    }

    private void HandleMovementInput()
    {
        Vector2 moveDirection = moveInput.Vector2;
        if (moveDirection.magnitude < 0.1f) return;

        Vector3 currentPos = playerHands.localPosition;
        Vector3 newPos = currentPos + (Vector3)moveDirection * moveSpeed * Time.deltaTime;

        // Limita a movimentação dentro da área de jogo
        newPos.x = Mathf.Clamp(newPos.x, -playAreaSize.x / 2f, playAreaSize.x / 2f);
        newPos.y = Mathf.Clamp(newPos.y, -playAreaSize.y / 2f, playAreaSize.y / 2f);

        playerHands.localPosition = newPos;

        // Verifica se precisa entregar o favo
        if (currentHandState == PlayerHandState.CarryingHoney)
        {
            float distanceToStart = Vector3.Distance(playerHands.localPosition, startWorldPosition);
            if (distanceToStart < interactionDistance)
            {
                ScorePoint();
            }
        }
    }

    private void HandleInteractionInput()
    {
        if (interactInput.Pressed)
        {
            float distanceToTarget = Vector3.Distance(playerHands.position, honeycombTarget.position);
            if (distanceToTarget <= interactionDistance && currentHandState == PlayerHandState.Idle)
            {
                currentHandState = PlayerHandState.CarryingHoney;
                inputToPress.SetActive(false);
                honeycombTarget.SetParent(playerHands, true);
                honeycombTarget.localPosition = Vector3.zero;
            }
            else
            {
                Debug.Log($"faltam: {distanceToTarget} e precisa de {interactionDistance}");
            }
        }
    }




    private void ScorePoint()
    {
        honeycombsCollected++;
        currentHandState = PlayerHandState.Idle;

        honeycombTarget.SetParent(honeycombSpawnPoint, true);
        honeycombTarget.gameObject.SetActive(true);
        inputToPress.SetActive(true);
        honeycombTarget.localPosition = Vector3.zero;
        UpdateCounterText();

        currentBeeSpeed += beeSpeedIncrease;
        moveSpeed += beeSpeedIncrease / 2;
        currentSpawnRate = Mathf.Max(minSpawnRate, currentSpawnRate - spawnRateReduction);
    }

    void UpdateCounterText()
    {
        favoCounter.text = honeycombsCollected.ToString() + "/ 6";
    }

    public void HandleSting()
    {
        if (isInvincible || !isGameActive) return;

        stings++;
        UpdateLifeUI();
        StartCoroutine(InvincibilityCoroutine());

        if (currentHandState == PlayerHandState.CarryingHoney)
        {
            currentHandState = PlayerHandState.Idle;
            honeycombTarget.SetParent(transform, true);
        }

        if (stings >= maxStings)
        {
            EndGame();
        }
    }

    void HandleFavos()
    {
        if (!isGameActive) return;

        if (honeycombsCollected >= 6)
        {
            EndGame();
        }
    }



    private IEnumerator BeeSpawnerCoroutine()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(currentSpawnRate);
            if (!isGameActive) break;

            RectTransform path = beeSpawnPaths[Random.Range(0, beeSpawnPaths.Length)];
            bool comesFromLeft = Random.value > 0.5f;

            float xPos = comesFromLeft ? -Screen.width / 1.5f : Screen.width / 1.5f;
            Vector3 spawnPos = new Vector3(xPos, path.localPosition.y, 0);
            Vector2 direction = comesFromLeft ? Vector2.right : Vector2.left;

            GameObject beeObj = Instantiate(beePrefab, beeContainer);
            beeObj.GetComponent<RectTransform>().localPosition = spawnPos;
            beeObj.GetComponent<Bee>().Initialize(currentBeeSpeed, direction, playerHands);
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(1.5f);
        isInvincible = false;
    }

    private void UpdateLifeUI()
    {
        foreach (var icon in lifeIcons)
        {
            Destroy(icon);
        }
        lifeIcons.Clear();
        if (lifeIconPrefab == null) return;
        for (int i = 0; i < maxStings - stings; i++)
        {
            lifeIcons.Add(Instantiate(lifeIconPrefab, lifeContainer.transform));
        }
    }

    private void EndGame()
    {
        if (!isGameActive) return;
        isGameActive = false;

        if (beeSpawnerCoroutine != null) StopCoroutine(beeSpawnerCoroutine);

        foreach (Bee bee in FindObjectsByType<Bee>(FindObjectsSortMode.None))
        {
            Destroy(bee.gameObject);
        }

        MinigamePerformance performance;

        if (honeycombsCollected >= 6)
            performance = MinigamePerformance.Gold;
        else if (honeycombsCollected >= 4)
            performance = MinigamePerformance.Silver;
        else if (honeycombsCollected >= 2)
            performance = MinigamePerformance.Bronze;
        else
            performance = MinigamePerformance.Fail;

        ConcludeMinigame(performance);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            // redesenha a posicao inicial da mao para refletir o valor no inspector
            playerHands.localPosition = startPosition;
        }

        // --- 1. desenha a area de jogo (retangulo amarelo) ---
        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.25f); //amarelo semi-transparente
        Vector3 areaCenter = transform.position;
        Vector3 areaSize = new Vector3(playAreaSize.x, playAreaSize.y, 0.1f);
        Gizmos.DrawCube(areaCenter, areaSize); //desenha um cubo solido

        Gizmos.color = Color.yellow; //amarelo solido para a borda
        Gizmos.DrawWireCube(areaCenter, areaSize); //desenha a borda

        // --- 2. desenha a posicao inicial da mao (esfera verde) ---
        Gizmos.color = Color.green;
        Vector3 startWorldPos = transform.position + (Vector3)startPosition;
        Gizmos.DrawSphere(startWorldPos, 20f); //desenha uma esfera com raio de 20 pixels
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(honeycombTarget.position, Vector3.one * interactionDistance);

    }
#endif
}