using UnityEngine;

public class HeatmapSphere : MonoBehaviour
{
    //lembre-se de aplicar as alteracoes no codigo que fiz de acordo com o pedido
    [Tooltip("Numero maximo de esferas proximas para atingir a cor vermelha.")]
    [SerializeField] private int maxNearbySpheres = 10; //renomeei para clareza

    private Renderer sphereRenderer;
    private int nearbySphereCount = 0;
    private static readonly Color startColor = Color.green;
    private static readonly Color endColor = Color.red;

    void Start()
    {
        //pega o renderer e define a cor inicial
        sphereRenderer = GetComponent<Renderer>();
        UpdateColor();
    }

    //chamado uma vez quando outro trigger entra
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HeatmapSphere"))
        {
            nearbySphereCount++;
            UpdateColor();
        }
    }

    //chamado uma vez quando outro trigger sai
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HeatmapSphere"))
        {
            nearbySphereCount--;
            UpdateColor();
        }
    }

    //funcao central para atualizar a cor
    private void UpdateColor()
    {
        //trava o valor entre 0 e maxNearbySpheres
        float count = Mathf.Clamp(nearbySphereCount, 0, maxNearbySpheres);

        //interpola a cor com base no numero de colisoes
        float t = count / maxNearbySpheres;
        sphereRenderer.material.color = Color.Lerp(startColor, endColor, t);
    }
}