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
            HeatmapSphere sphere = other.GetComponent<HeatmapSphere>();

            if (sphere.nearbySphereCount > this.nearbySphereCount)
            {
                IncreaseOtherHeatMap(sphere);
                return;
            }

            IncreaseMyHeatMap(other);

        }
    }

    private void IncreaseMyHeatMap(Collider other)
    {
        nearbySphereCount++;
        this.UpdateColor();
        transform.position = InterpolatePositions(other.gameObject.transform.position, this.gameObject.transform.position);

        //Otimizacao das esferas
        Destroy(other.gameObject, 0.1f);
        this.gameObject.transform.localScale += new Vector3(0.005f, 0.005f, 0.005f);
    }

    private void IncreaseOtherHeatMap(HeatmapSphere other)
    {
        other.nearbySphereCount++;
        other.UpdateColor();
        this.gameObject.transform.localScale += new Vector3(0.05f, 0.005f, 0.005f);
        other.transform.position = InterpolatePositions(other.gameObject.transform.position, this.gameObject.transform.position);

        Destroy(this.gameObject, 0.1f); 
    }

    private Vector3 InterpolatePositions(Vector3 a, Vector3 b)
    {
        a.y = 1f;
        b.y = 1f;

        return (a + b) / 2f;
    }

    //funcao central para atualizar a cor
    private void UpdateColor()
    {
        if (sphereRenderer == null)
        {
            sphereRenderer = GetComponent<Renderer>();
        }

        //trava o valor entre 0 e maxNearbySpheres
        float count = Mathf.Clamp(nearbySphereCount, 0, maxNearbySpheres);

        //interpola a cor com base no numero de colisoes
        float t = count / maxNearbySpheres;
        sphereRenderer.material.color = Color.Lerp(startColor, endColor, t);
    }
}