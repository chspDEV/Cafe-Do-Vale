//sem espaco no inicio, sem acentuacao, tudo minusculo
using Tcp4.Assets.Resources.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tcp4.Assets.Resources.Scripts.Systems.Clients
{
    public class Client : MonoBehaviour
    {
        //informacoes de identificacao e referencias visuais
        public string clientID;
        public string clientName;

        [Header("UI References")]
        public TextMeshProUGUI nameTmp;
        public Image wantedProduct;
        public Image timer;

        private GameObject currentModel;

        //setup agora e focado apenas na parte visual inicial
        public void Setup(string newID, string newName, Sprite newSprite, GameObject newModel)
        {
            this.clientID = newID;
            this.clientName = newName;
            this.nameTmp.text = newName;

            //limpa o modelo antigo se houver
            if (currentModel != null)
            {
                Destroy(currentModel);
            }
            //instancia o novo modelo
            currentModel = Instantiate(newModel, transform);

            //configuracao inicial da ui
            wantedProduct.gameObject.SetActive(false);
            timer.fillAmount = 1f;
        }

        //funcao publica para o clientmanager chamar quando o pedido for decidido
        public void ShowWantedProduct(Sprite productSprite)
        {
            if (productSprite != null)
            {
                wantedProduct.sprite = productSprite;
                wantedProduct.gameObject.SetActive(true);
            }
        }

        //funcao publica para o clientmanager chamar para atualizar a barra de tempo
        public void UpdateTimerUI(float fillAmount)
        {
            timer.fillAmount = Mathf.Clamp01(fillAmount);
        }

        void Start()
        {
            transform.rotation = Quaternion.Euler(0, -90, 0);
        }
    }
}