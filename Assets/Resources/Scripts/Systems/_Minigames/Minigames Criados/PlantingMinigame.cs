using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Managers;
using GameResources.Project.Scripts.Utilities.Audio;
using TMPro;

//define as propriedades de uma dificuldade de zona de acerto
[System.Serializable]
public class TargetZoneDifficulty
{
    public string name; //"facil", "medio", "dificil"
    [Range(10, 90)]
    public float angularSize = 45f; //o tamanho da zona em graus
}

//armazena os dados de uma zona de acerto ativa durante o jogo
public class ActiveZone
{
    public float startAngle;
    public float endAngle;
    public float angularSize;
    public GameObject zoneObject;
    public bool hit = false; //marca se esta zona ja foi acertada
}

public class PlantingMinigame : BaseMinigame
{
    [Header("Game Objects")]
    [SerializeField] private RectTransform rotatingIndicator;
    [SerializeField] private GameObject targetZonePrefab; 
    [SerializeField] private GameObject lifePrefab; 
    [SerializeField] private Image centerIcon; 
    [SerializeField] private Transform zonesContainer; 
    [SerializeField] private Transform lifeContainer; 
    [SerializeField] private Animator handsAnimator; 
    [SerializeField] private Animator inputToPress;

    [Header("Game Rules")]
    private PlugInputPack.InputAccessor inputKey;
    private int maxCycles = 3;
    [SerializeField] private int maxMisses = 2;
    [SerializeField] private int zonesPerCycle = 3;



    [Header("Gameplay")]
    [SerializeField] private float initialSpeed = 90f; //velocidade inicial em graus/segundo
    [SerializeField] private float speedIncreasePerHit = 15f; //quanto a velocidade aumenta

    [Header("Zone Difficulties")]
    [SerializeField] private List<TargetZoneDifficulty> difficulties;

    private List<GameObject> spawnedLifes = new();

    //estado do jogo
    private float currentAngle = 0f;
    private float currentSpeed;
    private int cyclesCompleted = 0;
    private int totalHits = 0;
    private int misses = 0;
    private bool isGameActive = false;
    private float currentPitch = 1f;

    private List<ActiveZone> activeZones = new List<ActiveZone>();
    private bool canRotate = true;
    [SerializeField] private TextMeshProUGUI counter;

    public void SetupMinigameIcon(Sprite newIcon)
    {
        centerIcon.sprite = newIcon;
    }

    #region --- Ciclo de Vida do Minigame ---

    public override void StartMinigame()
    {
        StartCoroutine(nameof(LoadingMinigame));
    }

    IEnumerator LoadingMinigame()
    {
        yield return new WaitForSeconds(1.5f);
        inputKey = GameAssets.Instance.inputComponent["Interact"];
        //reseta o estado inicial
        currentSpeed = initialSpeed;
        currentAngle = 0f;
        cyclesCompleted = 0;
        totalHits = 0;
        maxCycles = Random.Range(1, 3);
        misses = 0;
        UpdateCounterText();

        SetupMinigameIcon(GameAssets.Instance.lastProductionSprite);

        //prepara o primeiro ciclo e inicia o jogo
        GenerateNewTargetZones();
        isGameActive = true;

    }

    void UpdateCounterText()
    {
        counter.text = totalHits.ToString() + "/ " + maxCycles * 3;
    }

    private void Update()
    {
        if (!isGameActive) return;

        HandleRotation();
        HandleInput();
        HandleInputToPress();
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

        for (int i = 0; i < maxMisses - misses; i++)
        {

            spawnedLifes.Add(Instantiate(lifePrefab, lifeContainer));
        }
    }

    #endregion

    #region --- Lógica do Jogo ---

    private void HandleRotation()
    {
        if (!canRotate) return;

        //calcula o novo angulo
        currentAngle += currentSpeed * Time.deltaTime;

        //verifica se completou um ciclo
        if (currentAngle >= 360f)
        {
            currentAngle -= 360f;
            cyclesCompleted++;

            //verifica condicao de fim por ciclos
            if (cyclesCompleted >= maxCycles)
            {
                EndGame();
                return;
            }

            StartCoroutine(nameof(WaitForNextCycle));
        }

        //atualiza a rotacao do indicador visual
        rotatingIndicator.rotation = Quaternion.Euler(0, 0, -currentAngle);
    }

