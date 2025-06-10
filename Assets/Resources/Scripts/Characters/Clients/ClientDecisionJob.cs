using Unity.Jobs;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.Xml.Linq;
using System.Net.NetworkInformation;

namespace Tcp4
{
    public struct ClientDecisionJob : IJobParallelFor
    {
        public NativeArray<ClientData> clientDataArray;
        [WriteOnly] public NativeArray<ClientAction> actionArray;

        [ReadOnly] public float deltaTime;
        [ReadOnly] public float playerReputation; //ex: 0.0 a 1.0
        [ReadOnly] public float3 shopEntrancePosition;
        [ReadOnly] public float3 streetEndPosition;
        [ReadOnly] public float3 counterPosition;



        public void Execute(int index)
        {
            ClientData data = clientDataArray[index];
            ClientAction action = ClientAction.None;

            //maquina de estados baseada no estado atual do npc
            switch (data.currentState)
            {
                //passo 1: andando pela rua e decidindo entrar
                case ClientState.WalkingOnStreet:

                    float distanceFromDoor = math.distance(data.currentPosition, shopEntrancePosition);
                    if (distanceFromDoor <= 5f) //se estiver perto da porta
                    {

                        //a chance de entrar aumenta com a reputacao
                        float chanceToEnter = 0.1f + playerReputation * 0.8f;
                        if (GetRandomValue(data.id) < chanceToEnter)
                        {
                            data.currentState = ClientState.GoingToQueue;
                            action = ClientAction.MoveToTarget;
                        }
                    }
                    else
                    {
                        data.moveTarget = streetEndPosition;
                        action = ClientAction.MoveToTarget;
                    }
                    break;

                //passo 3: na fila, esperando
                case ClientState.GoingToQueue:
                    float distanceToQueueSpot = math.distance(data.currentPosition, data.moveTarget);

                    if (distanceToQueueSpot <= 0.5f)
                    {
                        data.currentState = ClientState.InQueue;
                        action = ClientAction.None; 
                        data.waitTime = 0f; 
                    }
                    else
                    {
                        action = ClientAction.MoveToTarget;
                    }
                    break;

                case ClientState.InQueue:
                    
                    break;

                //passo 2 & 4: no balcao, decidindo e fazendo o pedido
                case ClientState.AtCounter:
                    //passo 2: decidir o pedido (logica simplificada)
                    data.orderID = (int)(GetRandomValue(data.id + 1) * 5) + 1; //escolhe um pedido de 1 a 5

                    //passo 4: falar o pedido
                    action = ClientAction.ShowOrderBubble;
                    data.currentState = ClientState.WaitingForOrder;
                    data.waitTime = 0; //reseta o timer
                    break;

                case ClientState.WaitingForOrder:
                    //aqui, o job apenas espera. a interacao do jogador (atender o pedido)
                    //sera tratada pelo manager, que ira mudar o estado do npc.
                    break;

                case ClientState.GoingToSeat:
                    float distanceToSeat = math.distance(data.currentPosition, data.moveTarget);

                    //se o cliente ja chegou perto o suficiente do seu alvo...
                    if (distanceToSeat <= 2f) //1.0f e uma boa margem de erro
                    {
                        //...mudamos seu estado para o proximo passo
                        data.currentState = ClientState.Seated;
                        action = ClientAction.None;
                    }
                    else
                    {
                        action = ClientAction.MoveToTarget;
                    }

                    break;

                //passo 6: sentado, decidindo se faz um novo pedido
                case ClientState.Seated:
                    data.waitTime += deltaTime;
                    if (data.waitTime > 15f) //espera 15s enquanto esta sentado
                    {
                        if (GetRandomValue(data.id + 2) < 0.2f) //20% de chance de um novo pedido
                        {
                            data.currentState = ClientState.GoingToQueue;
                            data.waitTime = 0;
                        }
                        else
                        {
                            data.currentState = ClientState.LeavingShop;
                            action = ClientAction.Deactivate;
                        }
                    }
                    break;

                case ClientState.LeavingShop:

                    float distanceToStreetEnd = math.distance(data.currentPosition, data.moveTarget);

                    if (distanceToStreetEnd <= 10f)
                    {
                        action = ClientAction.Deactivate;
                    }
                    else
                    {
                        action = ClientAction.MoveToTarget;
                    }
                    break;
            }

            //atualiza os arrays de dados e acoes
            clientDataArray[index] = data;
            actionArray[index] = action;
        }

        private float GetRandomValue(int seed)
        {
            //gera um valor pseudo-aleatorio baseado no id para consistencia
            return noise.snoise(new float2(seed, deltaTime));
        }
    }
}
