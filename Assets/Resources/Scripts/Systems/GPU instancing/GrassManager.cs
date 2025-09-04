using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GrassManager : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("O Transform do jogador que o sistema de grama seguirá.")]
    public Transform playerTransform;

    [Header("Grid Settings")]
    [Tooltip("Tamanho de cada célula do grid em unidades do mundo.")]
    public int cellSize = 50;
    [Tooltip("Distância de renderização em número de células a partir do jogador.")]
    public int drawDistanceInCells = 3;

    [Header("Grass Settings")]
    public Mesh grassMesh;
    public Material grassMaterial;
    [Tooltip("Quantas instâncias de grama por célula.")]
    public int instancesPerCell = 1000;
    public LayerMask groundLayer;
    public LayerMask excludedLayers;
    public float checkRadius = 0.5f;

    [Header("Naturalness Settings")]
    [Tooltip("Define a variação de altura da grama (Mínimo, Máximo).")]
    public Vector2 heightScaleRange = new Vector2(0.8f, 1.5f);
    [Tooltip("Define a variação de largura da grama (Mínimo, Máximo).")]
    public Vector2 widthScaleRange = new Vector2(0.8f, 1.2f);
    [Tooltip("Ângulo máximo de inclinação da grama para um efeito mais suave.")]
    [Range(0f, 45f)]
    public float maxTiltAngle = 15f;
    [Tooltip("Se ativado, a grama irá se alinhar com a inclinação do terreno.")]
    public bool alignToGroundNormal = true;

    [Header("LOD Settings")]
    [Tooltip("Distâncias para cada nível de LOD. Ex: 30 = LOD0 até 30m.")]
    public float[] lodDistances = { 30f, 60f, 100f };
    [Tooltip("Percentual de instâncias a serem renderizadas para cada LOD. Ex: 1.0 = 100%")]
    [Range(0f, 1f)]
    public float[] lodInstanceRatios = { 1.0f, 0.5f, 0.2f };
    public Mesh[] lodMeshes;

    private Dictionary<Vector2Int, GrassCell> cells = new Dictionary<Vector2Int, GrassCell>();
    private Vector2Int lastPlayerCell;
    private Camera mainCamera;

    void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform não foi atribuído no GrassManager.");
            this.enabled = false;
            return;
        }

        grassMaterial = new Material(grassMaterial);

        mainCamera = Camera.main;
        lastPlayerCell = GetPlayerCell();
        UpdateCells();
    }

    void Update()
    {
        grassMaterial.SetVector("_trackerPosition", playerTransform.position);

        Vector2Int currentPlayerCell = GetPlayerCell();
        if (currentPlayerCell != lastPlayerCell)
        {
            lastPlayerCell = currentPlayerCell;
            UpdateCells();
        }

        foreach (var cell in cells.Values)
        {
            if (cell.IsActive)
            {
                cell.Draw(mainCamera);
            }
        }
    }

    private Vector2Int GetPlayerCell()
    {
        return new Vector2Int(
            Mathf.FloorToInt(playerTransform.position.x / cellSize),
            Mathf.FloorToInt(playerTransform.position.z / cellSize)
        );
    }

    private void UpdateCells()
    {
        HashSet<Vector2Int> requiredCells = new HashSet<Vector2Int>();
        for (int x = -drawDistanceInCells; x <= drawDistanceInCells; x++)
        {
            for (int z = -drawDistanceInCells; z <= drawDistanceInCells; z++)
            {
                requiredCells.Add(new Vector2Int(lastPlayerCell.x + x, lastPlayerCell.y + z));
            }
        }

        List<Vector2Int> cellsToRemove = cells.Keys.Where(c => !requiredCells.Contains(c)).ToList();
        foreach (var cellCoord in cellsToRemove)
        {
            cells[cellCoord].Deactivate();
            cells.Remove(cellCoord);
        }

        foreach (var cellCoord in requiredCells)
        {
            if (!cells.ContainsKey(cellCoord))
            {
                GrassCell newCell = new GrassCell(this, cellCoord);
                cells.Add(cellCoord, newCell);
            }
            cells[cellCoord].Activate();
        }
    }
}