//define os niveis de performance possiveis no minigame
public enum MinigamePerformance
{
    Fail,
    Bronze,
    Silver,
    Gold
}

//estrutura para carregar o resultado do minigame
public struct MinigameResult
{
    public MinigamePerformance performance;
}