using UnityEngine;

namespace Tcp4
{
    using UnityEngine;

    public class FestaDoTomate : MonoBehaviour
    {
        private Rigidbody rb;
        private float timer = 0f;
        public float intervalo = 1f; // Intervalo entre as for�as (em segundos)
        public float forca = 10f; // Intensidade da for�a aplicada

        // Start is called before the first frame update
        void Start()
        {
            // Pega o componente Rigidbody do pr�prio objeto
            rb = GetComponent<Rigidbody>();

            // Verifica se existe um Rigidbody, sen�o adiciona um
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Incrementa o timer com o tempo passado desde o �ltimo frame
            timer += Time.deltaTime;

            // Verifica se passou 1 segundo
            if (timer >= intervalo)
            {
                AplicarForcaAleatoria();
                timer = 0f; // Reseta o timer
            }
        }

        void AplicarForcaAleatoria()
        {
            // Gera uma dire��o aleat�ria normalizada (vetor com magnitude 1)
            Vector3 direcaoAleatoria = Random.insideUnitSphere.normalized;

            // Aplica a for�a no Rigidbody
            rb.AddForce(direcaoAleatoria * forca, ForceMode.Impulse);

            // Opcional: Debug para visualizar a dire��o no Editor
            Debug.DrawRay(transform.position, direcaoAleatoria * 2f, Color.red, 1f);
        }
    }
}
