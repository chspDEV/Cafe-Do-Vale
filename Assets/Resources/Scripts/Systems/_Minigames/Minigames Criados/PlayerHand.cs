using System;
using UnityEngine;

//este componente deve estar no mesmo gameobject que o collider2d da mao
[RequireComponent(typeof(BoxCollider))]
public class PlayerHand : MonoBehaviour
{
    //evento estatico que pode ser acessado de qualquer lugar do codigo
    //qualquer script interessado pode se inscrever para ser notificado
    public static event Action OnPlayerStung;

    public static void PlayerTakeDamage()
    { 
        OnPlayerStung?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        //se colidiu com um objeto com a tag "bee"
        if (other.CompareTag("Bee"))
        {
            //dispara o evento, notificando todos os inscritos (listeners)
            //o '?' e um 'null check' que so dispara o evento se houver alguem ouvindo
            OnPlayerStung?.Invoke();

            //destroi a abelha para que a mesma colisao nao seja registrada multiplas vezes
            Destroy(other.gameObject);
        }
    }
}