using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;

namespace Tcp4
{
    public class Collectable : BaseInteractable
    {
        public int money;

        public override void OnInteract()
        {
            base.OnInteract();
            ShopManager.Instance.AddMoney(money);
            SoundManager.PlaySound(SoundType.coletar, .7f);
            Destroy(gameObject);
        }
    }
}
