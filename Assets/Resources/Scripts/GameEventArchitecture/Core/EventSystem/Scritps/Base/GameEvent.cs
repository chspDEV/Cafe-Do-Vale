using UnityEngine;

namespace GameEventArchitecture.Core.EventSystem.Base
{
    /// <summary>
    /// Classe base abstrata e não-genérica para todos os GameEvents.
    /// Sua única finalidade é permitir a referência a qualquer tipo de GameEvent
    /// no Inspector do Unity através de polimorfismo.
    /// </summary>
    public abstract class GameEvent : ScriptableObject { }
}