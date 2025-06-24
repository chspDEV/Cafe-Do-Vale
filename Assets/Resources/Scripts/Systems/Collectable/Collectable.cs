using GameResources.Project.Scripts.Utilities.Audio;
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

            /* Deixei apenas o som de dinheiro
            //Fazendo o request de sfx
            SoundEventArgs sfxArgs = new()
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = "coletar", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                VolumeScale = .4f // Escala de volume (opcional, padrão é 1f)
            };
            SoundEvent.RequestSound(sfxArgs);
            */

            Destroy(gameObject);
        }
    }
}
