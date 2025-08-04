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
                    HandleTaskActionState(ref data, WorkerState.MovingToWorkstation, WorkerAction.CollectItem, index);
                    break;
                case WorkerState.MovingToWorkstation:
                    HandleMovementState(ref data, WorkerState.Working, WorkerAction.WorkAtStation, index);
                    break;
                case WorkerState.Working:
                    HandleTaskActionState(ref data, WorkerState.MovingToDestination, WorkerAction.WorkAtStation, index);
                    break;
                case WorkerState.MovingToDestination:
                    HandleMovementState(ref data, WorkerState.DeliveringItem, WorkerAction.DeliverItem, index);
                    break;
                case WorkerState.DeliveringItem:
                    HandleTaskActionState(ref data, WorkerState.Idle, WorkerAction.DeliverItem, index, true);
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

        private void HandleTaskActionState(ref WorkerData data, WorkerState nextState, WorkerAction currentAction, int workerIndex, bool isFinalStep = false)
        {
            data.workTimer += deltaTime;
            workerActions[workerIndex] = currentAction;

            if (data.workTimer >= data.actionDuration)
            {
                if (isFinalStep)
                {
                    workerActions[workerIndex] = WorkerAction.TaskCompleted;
                    data.currentState = WorkerState.Idle;
                    data.currentTaskID = -1;
                    data.isCarryingItem = false;
                    data.inventoryCount = 0;
                    data.carriedItems.Clear();
                }
                else
                {
                    if (ActiveTasks.TryGetValue(data.currentTaskID, out var task))
                    {
                        int nextTargetID = GetNextTargetID(data.currentState, task);
                        if (nextTargetID != -1 && stationPositions.TryGetValue(nextTargetID, out float3 nextPos))
                        {
                            data.moveTarget = nextPos;
                            data.currentState = nextState;
                            data.workTimer = 0f;
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

        private int GetNextTargetID(WorkerState currentState, WorkerTask task)
        {
            switch (currentState)
            {
                case WorkerState.CollectingItem:
                    return (task.type == TaskType.Harvest) ? task.destinationID : task.workstationID;
                case WorkerState.Working:
                    return task.destinationID;
                default:
                    return -1;
            }
        }
    }
}