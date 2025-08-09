using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Tcp4.Assets.Resources.Scripts.Managers;
using GameResources.Project.Scripts.Utilities.Audio;

//mantenha estas definicoes no mesmo arquivo ou em um separado
public enum ButtonType
{
    Cross,
    Triangle,
    Square,
    Circle
}

[System.Serializable]
public class ButtonMapping
{
    public ButtonType type;
    public GameObject buttonPrefab;
}


public class CowMinigame : BaseMinigame
{
    [Header("Game Objects & UI")]
    [SerializeField] private Image bucketFillImage;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Transform buttonPromptContainer;
    [SerializeField] private List<ButtonMapping> buttonMappings;
    [SerializeField] private GameObject lifePrefab;
    [SerializeField] private Transform lifeContainer;
    [SerializeField] private Image timeBar;
    [SerializeField] private Image cow;
    [SerializeField] private Animator cowAnimator;
    private List<GameObject> spawnedLifes = new();

    [Header("Game Rules")]
    [SerializeField] private float initialTime = 10f;
    [SerializeField] private float timeBonusPerHit = 0.4f;
    [SerializeField] private float fillAmountPerHit = 0.25f;
    [SerializeField] private int maxMisses = 2;
    [SerializeField] private float timePauseOnHitDuration = 0.3f;

    [Header("Difficulty Scaling")]
    [SerializeField] private float initialTimeToPress = 1.5f;
    [SerializeField] private float minTimeToPress = 0.4f;
    [SerializeField] private float timeReductionPerHit = 0.05f;

    [Header("UI Feedback")]
    [SerializeField] private Color fullTimeColor = Color.green;
    [SerializeField] private Color lowTimeColor = Color.red;

    //estado do jogo
    private float currentTime;
    private float timeToPress;
    private int misses;
    private float currentBucketFill;
    private int bucketsFilled;
    private bool isGameActive = false;
    private bool isWaitingForInput = false;
    private bool isTimerPaused = false;

    private float currentPitch = 1f;

    //input
    private Dictionary<ButtonType, PlugInputPack.InputAccessor> inputAccessors = new Dictionary<ButtonType, PlugInputPack.InputAccessor>();
    private ButtonType currentRequiredButton;
    private GameObject currentButtonObject;

    private Coroutine _promptCoroutine;

    private void Awake()
    {
        SetupInputs();
    }

    public override void StartMinigame()
    {
        currentTime = initialTime;
        timeToPress = initialTimeToPress;
        misses = 0;
        currentBucketFill = 0;
        bucketsFilled = 0;
        isTimerPaused = false;
        if (bucketFillImage) bucketFillImage.fillAmount = 0;
        isGameActive = true;
        UpdateTimeBar();
        UpdateLife();

        _promptCoroutine = StartCoroutine(ShowNextButtonCoroutine());
    }

    private void Update()
    {
        if (!isGameActive) return;

        if (!isTimerPaused)
        {
            currentTime -= Time.deltaTime;
            UpdateTimeBar();
        }

        if (timerText) timerText.text = currentTime.ToString("F2");

        if (currentTime <= 0)
        {
            EndGame();
            return;
        }

        if (isWaitingForInput)
        {
            CheckPlayerInput();
        }
    }

    void UpdateLife()
    {
        if (spawnedLifes.Count > 0)
        {
            foreach (var v in spawnedLifes)
            {
                Destroy(v);
            }
            spawnedLifes.Clear();
        }

        if (lifePrefab == null) return;

        for (int i = 0; i < maxMisses - misses; i++)
        {
            spawnedLifes.Add(Instantiate(lifePrefab, lifeContainer));
        }
    }

    void UpdateTimeBar()
    {
        if (timeBar == null) return;

        float normalizedTime = Mathf.Clamp01(currentTime / initialTime);
        timeBar.fillAmount = normalizedTime;
        timeBar.color = Color.Lerp(lowTimeColor, fullTimeColor, normalizedTime);
    }

    private void SetupInputs()
    {
        inputAccessors[ButtonType.Cross] = GameAssets.Instance.inputComponent["Minigame_Cross"];
        inputAccessors[ButtonType.Triangle] = GameAssets.Instance.inputComponent["Minigame_Triangle"];
        inputAccessors[ButtonType.Square] = GameAssets.Instance.inputComponent["Minigame_Square"];
        inputAccessors[ButtonType.Circle] = GameAssets.Instance.inputComponent["Minigame_Circle"];
    }

