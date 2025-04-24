using UnityEngine;

namespace Tcp4
{
    public static class StatusCalculator
    {
        public static int CalculateDamage(float baseDamage, float targetDefense)
        {
            // Calcula o dano reduzido pela defesa do alvo
            float damageAfterDefense = baseDamage * (1 - targetDefense / 100f);
            // Retorna o dano como um valor inteiro
            return Mathf.Max(Mathf.RoundToInt(damageAfterDefense), 0);
        }

        public static bool IsCritical(float criticalChance)
        {
            return Random.value <= criticalChance / 100f;
        }

        public static bool ShouldApplyEffect(float luck)
        {
            int calc = Random.Range(1, 101);
            //  Debug.Log(calc);
            return calc <= luck;
        }

        public static int CalculateFinalDamage(int baseDamage, bool isCritical)
        {
            return isCritical ? baseDamage * 2 : baseDamage;
        }
    }

}