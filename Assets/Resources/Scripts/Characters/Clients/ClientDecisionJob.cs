using Unity.Jobs;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.Xml.Linq;
using System.Net.NetworkInformation;
using Tcp4.Assets.Resources.Scripts.Managers;
namespace Tcp4
{
    public struct ClientDecisionJob : IJobParallelFor
    {
        public NativeArray<ClientData> clientDataArray;
        [WriteOnly] public NativeArray<ClientAction> actionArray;
        [ReadOnly] public float deltaTime;
        [ReadOnly] public float playerReputation;
        [ReadOnly] public float3 shopEntrancePosition;
        [ReadOnly] public float3 streetEndPosition;
        [ReadOnly] public float3 counterPosition;
        [ReadOnly] public bool isShopOpen;
        [ReadOnly] public float maxQueueTime;
        public void Execute(int index)
        {
            ClientData data = clientDataArray[index];
            ClientAction action = ClientAction.None;
            switch (data.currentState)
            {
                case ClientState.WalkingOnStreet:
                    float distanceFromDoor = math.distance(data.currentPosition, shopEntrancePosition);
                    if (distanceFromDoor > 3f)
                    {
                        data.moveTarget = shopEntrancePosition;
                        action = ClientAction.MoveToTarget;
                    }
                    else
                    {
                        if (isShopOpen)
                        {
                            float reputationFactor = playerReputation * 0.3f;
                            float baseChance = 0.2f;
                            float chanceToEnter = math.clamp(
                                baseChance + reputationFactor,
                                0.1f,
                                0.6f
                            );
                            float randomValue = GetRandomValue(data.id);
                            if (randomValue < chanceToEnter)
                            {
                                data.currentState = ClientState.GoingToQueue;
                                action = ClientAction.MoveToTarget;
                                data.moveTarget = shopEntrancePosition;
                            }
                            else
                            {
                                data.currentState = ClientState.LeavingShop;
                                data.moveTarget = streetEndPosition;
                                action = ClientAction.MoveToTarget;
                            }
                        }
                        else
                        {
                            data.currentState = ClientState.LeavingShop;
                            data.moveTarget = streetEndPosition;
                            action = ClientAction.MoveToTarget;
                        }
                    }
                    break;
                case ClientState.GoingToCounter:
                    if (!data.isShopOpen) { data.currentState = ClientState.LeavingShop; action = ClientAction.MoveToTarget; }
                    float distanceToCounter = math.distance(data.currentPosition, counterPosition);
                    if (distanceToCounter <= 1f)
                    {
                        data.currentState = ClientState.AtCounter;
                        action = ClientAction.None;
                    }
                    else
                    {
                        action = ClientAction.MoveToTarget;
                    }
                    break;
                case ClientState.GoingToQueue:
                    float distanceToEntrance = math.distance(data.currentPosition, shopEntrancePosition);
                    if (!isShopOpen)
                    {
                        data.currentState = ClientState.LeavingShop;
                        action = ClientAction.MoveToTarget;
                    }
                    else if (distanceToEntrance <= 3f && !data.canQueue)
                    {
                        action = ClientAction.RequestQueueSpot;
                    }
                    else if (data.canQueue)
                    {
                        float distanceToQueueSpot = math.distance(data.currentPosition, data.moveTarget);
                        if (distanceToQueueSpot <= 0.5f)
                        {
                            data.currentState = ClientState.InQueue;
                            action = ClientAction.None;
                            data.waitQueueTime = 0f;
                        }
                        else
                        {
                            action = ClientAction.MoveToTarget;
                        }
                    }
                    else
                    {
                        action = ClientAction.MoveToTarget;
                    }
                    break;
                case ClientState.InQueue:
                    data.waitQueueTime += deltaTime;
                    if (data.queueSpotIndex == 0)
                    {
                        data.currentState = ClientState.AtCounter;
                    }
                    else if (data.waitQueueTime > data.maxWaitQueueTime)
                    {
                        data.currentState = ClientState.LeavingShop;
                        action = ClientAction.ApplyPenalty;
                    }
                    else if (!isShopOpen)
                    {
                        data.currentState = ClientState.LeavingShop;
                        action = ClientAction.MoveToTarget;
                    }
                    break;
                case ClientState.AtCounter:
                    if (!data.isShopOpen) { data.currentState = ClientState.LeavingShop; action = ClientAction.MoveToTarget; }
                    action = ClientAction.ShowOrderBubble;
                    data.currentState = ClientState.WaitingForOrder;
                    data.waitQueueTime = 0;
                    data.waitOrderTime = 0;
                    break;
                case ClientState.WaitingForOrder:
                    data.waitOrderTime += deltaTime;
                    if (!data.isShopOpen) { data.currentState = ClientState.LeavingShop; action = ClientAction.MoveToTarget; }
                    break;
                case ClientState.GoingToSeat:
                    if (!data.isShopOpen) { data.currentState = ClientState.LeavingShop; action = ClientAction.MoveToTarget; }
                    float distanceToSeat = math.distance(data.currentPosition, data.moveTarget);
                    if (distanceToSeat <= .8f)
                    {
                        data.currentState = ClientState.Seated;
                        action = ClientAction.None;
                    }
                    else
                    {
                        action = ClientAction.MoveToTarget;
                    }
                    break;
                case ClientState.Seated:
                    if (!data.isShopOpen) { data.currentState = ClientState.LeavingShop; action = ClientAction.MoveToTarget; }
                    data.waitQueueTime += deltaTime;
                    if (data.waitQueueTime > 15f)
                    {
                        if (GetRandomValue(data.id + 2) < 0.2f)
                        {
                            data.currentState = ClientState.GoingToQueue;
                            data.waitQueueTime = 0;
                        }
                        else
                        {
                            data.currentState = ClientState.LeavingShop;
                            action = ClientAction.Deactivate;
                        }
                    }
                    break;
                case ClientState.LeavingShop:
                    if (!data.isShopOpen) { data.currentState = ClientState.LeavingShop; action = ClientAction.MoveToTarget; }
                    float distanceToStreetEnd = math.distance(data.currentPosition, data.moveTarget);
                    if (distanceToStreetEnd <= 2f)
                    {
                        action = ClientAction.Deactivate;
                    }
                    else
                    {
                        action = ClientAction.MoveToTarget;
                    }
                    break;
            }
            clientDataArray[index] = data;
            actionArray[index] = action;
        }
        private float GetRandomValue(int seed)
        {
            Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)(seed * 1000 + (int)(deltaTime * 1000)));
            return random.NextFloat(0f, 1f);
        }
    }
}