    IEnumerator WaitForNextCycle()
    {
        canRotate = false;
        currentAngle = 0;
        GenerateNewTargetZones();

        yield return new WaitForSeconds(1f);

        canRotate = true;
    }

    private void HandleInputToPress()
    {
        if (CheckForActiveInput())
        {
            if (inputToPress) inputToPress.Play("canPress_inputToPress");
        }
    }



    private void HandleInput()
    {
        if (inputKey.Pressed)
        {
            bool hitSuccess = CheckForHit();

            if (hitSuccess)
            {
                //sucesso
                totalHits++;

                //Fazendo o request de sfx
                SoundEventArgs sfxArgs = new()
                {
                    Category = SoundEventArgs.SoundCategory.SFX,
                    AudioID = "interacao", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                    VolumeScale = 1f, // Escala de volume (opcional, padrão é 1f)
                    Pitch = currentPitch

                };
                SoundEvent.RequestSound(sfxArgs);

                UpdateCounterText();

                currentPitch += 0.1f;
                StartCoroutine(IncreaseSpeed(speedIncreasePerHit));
                Debug.Log($"acerto! total de acertos: {totalHits}");
                if (handsAnimator) handsAnimator.Play("hands_get");
                if (inputToPress) inputToPress.Play("active_inputToPress");
            }
            else
            {
                //erro
                misses++;
                UpdateLife();
                Debug.LogWarning($"erro! total de erros: {misses}");
                //verifica condicao de fim por erros
                if (misses >= maxMisses)
                {
                    EndGame();
                }
            }
        }
    }

    IEnumerator IncreaseSpeed(float increase)
    {
        var newSpeed = currentSpeed + increase;

        while (currentSpeed < newSpeed) 
        { 
            currentSpeed += 0.5f;
        }

        currentSpeed = newSpeed;

        yield return null;
    }

    private void EndGame()
    {
        isGameActive = false;
        Debug.Log("fim do minigame! total de acertos: " + totalHits);

        //determina a performance final baseada no numero de acertos
        MinigamePerformance performance;
        if (totalHits == 0)
            performance = MinigamePerformance.Fail;
        else if (totalHits <= 3)
            performance = MinigamePerformance.Bronze;
        else if (totalHits <= 6)
            performance = MinigamePerformance.Silver;
        else
            performance = MinigamePerformance.Gold;

        //usa o metodo da classe base para concluir e entregar a recompensa
        ConcludeMinigame(performance);
    }

    #endregion

    #region --- Lógica das Zonas de Acerto ---

    private bool DoZonesOverlap(float startA, float endA, float startB, float endB)
    {
        float normalizedStartA = Mathf.Repeat(startA, 360f);
        float normalizedEndA = Mathf.Repeat(endA, 360f);
        float normalizedStartB = Mathf.Repeat(startB, 360f);
        float normalizedEndB = Mathf.Repeat(endB, 360f);

        // Lida com casos onde zona passa de 360 -> 0
        List<(float, float)> rangesA = SplitIfWraps(normalizedStartA, normalizedEndA);
        List<(float, float)> rangesB = SplitIfWraps(normalizedStartB, normalizedEndB);

        foreach (var (sa1, ea1) in rangesA)
        {
            foreach (var (sa2, ea2) in rangesB)
            {
                if (sa1 < ea2 && ea1 > sa2)
                    return true;
            }
        }

        return false;
    }

    // Divide uma zona em dois ranges se ela passa por 0°
    private List<(float, float)> SplitIfWraps(float start, float end)
    {
        if (end >= start)
            return new List<(float, float)> { (start, end) };
        else
            return new List<(float, float)>
        {
            (start, 360f),
            (0f, end)
        };
    }


