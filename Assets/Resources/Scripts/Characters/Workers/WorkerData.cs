using Unity.Collections;
using Unity.Mathematics;

public enum TaskType
{
    Harvest,        // Fazendeiro: coletar produ��o pronta
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
    MovingToWorkstation,    // Indo para esta��o de trabalho
    Working,                // Trabalhando na esta��o
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

    // Localiza��es
    public int originID;            // De onde pegar o item
    public int workstationID;       // Onde trabalhar (se necess�rio)
    public int destinationID;       // Para onde levar o resultado

    // Produtos
    public int inputItemID;         // Item necess�rio para a tarefa
    public int outputItemID;        // Item que ser� produzido

    // Para Barista - m�ltiplos ingredientes usando FixedList ao inv�s de NativeArray
    public FixedList128Bytes<int> requiredIngredients; // IDs dos ingredientes necess�rios
    public int drinkOrderID;        // ID do pedido espec�fico

    // Timing
    public float estimatedDuration;
    public float priority;          // Para ordena��o de tarefas
}

public struct WorkerData
{
    public int id;
    public WorkerType type;
    public WorkerState currentState;
    public WorkerState previousState; // Para debug e recupera��o de erros

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
    public float stuckTimer;        // Para detectar quando o worker est� preso

    // Status
    public bool isActive;
    public bool isHired;
    public bool isCarryingItem;
    public int inventoryCount;      // Quantos itens est� carregando

    // Performance
    public float efficiency;        // Multiplicador de velocidade/qualidade
    public int tasksCompleted;
    public int tasksFailed;

    // Estados espec�ficos por tipo
    public WorkerSpecificData specificData;

    public int dailyCost;
    public float pauseChance;
    public float workDuration;
    public float restDuration;
    public float3 homePosition;

    public bool isWorkingTime { get; internal set; }
}

// Dados espec�ficos por tipo de trabalhador
public struct WorkerSpecificData
{
    // Fazendeiro
    public int harvestExperience;

    // Repositor
    public int refiningExperience;
    public int preferredMachineID; // M�quina preferida para efici�ncia

    // Barista
    public int drinksCreated;
    public int currentOrderStep;    // Qual ingrediente est� buscando
    public bool hasAllIngredients;  // Se j� coletou todos os ingredientes
}