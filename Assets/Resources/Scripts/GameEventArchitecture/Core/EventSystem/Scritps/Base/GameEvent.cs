using UnityEngine;

namespace GameEventArchitecture.Core.EventSystem.Base
{
    /// <summary>
    /// Classe base abstrata e n�o-gen�rica para todos os GameEvents.
    /// Sua �nica finalidade � permitir a refer�ncia a qualquer tipo de GameEvent
    /// no Inspector do Unity atrav�s de polimorfismo.
    /// </summary>
    public abstract class GameEvent : ScriptableObject { }
}