    private void GenerateNewTargetZones()
    {
        // Limpa zonas anteriores
        foreach (var zone in activeZones)
        {
            Destroy(zone.zoneObject);
        }
        activeZones.Clear();

        UpdateLife();

        const int maxPlacementTries = 100;
        const int maxSafeRetries = 3;

        int retries = 0;

        while (retries < maxSafeRetries)
        {
            activeZones.Clear();
            int placedCount = 0;

            for (int i = 0; i < zonesPerCycle; i++)
            {
                bool placed = false;

                for (int tryCount = 0; tryCount < maxPlacementTries; tryCount++)
                {
                    var difficulty = difficulties[Random.Range(0, difficulties.Count)];
                    float size = difficulty.angularSize;
                    float start = Random.Range(0f, 360f);
                    float end = start + size;

                    float offset = Mathf.Min(10f, size / 2f); // menor offset se a zona for pequena

                    bool overlaps = false;

                    foreach (var other in activeZones)
                    {
                        if (ZonesOverlap360(start - offset, end + offset, other.startAngle - offset, other.endAngle + offset))
                        {
                            overlaps = true;
                            break;
                        }
                    }

                    if (!overlaps)
                    {
                        var newZone = new ActiveZone
                        {
                            startAngle = start,
                            endAngle = end,
                            angularSize = size,
                            hit = false
                        };

                        GameObject zoneObj = Instantiate(targetZonePrefab, zonesContainer);
                        var img = zoneObj.GetComponent<Image>();
                        img.fillAmount = size / 360f;
                        zoneObj.transform.rotation = Quaternion.Euler(0, 0, -start);
                        newZone.zoneObject = zoneObj;
                        activeZones.Add(newZone);

                        placed = true;
                        placedCount++;
                        break;
                    }
                }
            }

            if (placedCount == zonesPerCycle)
                return; // sucesso total

            retries++;
        }

        Debug.LogWarning("Falha ao posicionar zonas. Reduza 'zonesPerCycle' ou aumente espaço.");
    }

    private bool ZonesOverlap360(float startA, float endA, float startB, float endB)
    {
        startA = Mathf.Repeat(startA, 360f);
        endA = Mathf.Repeat(endA, 360f);
        startB = Mathf.Repeat(startB, 360f);
        endB = Mathf.Repeat(endB, 360f);

        if (endA < startA) endA += 360f;
        if (endB < startB) endB += 360f;

        return !(endA <= startB || endB <= startA);
    }

    private bool CheckForHit()
    {
        foreach (var zone in activeZones)
        {
            if (zone.hit) continue;

            //aqui esta a logica corrigida e confiavel:
            //1. calcula o centro da zona. se start=350 e end=390 (tamanho 40), o centro e 370.
            //   mathf.deltaangle entende que 370 e o mesmo que 10 graus.
            float zoneCenterAngle = (zone.startAngle + zone.endAngle) / 2f;

            //2. calcula a menor distancia angular entre o indicador e o centro da zona
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, zoneCenterAngle));

            //3. a tolerancia para o acerto e metade do tamanho total da zona
            float tolerance = zone.angularSize / 2f;

            if (angleDifference <= tolerance)
            {
                zone.hit = true;
                zone.zoneObject.GetComponent<Image>().color = Color.cyan;
                return true;
            }
        }

        return false;
    }

    private bool CheckForActiveInput()
    {
        foreach (var zone in activeZones)
        {
            if (zone.hit) continue;

            //aqui esta a logica corrigida e confiavel:
            //1. calcula o centro da zona. se start=350 e end=390 (tamanho 40), o centro e 370.
            //   mathf.deltaangle entende que 370 e o mesmo que 10 graus.
            float zoneCenterAngle = (zone.startAngle + zone.endAngle) / 2f;

            //2. calcula a menor distancia angular entre o indicador e o centro da zona
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, zoneCenterAngle));

            //3. a tolerancia para o acerto e metade do tamanho total da zona
            float tolerance = zone.angularSize / 2f;

            if (angleDifference <= tolerance)
            {
                return true;
            }
        }

        return false;
    }

    #endregion
}