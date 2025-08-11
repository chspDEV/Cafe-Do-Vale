using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Tcp4
{
    [BurstCompile]
    public struct WorkerDecisionJob : IJobParallelFor
    {
        public NativeArray<WorkerData> workerDataArray;
        [WriteOnly] public NativeArray<WorkerAction> workerActions;

        [ReadOnly] public NativeHashMap<int, WorkerTask> ActiveTasks;
        [ReadOnly] public NativeHashMap<int, float3> stationPositions;
        [ReadOnly] public NativeArray<FixedList128Bytes<int>> allCarriedItems;

        [ReadOnly] public float deltaTime;
        [ReadOnly] public bool isWorkingTime;

        public void Execute(int index)
        {
            WorkerData data = workerDataArray[index];
            if (!data.isActive || !data.isHired)
            {
                workerActions[index] = WorkerAction.None;
                return;
            }

            data.carriedItems = allCarriedItems[index];

            if (!isWorkingTime && data.currentState != WorkerState.GoingHome && data.currentState != WorkerState.OffDuty)
            {
                if (data.currentTaskID != -1)
                {
                    workerActions[index] = WorkerAction.TaskFailed;
                    data.currentTaskID = -1;
                    data.isCarryingItem = false;
                    data.inventoryCount = 0;
                }
                data.currentState = WorkerState.GoingHome;
                data.moveTarget = data.homePosition;
            }

            switch (data.currentState)
            {
                case WorkerState.Idle:
                    HandleIdleState(ref data, index);
                    break;
                case WorkerState.MovingToOrigin:
                    HandleMovementState(ref data, WorkerState.CollectingItem, WorkerAction.CollectItem, index);
                    break;
                case WorkerState.CollectingItem:
                    HandleTaskActionState(ref data, WorkerAction.CollectItem, index);
                    break;
                case WorkerState.MovingToWorkstation:
                    HandleMovementState(ref data, WorkerState.Working, WorkerAction.WorkAtStation, index);
                    break;
                case WorkerState.Working:
                    HandleTaskActionState(ref data, WorkerAction.WorkAtStation, index);
                    break;
                case WorkerState.MovingToDestination:
                    HandleMovementState(ref data, WorkerState.DeliveringItem, WorkerAction.DeliverItem, index);
                    break;
                case WorkerState.DeliveringItem:
                    HandleTaskActionState(ref data, WorkerAction.DeliverItem, index, true);
                    break;
                case WorkerState.GoingHome:
                    HandleGoingHomeState(ref data, index);
                    break;
                case WorkerState.Resting:
                    HandleRestingState(ref data, index);
                    break;
            }

            workerDataArray[index] = data;
        }

        private void HandleIdleState(ref WorkerData data, int workerIndex)
        {
            if (data.currentTaskID != -1)
            {
                if (ActiveTasks.TryGetValue(data.currentTaskID, out var task))
                {
                    if (stationPositions.TryGetValue(task.originID, out float3 originPos))
                    {
                        data.moveTarget = originPos;
                        data.currentState = WorkerState.MovingToOrigin;
                        workerActions[workerIndex] = WorkerAction.MoveToTarget;
                    }
                    else
                    {
                        workerActions[workerIndex] = WorkerAction.TaskFailed;
                        data.currentTaskID = -1;
                    }
                }
                else
                {
                    workerActions[workerIndex] = WorkerAction.TaskFailed;
                    data.currentTaskID = -1;
                }
                return;
            }

            data.workTimer += deltaTime;
            if (data.workTimer >= data.workDuration && data.restDuration > 0)
            {
                data.currentState = WorkerState.Resting;
                data.workTimer = 0f;
            }
            workerActions[workerIndex] = WorkerAction.None;
        }

        private void HandleMovementState(ref WorkerData data, WorkerState nextState, WorkerAction actionOnArrival, int workerIndex)
        {
            if (math.distance(data.currentPosition, data.moveTarget) < 1.5f)
            {
                data.currentState = nextState;
                data.workTimer = 0f;
                workerActions[workerIndex] = actionOnArrival;
            }
            else
            {
                workerActions[workerIndex] = WorkerAction.MoveToTarget;
            }
        }

        // Em WorkerDecisionJob.cs

        private void HandleTaskActionState(ref WorkerData data, WorkerAction currentAction, int workerIndex, bool isFinalStep = false)
        {
            data.workTimer += deltaTime;
            workerActions[workerIndex] = currentAction;

            // CONDIÇÃO DE TRANSIÇÃO ORIGINAL (PROBLEMA)
            // if (data.workTimer >= data.actionDuration)

            // CORREÇÃO:
            // A transição só deve ocorrer se o tempo de ação acabou E a ação teve o efeito esperado.
            // Para coleta (CollectingItem), o trabalhador deve estar carregando algo.
            // Para entrega (DeliveringItem), ele não deve mais estar carregando nada.
            bool actionConfirmed = (data.currentState == WorkerState.CollectingItem && data.isCarryingItem) ||
                                   (data.currentState == WorkerState.Working && data.isCarryingItem) || // Para o caso de Refine
                                   (data.currentState == WorkerState.DeliveringItem && !data.isCarryingItem);

            if (data.workTimer >= data.actionDuration && (actionConfirmed || isFinalStep))
            {
                if (isFinalStep)
                {
                    workerActions[workerIndex] = WorkerAction.TaskCompleted;
                    data.currentState = WorkerState.Idle;
                    data.currentTaskID = -1;
                    // Limpar dados do item carregado é feito pelo ExecuteDeliverAction agora.
                }
                else
                {
                    if (ActiveTasks.TryGetValue(data.currentTaskID, out var task))
                    {
                        (WorkerState nextState, int nextTargetID) = GetNextStateAndTarget(data.currentState, task);

                        if (nextTargetID != -1 && stationPositions.TryGetValue(nextTargetID, out float3 nextPos))
                        {
                            data.moveTarget = nextPos;
                            data.currentState = nextState;
                            data.workTimer = 0f;
                            workerActions[workerIndex] = WorkerAction.MoveToTarget;
                        }
                        else
                        {
                            workerActions[workerIndex] = WorkerAction.TaskFailed;
                            data.currentState = WorkerState.Idle;
                            data.currentTaskID = -1;
                        }
                    }
                    else
                    {
                        workerActions[workerIndex] = WorkerAction.TaskFailed;
                        data.currentState = WorkerState.Idle;
                        data.currentTaskID = -1;
                    }
                }
            }
        }

        private void HandleGoingHomeState(ref WorkerData data, int workerIndex)
        {
            if (math.distance(data.currentPosition, data.homePosition) < 1.5f)
            {
                data.currentState = WorkerState.OffDuty;
                workerActions[workerIndex] = WorkerAction.None;
            }
            else
            {
                data.moveTarget = data.homePosition;
                workerActions[workerIndex] = WorkerAction.MoveToTarget;
            }
        }

        private void HandleRestingState(ref WorkerData data, int workerIndex)
        {
            data.workTimer += deltaTime;
            if (data.workTimer >= data.restDuration)
            {
                data.currentState = WorkerState.Idle;
                data.workTimer = 0f;
            }
            workerActions[workerIndex] = WorkerAction.None;
        }

        private (WorkerState, int) GetNextStateAndTarget(WorkerState currentState, WorkerTask task)
        {
            switch (currentState)
            {
                case WorkerState.CollectingItem:
                    if (task.type == TaskType.Harvest)
                    {
                        return (WorkerState.MovingToDestination, task.destinationID);
                    }
                    else
                    {
                        return (WorkerState.MovingToWorkstation, task.workstationID);
                    }
                case WorkerState.Working:
                    return (WorkerState.MovingToDestination, task.destinationID);

                default:
                    return (WorkerState.Idle, -1);
            }
        }
    }
}