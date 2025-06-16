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

        [Header("UI References")]
        public TextMeshProUGUI orderTmp;
        public Image wantedProduct;
        public Image timer;


        private GameObject currentModel;
        public string debugAction = "naoConfigurado";
        public string debugState = "naoConfigurado";

        public string idleAnimationName = "";
        public string runAnimationName = "";

        private Animator anim;

        //setup agora e focado apenas na parte visual inicial
        public void Setup(string newID, Sprite newSprite, GameObject newModel)
        {
            this.clientID = newID;

            //limpa o modelo antigo se houver
            if (currentModel != null)
            {
                Destroy(currentModel);
            }
            //instancia o novo modelo
            currentModel = Instantiate(newModel, transform);
            anim = currentModel.GetComponent<Animator>();

            //configuracao inicial da ui
            ControlBubble(false);
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

        public void ControlBubble(bool isActive)
        {
            timer.gameObject.SetActive(isActive);
            wantedProduct.gameObject.SetActive(isActive);
            orderTmp.gameObject.SetActive(isActive);
        }

        public void UpdateOrderName(string newOrderName)
        {
            this.orderTmp.text = newOrderName;
        }

        public void UpdateAnimation()
        {
            /* STATES
             WalkingOnStreet,
            GoingToQueue,
            InQueue,
            GoingToCounter,
            AtCounter,
            WaitingForOrder,
            GoingToSeat,
            Seated,
            LeavingShop
             */
            switch (debugState)
            {
                case "InQueue":
                    PlayAnimation(idleAnimationName);
                    //Debug.Log("ANIMACAO: idleAnimationName");
                    break;

                case "Seated":
                    PlayAnimation(idleAnimationName);
                    //Debug.Log("ANIMACAO: idleAnimationName");
                    break;

                case "WaitingForOrder":
                    PlayAnimation(idleAnimationName);
                    //Debug.Log("ANIMACAO: idleAnimationName");
                    break;
                case "AtCounter":
                    PlayAnimation(idleAnimationName);
                    //Debug.Log("ANIMACAO: idleAnimationName");
                    break;

                default:
                    PlayAnimation(runAnimationName);
                    //Debug.Log("ANIMACAO: runAnimationName");
                    break;
            }
        }

        private void PlayAnimation(string animToPlay)
        {
            if (anim != null)
            {
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                {
                    anim.Play(animToPlay);
                }  
            }
            else
            {
                Debug.LogError($"Animator nao encontrado no cliente {clientID}!");
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