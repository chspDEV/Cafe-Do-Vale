using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tcp4.Assets.Resources.Scripts.Managers;
public class PlaytestManager : Singleton<PlaytestManager>
{
#pragma warning disable 0649
    [Header("Configuracoes do Heatmap")]
    [Tooltip("O Prefab da esfera que sera instanciada.")]
    [SerializeField] private GameObject heatmapSpherePrefab;
    [Tooltip("A camera de cima que vai tirar a foto do mapa.")]
    [SerializeField] private Camera playtestCamera;
    [Tooltip("O objeto do jogador para rastrear a posicao.")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("Com que frequencia (em segundos) uma nova esfera eh criada.")]
    [SerializeField] private float logInterval = 1.0f;

    [Header("Configuracoes do Relatorio")]
    [Tooltip("Nome base para os arquivos de relatorio.")]
    [SerializeField] private string reportBaseName = "PlaytestReport";
    private int clientsServed = 0;
    private int clientsMissed = 0;
    private float moneyGained = 0f;
    private float moneySpent = 0f;
    private float reputationGained = 0f;
    private float reputationLost = 0f;
    private float startTime;


    private List<GameObject> spawnedSpheres = new List<GameObject>();
    [SerializeField] private bool canSavePlaytest;

    private void Start()
    {
        if (!canSavePlaytest) return;
        startTime = Time.time;
        playerTransform = GameAssets.Instance.player.transform;
        InvokeRepeating(nameof(LogPlayerPosition), 0, logInterval);
    }
    private void OnApplicationQuit()
    {
        if (!canSavePlaytest) return;
        GenerateReportAndScreenshot();
    }

    #region METODOS PARA SALVAR DADOS DE PLAYTEST
    public void RecordClientServed(int amount = 1) => clientsServed += amount;
    public void RecordClientMissed(int amount = 1) => clientsMissed += amount;
    public void AddMoney(float amount) => moneyGained += amount;
    public void AddSpentMoney(float amount) => moneySpent += amount;
    public void AddReputation(float amount) => reputationGained += amount;
    public void LoseReputation(float amount) => reputationLost += amount;
    #endregion


    private void LogPlayerPosition()
    {
        if (playerTransform != null && heatmapSpherePrefab != null)
        {
            Instantiate(heatmapSpherePrefab, playerTransform.position, Quaternion.identity, transform);
        }
    }
    public void GenerateReportAndScreenshot()
    {
        float totalPlayTime = (Time.time - startTime) / 60.0f;
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string reportFileName = $"{reportBaseName}_{timestamp}.txt";
        string imageFileName = $"{reportBaseName}_Heatmap_{timestamp}.png";
        string buildRootPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string reportsFolderPath = Path.Combine(buildRootPath, "PlaytestReports");
        Directory.CreateDirectory(reportsFolderPath);
        string reportPath = Path.Combine(reportsFolderPath, reportFileName);
        string imagePath = Path.Combine(reportsFolderPath, imageFileName);

        CaptureHeatmap(imagePath);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== RELATORIO DE PLAYTEST =====");
        sb.AppendLine($"Data e Hora: {timestamp}");
        sb.AppendLine($"Tempo de Jogo: {totalPlayTime:F2} minutos");
        sb.AppendLine();
        sb.AppendLine("--- Atendimento ---");
        sb.AppendLine($"Clientes Atendidos: {clientsServed}");
        sb.AppendLine($"Clientes Nao Atendidos: {clientsMissed}");
        sb.AppendLine();
        sb.AppendLine("--- Financas ---");
        sb.AppendLine($"Dinheiro Ganho: ${moneyGained:F2}");
        sb.AppendLine($"Dinheiro Gasto: ${moneySpent:F2}");
        float dinheiroFinal = moneyGained - moneySpent;
        if (dinheiroFinal < 0) dinheiroFinal = 0f;
        sb.AppendLine($"Dinheiro Final: ${dinheiroFinal:F2}");
        sb.AppendLine();
        sb.AppendLine("--- Reputacao ---");
        sb.AppendLine($"Reputacao Ganha: {reputationGained}");
        sb.AppendLine($"Reputacao Perdida: {reputationLost}");
        float repFinal = reputationGained - reputationLost;
        if (repFinal < 0) repFinal = 0f;
        sb.AppendLine($"Reputacao Final: {repFinal}");
        sb.AppendLine("===============================");
        File.WriteAllText(reportPath, sb.ToString());

        Debug.Log($"<color=lime>Relatorio de playtest salvo em: {reportPath}</color>");
    }
    private void CaptureHeatmap(string path)
    {
        if (playtestCamera == null)
        {
            Debug.LogError("Camera de topo nao foi definida no PlaytestManager!");
            return;
        }
        RenderTexture rt = new RenderTexture(1920, 1080, 24);
        playtestCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        playtestCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        playtestCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Debug.Log($"<color=cyan>Heatmap salvo em: {path}</color>");
    }
}