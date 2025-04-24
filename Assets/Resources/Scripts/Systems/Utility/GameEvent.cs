using System.Collections.Generic;
using UnityEngine;

namespace Tcp4.Resources.Scripts.Systems.Utility
{
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Game Events/Game Event", order = 0)]
    public class GameEvent : ScriptableObject
    {
        private readonly List<GameEventListener> eventListeners = new List<GameEventListener>();

        public void Raise()
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
            {
                eventListeners[i].OnEventRaised();
            }
        }

        public void RegisterListener(GameEventListener listener)
        {
            if (!eventListeners.Contains(listener))
            {
                eventListeners.Add(listener);
            }
        }

        public void UnregisterListener(GameEventListener listener)
        {
            if (eventListeners.Contains(listener))
            {
                eventListeners.Remove(listener);
            }
        }
    }

}