//sem espaco no inicio, sem acentuacao, tudo minusculo
using TMPro;
using UnityEngine;
using UnityEngine.AI;
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
        public Image timerAtCounter;
        public Image timerAtQueue;
        public Image orderTmpBackground;
        public Image backgroundTimers;


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
            ControlOrderBubble(false);
            ControlQueueBubble(false);
            timerAtCounter.fillAmount = 1f;
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

        public void ControlOrderBubble(bool isActive)
        {
            backgroundTimers.gameObject.SetActive(isActive);
            timerAtCounter.gameObject.SetActive(isActive);
            wantedProduct.gameObject.SetActive(isActive);
            orderTmp.gameObject.SetActive(isActive);
            orderTmpBackground.gameObject.SetActive(isActive);
        }

        public void ControlQueueBubble(bool isActive)
        {
            backgroundTimers.gameObject.SetActive(isActive);
            timerAtQueue.gameObject.SetActive(isActive);
        }

        public void UpdateOrderName(string newOrderName)
        {
            this.orderTmp.text = newOrderName;
        }

        public void UpdateAnimation()
        {
            var agent = GetComponent<NavMeshAgent>();

            if (agent == null)
            {
                agent = GetComponentInChildren<NavMeshAgent>();
            }

            bool isMoving = agent != null && agent.velocity.magnitude > 0.1f;

            PlayAnimation(isMoving ? runAnimationName : idleAnimationName);

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

        public void UpdateTimer(Image timerImage, float fillAmount, bool isCritical = false)
        {
            fillAmount = Mathf.Clamp01(fillAmount);
            timerImage.fillAmount = fillAmount;

            // Cores diferentes para estado crítico
            if (isCritical)
            {
                // Pisca entre vermelho e amarelo quando crítico
                float blink = Mathf.PingPong(Time.time * 5f, 1f);
                timerImage.color = Color.Lerp(Color.red, Color.yellow, blink);
            }
            else
            {
                // Transição suave verde -> amarelo -> vermelho
                timerImage.color = new Color(
                    Mathf.Clamp01(1.5f - fillAmount * 1.5f), // Vermelho aumenta
                    Mathf.Clamp01(fillAmount * 1.5f),          // Verde diminui
                    0f
                );
            }
        }

        public void UpdateTimerAtCount(float fillAmount)
        {
            bool critical = (fillAmount < 0.3f); // Considera crítico abaixo de 30%
            UpdateTimer(timerAtCounter, fillAmount, critical);
        }

        public void UpdateTimerAtQueue(float fillAmount)
        {
            bool critical = (fillAmount < 0.2f); // Considera crítico abaixo de 20%
            UpdateTimer(timerAtQueue, fillAmount, critical);
        }

        void Start()
        {
            transform.rotation = Quaternion.Euler(0, -90, 0);
        }
    }
}