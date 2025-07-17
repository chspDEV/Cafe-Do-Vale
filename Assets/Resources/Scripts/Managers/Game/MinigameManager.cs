using Tcp4;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;
using UnityEngine.Events;

public class MinigameManager : Singleton<MinigameManager>
{
    //eventos publicos para outros sistemas
    public UnityEvent<MinigameData> OnMinigameStarted;
    public UnityEvent OnMinigameFinished;

    private BaseMinigame _currentMinigameInstance;
    private MinigameData _currentMinigameData;

    Inventory playerInventory;


    public void Start()
    {
        //playerInventory = GameAssets.Instance.player.GetComponent<Inventory>();
    }
    public void StartMinigame(MinigameData data)
    {
        if (_currentMinigameInstance != null)
        {
            Debug.LogWarning("nao pode iniciar outro minigame enquanto tem um iniciado");
            return;
        }

        _currentMinigameData = data;

        //cria a instancia do minigame a partir do prefab
        GameObject minigameObject = Instantiate(data.minigamePrefab, UIManager.Instance.hudCanvas.transform);
        _currentMinigameInstance = minigameObject.GetComponent<BaseMinigame>();

        if (_currentMinigameInstance == null)
        {
            Debug.LogError($"prefab para minigame '{data.minigameName}' nao tem um BaseMinigame component.");
            Destroy(minigameObject);
            return;
        }

        //se inscreve no evento de conclusao do minigame
        _currentMinigameInstance.OnMinigameConcluded += HandleMinigameConclusion;

        //dispara o evento global de inicio
        OnMinigameStarted?.Invoke(data);

        //inicia a logica especifica do minigame
        _currentMinigameInstance.StartMinigame();
    }

    private void HandleMinigameConclusion(MinigameResult result)
    {
        //cancela a inscricao do evento para evitar memory leaks
        _currentMinigameInstance.OnMinigameConcluded -= HandleMinigameConclusion;

        //calcula e entrega a recompensa
        int rewardAmount = _currentMinigameData.GetRewardAmount(result.performance);
        Debug.Log($"minigame finalizado! performance: {result.performance}. recompensando player com {rewardAmount} de {_currentMinigameData.rewardProduct.productName}.");

        //aqui voce adicionaria a logica para dar o item ao inventario do jogador
        playerInventory.AddProduct(_currentMinigameData.rewardProduct, rewardAmount);

        //limpa a instancia do minigame
        Destroy(_currentMinigameInstance.gameObject);
        _currentMinigameInstance = null;
        _currentMinigameData = null;

        //dispara o evento global de fim
        OnMinigameFinished?.Invoke();
    }
}