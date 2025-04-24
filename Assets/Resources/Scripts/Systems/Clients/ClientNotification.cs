using Tcp4.Assets.Resources.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tcp4
{
    public class ClientNotification: MonoBehaviour
    {
        public Image clientImage;
        public TextMeshProUGUI amountStar;
        public Image orderImage;

        public void Setup(Sprite newClientImage, Sprite newOrderImage, float amount)
        {
            clientImage.sprite = newClientImage;
            orderImage.sprite = newOrderImage;

            //maxStars é 1000f só que as estrelas vao de 1 a 5 entao 1000/1000 = 1 * 5 = 5
            float amountTratada = amount / ShopManager.Instance.GetMaxStars() * 5;

            amountStar.text = amountTratada.ToString("F1");
        }
    }
}
