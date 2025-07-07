using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tcp4.Assets.Resources.Scripts.Managers
{
    public class ProductionManager : Singleton<ProductionManager>
    {
        private readonly List<ProductionCard> choices = new();

        [SerializeField] private List<Production> productions;
        private ProductionArea reference;

        private Production productionToSet;
        public event Action OnChooseProduction;

        public void SetupNewProduction(Production newProduction) => productionToSet = newProduction;

        public void SetupNewReference(ProductionArea reference) => this.reference = reference;

        public ProductionArea GetCurrentReference() => reference;

        public Production GetNewProduction() => productionToSet;

        public bool HasProduction(Production p)
        { 
            return productions.Contains(p);
        }

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

        public void ReloadCards(List<Production> canProduce)
        {
            choices.Clear();

            var goList = UIManager.Instance.GetCardInstances();

            foreach (var go in goList)
            {
                var p = go.GetComponent<ProductionCard>();


                if (p != null && canProduce.Contains(p.myProduction))
                {
                    p.SetColletArea(reference);
                    choices.Add(p);
                    p.gameObject.SetActive(true);
                }
                else
                { 
                    p.gameObject.SetActive(false);
                }
            }
        }
    }
}
