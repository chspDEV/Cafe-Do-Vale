using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tcp4;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine.UI;
using System;

namespace Tcp4.Assets.Resources.Scripts.Systems.Areas
{
    public class RefinamentActivator : BaseInteractable
    {
        public event Action OnActive;
        public bool isActive;
        public bool canInteract = false;

        private Vector3 initialPos; //DEBUG
        public override void Start()
        {
            base.Start();
            initialPos = transform.position;
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
                Activate();
            }
            
        }

        public void Activate()
        {
            isActive = true;
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.35f, transform.position.z);
            DisableInteraction();
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