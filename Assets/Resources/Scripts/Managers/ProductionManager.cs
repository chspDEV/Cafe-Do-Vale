using ComponentUtils.ComponentUtils.Scripts;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class ProductionManager : Singleton<ProductionManager>
    {
        private readonly List<ProductionCard> choices = new();

        [SerializeField] private List<Production> productions;
        private CollectArea reference;

        private Production productionToSet;
        public event Action OnChooseProduction;

        public void SetupNewProduction(Production newProduction) => productionToSet = newProduction;

        public void SetupNewReference(CollectArea reference) => this.reference = reference;

        public CollectArea GetCurrentReference() => reference;

        public Production GetNewProduction() => productionToSet;

        public void Clean()
        {
            productionToSet = null;
            reference = null;
        }

        public void InvokeChooseProduction() => OnChooseProduction?.Invoke();

        public Production GetProductionByID(int ID)
        {
            for(var i = 0; i < productions.Count; i++)
            {
                if(i == ID)
                {
                    return productions[i];
                }
            }

            return null;
        }

        public void ReloadCards()
        {
            choices.Clear();

            var goList = UIManager.Instance.GetCardInstances();

            foreach (var go in goList)
            {
                var p = go.GetComponent<ProductionCard>();
                if (p != null)
                {
                    p.SetColletArea(reference);
                    choices.Add(p);
                }
            }
        }
    }
}