    private void CheckPlayerInput()
    {
        //agora chama as corrotinas de handle em vez dos metodos diretos
        if (inputAccessors[currentRequiredButton].Pressed)
        {
            StartCoroutine(HandleSuccessCoroutine());
            return;
        }

        foreach (var accessorPair in inputAccessors)
        {
            if (accessorPair.Key == currentRequiredButton) continue;
            if (accessorPair.Value.Pressed)
            {
                StartCoroutine(HandleMissCoroutine());
                return;
            }
        }
    }

    private IEnumerator ShowNextButtonCoroutine()
    {
        isWaitingForInput = false;
        yield return new WaitForSeconds(0.2f);

        int randomIndex = Random.Range(0, buttonMappings.Count);
        currentRequiredButton = buttonMappings[randomIndex].type;

        currentButtonObject = Instantiate(buttonMappings[randomIndex].buttonPrefab, buttonPromptContainer);
        isWaitingForInput = true;

        yield return new WaitForSeconds(timeToPress);

        if (isWaitingForInput)
        {
            StartCoroutine(HandleMissCoroutine(true)); //erro por timeout
        }
    }

    private IEnumerator HandleSuccessCoroutine()
    {
        if (!isWaitingForInput) yield break; //guarda para evitar processamento duplo
        isWaitingForInput = false;
        if (_promptCoroutine != null) StopCoroutine(_promptCoroutine);
        if (currentButtonObject) Destroy(currentButtonObject);

        //aqui esta a correcao: esperamos um frame antes de continuar
        yield return new WaitForEndOfFrame();

        StartCoroutine(PauseTimerOnHit());
        currentTime += timeBonusPerHit;
        currentBucketFill += fillAmountPerHit;

        SoundEventArgs sfxArgs = new()
        {
            Category = SoundEventArgs.SoundCategory.SFX,
            AudioID = "interacao", // O ID do seu SFX (sem "sfx_" e em minúsculas)
            VolumeScale = 1f, // Escala de volume (opcional, padrão é 1f)
            Pitch = currentPitch

        };
        SoundEvent.RequestSound(sfxArgs);

        cow.transform.localScale = new(cow.transform.localScale.x * -1,
            cow.transform.localScale.y, cow.transform.localScale.z);

        cowAnimator.Play("Hit");

        currentPitch += 0.05f;

        UpdateTimeBar();

        if (currentBucketFill >= 1f)
        {
            bucketsFilled++;
            SoundEventArgs sfxArgs2 = new()
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = "servindo", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                VolumeScale = 1f, // Escala de volume (opcional, padrão é 1f)
                Pitch = currentPitch

            };
            SoundEvent.RequestSound(sfxArgs2);
            currentBucketFill = 0f;
        }
        if (bucketFillImage) bucketFillImage.fillAmount = currentBucketFill;

        timeToPress = Mathf.Max(minTimeToPress, timeToPress - timeReductionPerHit);
        _promptCoroutine = StartCoroutine(ShowNextButtonCoroutine());
    }

    private IEnumerator PauseTimerOnHit()
    {
        isTimerPaused = true;
        yield return new WaitForSeconds(timePauseOnHitDuration);
        isTimerPaused = false;
    }

    private IEnumerator HandleMissCoroutine(bool isTimeout = false)
    {
        if (!isWaitingForInput) yield break; //guarda
        isWaitingForInput = false;

        //se o erro nao foi por timeout, a corrotina prompt ainda esta rodando
        if (!isTimeout && _promptCoroutine != null) StopCoroutine(_promptCoroutine);

        if (currentButtonObject) Destroy(currentButtonObject);

        yield return new WaitForEndOfFrame();

        misses++;
        SoundEventArgs sfxArgs = new()
        {
            Category = SoundEventArgs.SoundCategory.SFX,
            AudioID = "erro", // O ID do seu SFX (sem "sfx_" e em minúsculas)
            VolumeScale = 1f, // Escala de volume (opcional, padrão é 1f)
            Pitch = .8f

        };
        SoundEvent.RequestSound(sfxArgs);

        UpdateLife();

        if (misses >= maxMisses)
        {
            EndGame();
        }
        else
        {
            _promptCoroutine = StartCoroutine(ShowNextButtonCoroutine());
        }
    }

    private void EndGame()
    {
        if (!isGameActive) return;
        isGameActive = false;
        if (_promptCoroutine != null) StopCoroutine(_promptCoroutine);
        if (currentButtonObject != null) Destroy(currentButtonObject);

        MinigamePerformance performance;
        if (bucketsFilled < 1) performance = MinigamePerformance.Fail;
        else if (bucketsFilled < 2) performance = MinigamePerformance.Bronze;
        else if (bucketsFilled < 3) performance = MinigamePerformance.Silver;
        else performance = MinigamePerformance.Gold;

        ConcludeMinigame(performance);
    }
}