using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tcp4;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine.UI;
using System;
using GameResources.Project.Scripts.Utilities.Audio;

namespace Tcp4.Assets.Resources.Scripts.Systems.Areas
{
    public class RefinementActivator : BaseInteractable
    {
        public event Action OnActive;
        public bool isActive;
        public bool canInteract = false;
        public GameObject indicator;

        private Vector3 initialPos; //DEBUG
        public override void Start()
        {
            base.Start();
            initialPos = transform.position;
            interactable_id = "refinementButton";
            indicator.SetActive(false);
        }

        public override void Update()
        {
            base.Update();

            indicator.SetActive(canInteract);
        }

        public override void OnFocus()
        {
            if(canInteract)
                base.OnFocus();
        }

        public override void OnLostFocus()
        {
            if (canInteract)
                base.OnLostFocus();
        }

        public override void OnInteract()
        {
            base.OnInteract();

            if (canInteract)
            {
                InteractionManager.Instance.UpdateLastId(interactable_id);
                Activate();
            }
            
        }

        public void Activate()
        {
            isActive = true;
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.25f, transform.position.z);
            DisableInteraction();
            //Fazendo o request de sfx
            SoundEventArgs ostArgs = new()
            {
                Category = SoundEventArgs.SoundCategory.SFX,
                AudioID = "button_click", // O ID do seu SFX (sem "sfx_" e em minúsculas)
                VolumeScale = 1f // Escala de volume (opcional, padrão é 1f)
            };
            SoundEvent.RequestSound(ostArgs);
            OnActive?.Invoke();
        }

        public void Reset() 
        {
            isActive = false;
            canInteract = false;
            transform.position = initialPos;
        } 
        
        
    }
}