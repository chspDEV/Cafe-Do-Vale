using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Managers;

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
    [SerializeField] private int maxCycles = 3;
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

    private List<ActiveZone> activeZones = new List<ActiveZone>();

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
        misses = 0;

        //prepara o primeiro ciclo e inicia o jogo
        GenerateNewTargetZones();
        isGameActive = true;

    }

    private void Update()
    {
        if (!isGameActive) return;

        HandleRotation();
        HandleInput();
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

            //gera novas zonas para o proximo ciclo
            GenerateNewTargetZones();
        }

        //atualiza a rotacao do indicador visual
        rotatingIndicator.rotation = Quaternion.Euler(0, 0, -currentAngle);
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
                currentSpeed += speedIncreasePerHit;
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
        float normalizedEndA = endA < startA ? endA + 360 : endA;
        float normalizedEndB = endB < startB ? endB + 360 : endB;

        bool overlaps = (startA < normalizedEndB && endA > startB) ||
                        (startA < (normalizedEndB + 360) && (normalizedEndA) > (startB + 360)) ||
                        ((startA + 360) < normalizedEndB && (normalizedEndA + 360) > startB);

        return overlaps;
    }

    private void GenerateNewTargetZones()
    {
        //limpa as zonas antigas
        foreach (var zone in activeZones)
        {
            Destroy(zone.zoneObject);
        }
        activeZones.Clear();

        UpdateLife();

        const int maxPlacementTries = 50; //evita loops infinitos

        //cria as novas zonas
        for (int i = 0; i < zonesPerCycle; i++)
        {
            bool placedSuccessfully = false;
            for (int tryCount = 0; tryCount < maxPlacementTries; tryCount++)
            {
                TargetZoneDifficulty randomDifficulty = difficulties[Random.Range(0, difficulties.Count)];
                float size = randomDifficulty.angularSize;
                float start = Random.Range(0f, 360f);
                float end = start + size;

                bool overlapsWithOthers = false;
                //verifica se a nova zona colide com alguma ja existente
                foreach (var existingZone in activeZones)
                {
                    if (DoZonesOverlap(start, end, existingZone.startAngle, existingZone.endAngle))
                    {
                        overlapsWithOthers = true;
                        break;
                    }
                }

                //se nao ha sobreposicao, cria a zona
                if (!overlapsWithOthers)
                {
                    var newZone = new ActiveZone
                    {
                        startAngle = start,
                        endAngle = end, //end pode ser > 360. isso e importante.
                        angularSize = size,
                        hit = false
                    };

                    //cria o objeto visual
                    GameObject zoneObj = Instantiate(targetZonePrefab, zonesContainer);
                    Image zoneImage = zoneObj.GetComponent<Image>();
                    zoneImage.fillAmount = size / 360f;
                    zoneObj.transform.rotation = Quaternion.Euler(0, 0, -start);

                    newZone.zoneObject = zoneObj;
                    activeZones.Add(newZone);

                    placedSuccessfully = true;
                    break; //sai do loop de tentativas e vai para a proxima zona
                }
            }

            if (!placedSuccessfully)
            {
                Debug.LogWarning("nao foi possivel posicionar uma zona sem sobreposicao. o circulo pode estar muito cheio.");
            }
        }
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

    #endregion
}