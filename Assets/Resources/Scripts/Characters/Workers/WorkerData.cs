using Unity.Collections;
using Unity.Mathematics;

public enum TaskType
{
    Harvest,        // Fazendeiro: coletar produção pronta
    Refine,         // Repositor: beneficiar produto bruto
    CreateDrink     // Barista: criar bebida
}

public enum TaskStatus { Pending, InProgress, Completed, Failed }

public enum WorkerType { Fazendeiro, Barista, Repositor }

public enum WorkerState
{
    Idle,                   // Procurando tarefa
    MovingToOrigin,         // Indo para o local de origem (antigo MovingToPickup)
    CollectingItem,         // Coletando item (antigo PickingUpItem)
    MovingToWorkstation,    // Indo para estação de trabalho
    Working,                // Trabalhando na estação
    MovingToDestination,    // Indo para destino final (antigo MovingToDropoff)
    DeliveringItem,         // Entregando item (antigo DroppingOffItem)
    Resting,
    GoingHome,
    OffDuty
}

public enum WorkerAction
{
    None,
    MoveToTarget,
    CollectItem,            // Antigo ExecuteTaskAction para coleta
    WorkAtStation,          // Antigo ExecuteTaskAction para trabalho
    DeliverItem,            // Antigo ExecuteTaskAction para entrega
    TaskCompleted,
    TaskFailed
}

// --- Estruturas de Dados ---
public struct WorkerTask
{
    public int taskID;
    public TaskType type;
    public TaskStatus status;
    public WorkerType requiredWorkerType;

    // Localizações
    public int originID;            // De onde pegar o item
    public int workstationID;       // Onde trabalhar (se necessário)
    public int destinationID;       // Para onde levar o resultado

    // Produtos
    public int inputItemID;         // Item necessário para a tarefa
    public int outputItemID;        // Item que será produzido

    // Para Barista - múltiplos ingredientes usando FixedList ao invés de NativeArray
    public FixedList128Bytes<int> requiredIngredients; // IDs dos ingredientes necessários
    public int drinkOrderID;        // ID do pedido específico

    // Timing
    public float estimatedDuration;
    public float priority;          // Para ordenação de tarefas
}

public struct WorkerData
{
    public int id;
    public WorkerType type;
    public WorkerState currentState;
    public WorkerState previousState; // Para debug e recuperação de erros

    // Posicionamento
    public float3 currentPosition;
    public float3 moveTarget;

    // Tarefa atual
    public int currentTaskID;
    public int carriedItemID;
    public FixedList128Bytes<int> carriedItems;

    // Timers
    public float workTimer;
    public float actionDuration;
    public float stuckTimer;        // Para detectar quando o worker está preso

    // Status
    public bool isActive;
    public bool isHired;
    public bool isCarryingItem;
    public int inventoryCount;      // Quantos itens está carregando

    // Performance
    public float efficiency;        // Multiplicador de velocidade/qualidade
    public int tasksCompleted;
    public int tasksFailed;

    // Estados específicos por tipo
    public WorkerSpecificData specificData;

    public int dailyCost;
    public float pauseChance;
    public float workDuration;
    public float restDuration;
    public float3 homePosition;

    public bool isWorkingTime { get; internal set; }
}

// Dados específicos por tipo de trabalhador
public struct WorkerSpecificData
{
    // Fazendeiro
    public int harvestExperience;

    // Repositor
    public int refiningExperience;
    public int preferredMachineID; // Máquina preferida para eficiência

    // Barista
    public int drinksCreated;
    public int currentOrderStep;    // Qual ingrediente está buscando
    public bool hasAllIngredients;  // Se já coletou todos os ingredientes
}