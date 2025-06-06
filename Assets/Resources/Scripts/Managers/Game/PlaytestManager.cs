using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class PlaytestManager : MonoBehaviour
{
#pragma warning disable 0649 //desabilita o aviso de variavel nunca usada, pois a Unity que as preenche
    //lembre-se de aplicar as alteracoes no codigo que fiz de acordo com o pedido
    //singleton instance
    public static PlaytestManager Instance { get; private set; }

    [Header("Configuracoes do Heatmap")]
    [Tooltip("O Prefab da esfera que sera instanciada.")]
    [SerializeField] private GameObject heatmapSpherePrefab;
    [Tooltip("A camera de cima que vai tirar a foto do mapa.")]
    [SerializeField] private Camera topDownCamera;
    [Tooltip("O objeto do jogador para rastrear a posicao.")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("Com que frequencia (em segundos) uma nova esfera eh criada.")]
    [SerializeField] private float logInterval = 1.0f;

    [Header("Configuracoes do Relatorio")]
    [Tooltip("Nome base para os arquivos de relatorio.")]
    [SerializeField] private string reportBaseName = "PlaytestReport";

    //dados coletados
    private int clientsServed = 0;
    private int clientsMissed = 0;
    private float moneyGained = 0f;
    private float moneySpent = 0f;
    private float reputationGained = 0f;
    private float reputationLost = 0f;
    private float startTime;

    private List<GameObject> spawnedSpheres = new List<GameObject>();

    private void Awake()
    {
        //logica do singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        //inicia o tempo e o log de posicao
        startTime = Time.time;
        InvokeRepeating(nameof(LogPlayerPosition), 0, logInterval);
    }

    private void OnApplicationQuit()
    {
        //cria o relatorio final quando o jogo fecha
        GenerateReportAndScreenshot();
    }

    //metodos publicos para outros scripts chamarem
    public void RecordClientServed(int amount = 1) => clientsServed += amount;
    public void RecordClientMissed(int amount = 1) => clientsMissed += amount;
    public void AddMoney(float amount) => moneyGained += amount;
    public void AddSpentMoney(float amount) => moneySpent += amount;
    public void AddReputation(float amount) => reputationGained += amount;
    public void LoseReputation(float amount) => reputationLost += amount;

    private void LogPlayerPosition()
    {
        //instancia a esfera na posicao do jogador
        if (playerTransform != null && heatmapSpherePrefab != null)
        {
            GameObject sphere = Instantiate(heatmapSpherePrefab, playerTransform.position, Quaternion.identity, transform);
            spawnedSpheres.Add(sphere);
        }
    }

    public void GenerateReportAndScreenshot()
    {
        //calcula o tempo jogado
        float totalPlayTime = (Time.time - startTime) / 60.0f; //em minutos

        //cria um nome unico para o arquivo com data e hora
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string reportFileName = $"{reportBaseName}_{timestamp}.txt";
        string imageFileName = $"{reportBaseName}_Heatmap_{timestamp}.png";

        // --- INICIO DA MODIFICACAO ---

        //pega o caminho da pasta da build (funciona no editor e na build final)
        //no editor, salvara na pasta raiz do projeto (fora de assets)
        //na build, salvara ao lado do .exe
        string buildRootPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        //cria uma subpasta para organizar os relatorios
        string reportsFolderPath = Path.Combine(buildRootPath, "PlaytestReports");
        Directory.CreateDirectory(reportsFolderPath); //cria a pasta se ela nao existir

        //define o caminho completo para os arquivos dentro da nova pasta
        string reportPath = Path.Combine(reportsFolderPath, reportFileName);
        string imagePath = Path.Combine(reportsFolderPath, imageFileName);

        // --- FIM DA MODIFICACAO ---

        //gera a imagem do heatmap
        CaptureHeatmap(imagePath);



        //cria o conteudo do relatorio de texto
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

        //salva o arquivo de texto
        File.WriteAllText(reportPath, sb.ToString());
        Debug.Log($"<color=lime>Relatorio de playtest salvo em: {reportPath}</color>");
    }

    private void CaptureHeatmap(string path)
    {
        if (topDownCamera == null)
        {
            Debug.LogError("Camera de topo nao foi definida no PlaytestManager!");
            return;
        }

        //prepara a camera para renderizar para uma textura
        RenderTexture rt = new RenderTexture(1920, 1080, 24);
        topDownCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);

        //renderiza a camera e le os pixels
        topDownCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);

        //limpa
        topDownCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        //salva o arquivo de imagem
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Debug.Log($"<color=cyan>Heatmap salvo em: {path}</color>");
    }
}