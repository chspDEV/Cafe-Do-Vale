/// <summary>
/// Define um contrato universal para qualquer classe que queira ouvir um BaseGameEvent.
/// Garante que o ouvinte implementara um metodo para reagir a transmissao do evento.
/// O tipo generico 'T' especifica qual tipo de dado este ouvinte esta preparado para receber.
/// </summary>
/// 

namespace GameEventArchitecture.Core.EventSystem.Base
{
    public interface IEventListener<T>
    {
        void OnEventBroadcasted(T item);
    }
}