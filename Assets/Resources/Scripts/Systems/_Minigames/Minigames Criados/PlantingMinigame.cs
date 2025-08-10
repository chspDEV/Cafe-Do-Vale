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

    [Header("Zone Spacing")]
    [SerializeField] private float minimumSpacingBetweenZones = 20f; // Espaçamento mínimo entre zonas em graus

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
        // CORREÇÃO: Só mostra indicador de input quando pode rotacionar (indicador se movendo)
        if (canRotate && CheckForActiveInput())
        {
            if (inputToPress) inputToPress.Play("canPress_inputToPress");
        }
    }

    private void HandleInput()
    {
        // CORREÇÃO: Só aceita input quando o indicador está se movendo
        if (inputKey.Pressed && canRotate)
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
                currentPitch += 0.1f;

                UpdateCounterText();

                StartCoroutine(IncreaseSpeed(speedIncreasePerHit));
                Debug.Log($"acerto! total de acertos: {totalHits}");
                if (handsAnimator) handsAnimator.Play("hands_get");
                if (inputToPress) inputToPress.Play("active_inputToPress");
            }
            else
            {
                //erro
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

    // FUNÇÃO CORRIGIDA: Verifica se duas zonas se sobrepõem com espaçamento mínimo
    private bool DoZonesOverlapWithSpacing(float startA, float endA, float startB, float endB, float minSpacing)
    {
        // Normaliza todos os ângulos para 0-360
        startA = Mathf.Repeat(startA, 360f);
        endA = Mathf.Repeat(endA, 360f);
        startB = Mathf.Repeat(startB, 360f);
        endB = Mathf.Repeat(endB, 360f);

        // Calcula o centro de cada zona usando DeltaAngle para lidar com wrap-around
        float centerA = startA + Mathf.Repeat((endA - startA), 360f) / 2f;
        float centerB = startB + Mathf.Repeat((endB - startB), 360f) / 2f;

        // Calcula o tamanho de cada zona
        float sizeA = Mathf.Repeat((endA - startA), 360f);
        float sizeB = Mathf.Repeat((endB - startB), 360f);

        // Calcula a distância angular mínima entre os centros
        float distanceBetweenCenters = Mathf.Abs(Mathf.DeltaAngle(centerA, centerB));

        // Distância mínima necessária = metade de cada zona + espaçamento
        float minimumDistance = (sizeA / 2f) + (sizeB / 2f) + minSpacing;

        // Se a distância entre centros é menor que a mínima necessária, há sobreposição
        bool overlaps = distanceBetweenCenters < minimumDistance;

        if (overlaps)
        {
            Debug.Log($"SOBREPOSIÇÃO DETECTADA: ZonaA({startA:F1}°-{endA:F1}°, centro:{centerA:F1}°) " +
                     $"vs ZonaB({startB:F1}°-{endB:F1}°, centro:{centerB:F1}°) " +
                     $"- Distância:{distanceBetweenCenters:F1}° < Mínima:{minimumDistance:F1}°");
        }

        return overlaps;
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

        const int maxPlacementTries = 200; // Aumentado para mais tentativas
        const int maxSafeRetries = 5; // Mais tentativas de restart

        int retries = 0;

        while (retries < maxSafeRetries)
        {
            activeZones.Clear();
            bool allZonesPlaced = true;

            // Tenta colocar cada zona
            for (int i = 0; i < zonesPerCycle; i++)
            {
                bool zonePlaced = false;

                // Tenta várias posições para esta zona
                for (int tryCount = 0; tryCount < maxPlacementTries; tryCount++)
                {
                    var difficulty = difficulties[Random.Range(0, difficulties.Count)];
                    float size = difficulty.angularSize;
                    float start = Random.Range(0f, 360f - size); // Garante que não ultrapasse 360°
                    float end = start + size;

                    // Verifica se esta posição conflita com zonas já colocadas
                    bool hasConflict = false;
                    foreach (var existingZone in activeZones)
                    {
                        if (DoZonesOverlapWithSpacing(start, end, existingZone.startAngle, existingZone.endAngle, minimumSpacingBetweenZones))
                        {
                            hasConflict = true;
                            break;
                        }
                    }

                    // Se não há conflito, cria a zona
                    if (!hasConflict)
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

                        zonePlaced = true;
                        Debug.Log($"Zona {i + 1} colocada: {start:F1}° - {end:F1}°");
                        break;
                    }
                }

                // Se não conseguiu colocar esta zona, restart completo
                if (!zonePlaced)
                {
                    Debug.LogWarning($"Não foi possível colocar zona {i + 1}. Reiniciando geração...");
                    allZonesPlaced = false;
                    break;
                }
            }

            // Se todas as zonas foram colocadas com sucesso
            if (allZonesPlaced)
            {
                Debug.Log($"Todas as {zonesPerCycle} zonas foram colocadas com sucesso!");
                return;
            }

            // Limpa as zonas para tentar novamente
            foreach (var zone in activeZones)
            {
                Destroy(zone.zoneObject);
            }
            activeZones.Clear();

            retries++;
        }

        // Se chegou aqui, não conseguiu colocar todas as zonas
        Debug.LogError($"Falha ao posicionar {zonesPerCycle} zonas após {maxSafeRetries} tentativas. " +
                      $"Considere: 1) Reduzir 'zonesPerCycle', 2) Reduzir 'minimumSpacingBetweenZones', " +
                      $"3) Reduzir tamanho das zonas nas dificuldades.");

        // Como fallback, tenta colocar o máximo de zonas possível
        TryPlaceFallbackZones();
    }

    // Função de fallback que tenta colocar o máximo de zonas possível
    private void TryPlaceFallbackZones()
    {
        Debug.Log("Tentando colocação de fallback...");

        const int maxTries = 100;
        int zonesPlaced = 0;

        for (int attempt = 0; attempt < maxTries && zonesPlaced < zonesPerCycle; attempt++)
        {
            var difficulty = difficulties[Random.Range(0, difficulties.Count)];
            float size = difficulty.angularSize;
            float start = Random.Range(0f, 360f - size);
            float end = start + size;

            bool hasConflict = false;
            foreach (var existingZone in activeZones)
            {
                if (DoZonesOverlapWithSpacing(start, end, existingZone.startAngle, existingZone.endAngle, minimumSpacingBetweenZones))
                {
                    hasConflict = true;
                    break;
                }
            }

            if (!hasConflict)
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

                zonesPlaced++;
                Debug.Log($"Zona de fallback {zonesPlaced} colocada: {start:F1}° - {end:F1}°");
            }
        }

        Debug.Log($"Fallback concluído: {zonesPlaced} zonas colocadas de {zonesPerCycle} desejadas.");
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