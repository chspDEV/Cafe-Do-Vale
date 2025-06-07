using Unity.Mathematics;

namespace Tcp4
{
    public struct ClientData
    {
        public int id;
        public ClientState currentState;
        public float3 currentPosition;
        public float3 moveTarget;

        //dados para as decisoes
        public int orderID;
        public float waitTime;
        public float speed;
        internal bool isActive;
    }
}
