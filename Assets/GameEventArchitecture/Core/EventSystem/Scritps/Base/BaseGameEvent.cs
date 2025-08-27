using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A classe base abstrata para todos os eventos do jogo baseados em ScriptableObject.
/// Contem a logica central para inscrever, desinscrever e notificar os ouvintes.
/// E generica ('T') para poder transmitir qualquer tipo de dado.
/// Por ser abstrata, não pode ser criada como um asset. É preciso criar classes filhas concretas.
/// </summary>
/// 

namespace GameEventArchitecture.Core.EventSystem.Base
{
    public abstract class BaseGameEvent<T> : GameEvent
    {
        private readonly List<IEventListener<T>> listeners = new List<IEventListener<T>>();

        public void Broadcast(T item)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventBroadcasted(item);
            }
        }

        public void Subscribe(IEventListener<T> listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        public void Unsubscribe(IEventListener<T> listener)
        {
            if (listeners.Contains(listener))
            {
                listeners.Remove(listener);
            }
        }
    }
}