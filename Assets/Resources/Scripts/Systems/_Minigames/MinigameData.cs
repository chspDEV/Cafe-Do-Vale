using System;
using System.Collections.Generic;
using UnityEngine;

//define uma faixa de recompensa baseada na performance
[System.Serializable]
public class RewardTier
{
    public MinigamePerformance performanceLevel;
    public int amount;
}

[CreateAssetMenu(fileName = "Minigame_", menuName = "Minigames/Minigame Data")]
public class MinigameData : ScriptableObject
{
    [Header("Configuration")]
    public string minigameName;
    public GameObject minigamePrefab; //o prefab com a logica do minigame

    [Header("Rewards")]
    public BaseProduct rewardProduct;
    public List<RewardTier> rewardTiers;

    public event Action OnGetReward;

    
    //funcao para fazr setup da recompensa
    public void SetupReward(BaseProduct rewardProduct)
    { 
        this.rewardProduct = rewardProduct;
    }

    //funcao auxiliar para pegar a recompensa com base na performance
    public int GetRewardAmount(MinigamePerformance performance)
    {
        OnGetReward?.Invoke();

        foreach (var tier in rewardTiers)
        {
            if (tier.performanceLevel == performance)
            {
                return tier.amount;
            }
        }
        return 0; //retorna 0 se nenhuma faixa for encontrada
    }
}