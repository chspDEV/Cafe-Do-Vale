using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.Utility
{

    public class EventTrigger : MonoBehaviour
    {
        [SerializeField]
        private GameEvent gameEvent;

        public void TriggerEvent()
        {
            gameEvent.Raise();
        }
    }

}