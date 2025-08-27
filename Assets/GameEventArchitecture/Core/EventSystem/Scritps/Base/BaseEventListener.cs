using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A classe base abstrata para todos os componentes ouvintes de eventos.
/// Este MonoBehaviour serve como uma ponte entre um GameEvent (asset) e as respostas na cena (UnityEvents).
/// 'T': O tipo do dado que o evento carrega.
/// 'E': O tipo do GameEvent concreto que será ouvido.
/// 'UER': O tipo do UnityEvent Response concreto que será invocado.
/// </summary>
/// 
namespace GameEventArchitecture.Core.EventSystem.Base
{
    public abstract class BaseEventListener<T, E, UER> : MonoBehaviour, IEventListener<T>
    where E : BaseGameEvent<T>
    where UER : UnityEvent<T>
    {
        [SerializeField] private E gameEvent;
        [SerializeField] private UER unityEventResponse;

        public E GameEvent { get => gameEvent; set => gameEvent = value; }

        private void OnEnable()
        {
            if (gameEvent == null) return;
            gameEvent.Subscribe(this);
        }

        private void OnDisable()
        {
            if (gameEvent == null) return;
            gameEvent.Unsubscribe(this);
        }

        public void OnEventBroadcasted(T item)
        {
            unityEventResponse?.Invoke(item);
        }
    }
}


