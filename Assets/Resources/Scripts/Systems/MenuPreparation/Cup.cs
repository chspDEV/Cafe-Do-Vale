using Tcp4;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;

public class Cup : MonoBehaviour
{
    public Drink myDrink;
    public Transform point; // Primeiro ponto de referência
    public float speed = 2f; // Velocidade do movimento
    private bool moving = true; // Controle de direção

    void Update()
    {
        if (point == null)
        {
            Debug.LogError("Os pontos de referência não estão atribuídos!");
            return;
        }

        // Move o objeto em direção ao alvo
        transform.position = Vector3.MoveTowards(transform.position, point.position, speed * Time.deltaTime);

        // Se chegou no destino, inverte a direção
        if (Vector3.Distance(transform.position, point.position) < 0.01f)
        {
            moving = false;
            ClientManager.Instance.ServeClient(myDrink);
            Destroy(this.gameObject);
        }
    }
}
