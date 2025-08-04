using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public static class WorkerTaskFactory
{
    private static int nextTaskId = 1;

    public static WorkerTask CreateHarvestTask(int productionAreaID, int storageID, int productID)
    {
        return new WorkerTask
        {
            taskID = nextTaskId++,
            type = TaskType.Harvest,
            status = TaskStatus.Pending,
            requiredWorkerType = WorkerType.Fazendeiro,
            originID = productionAreaID,
            destinationID = storageID,
            outputItemID = productID,
            estimatedDuration = 5f,
            priority = 1.0f,
            requiredIngredients = new FixedList128Bytes<int>() // Inicializar vazio
        };
    }

    public static WorkerTask CreateRefineTask(int rawStorageID, int machineID, int refinedStorageID,
                                            int rawProductID, int refinedProductID)
    {
        return new WorkerTask
        {
            taskID = nextTaskId++,
            type = TaskType.Refine,
            status = TaskStatus.Pending,
            requiredWorkerType = WorkerType.Repositor,
            originID = rawStorageID,
            workstationID = machineID,
            destinationID = refinedStorageID,
            inputItemID = rawProductID,
            outputItemID = refinedProductID,
            estimatedDuration = 8f,
            priority = 1.0f,
            requiredIngredients = new FixedList128Bytes<int>() // Inicializar vazio
        };
    }

    public static WorkerTask CreateDrinkTask(int orderID, List<int> ingredientIDs,
                                           int coffeeStationID, int deliveryPointID, int drinkID)
    {
        var task = new WorkerTask
        {
            taskID = nextTaskId++,
            type = TaskType.CreateDrink,
            status = TaskStatus.Pending,
            requiredWorkerType = WorkerType.Barista,
            workstationID = coffeeStationID,
            destinationID = deliveryPointID,
            outputItemID = drinkID,
            drinkOrderID = orderID,
            estimatedDuration = 10f,
            priority = 1.0f,
            requiredIngredients = new FixedList128Bytes<int>()
        };

        // Adicionar ingredientes à FixedList com verificação de capacidade
        foreach (int ingredientID in ingredientIDs)
        {
            if (task.requiredIngredients.Length < task.requiredIngredients.Capacity)
            {
                task.requiredIngredients.Add(ingredientID);
            }
            else
            {
                Debug.LogWarning($"FixedList capacity exceeded. Maximum ingredients: {task.requiredIngredients.Capacity}");
                break;
            }
        }

        return task;
    }
}