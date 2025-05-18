using UnityEngine;

namespace Tcp4
{
    using UnityEngine;

    public class FestaDoTomate : MonoBehaviour
    {
        private Rigidbody rb;
        private float timer = 0f;
        public float intervalo = 1f; // Intervalo entre as forças (em segundos)
        public float forca = 10f; // Intensidade da força aplicada

        // Start is called before the first frame update
        void Start()
        {
            // Pega o componente Rigidbody do próprio objeto
            rb = GetComponent<Rigidbody>();

            // Verifica se existe um Rigidbody, senão adiciona um
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Incrementa o timer com o tempo passado desde o último frame
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
            // Gera uma direção aleatória normalizada (vetor com magnitude 1)
            Vector3 direcaoAleatoria = Random.insideUnitSphere.normalized;

            // Aplica a força no Rigidbody
            rb.AddForce(direcaoAleatoria * forca, ForceMode.Impulse);

            // Opcional: Debug para visualizar a direção no Editor
            Debug.DrawRay(transform.position, direcaoAleatoria * 2f, Color.red, 1f);
        }
    }
}
