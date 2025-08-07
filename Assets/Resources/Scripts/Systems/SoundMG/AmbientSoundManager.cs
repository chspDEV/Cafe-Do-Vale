// Sistema principal de som ambiente
using GameResources.Project.Scripts.Utilities.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSoundManager : Singleton<AmbientSoundManager>
{
    [Header("Configura��es dos Sons Ambiente")]
    [SerializeField] private List<AmbientSoundConfig> ambientConfigs = new List<AmbientSoundConfig>();

    [Header("Debug")]
    [SerializeField] private AmbientType currentAmbient = AmbientType.None;
    [SerializeField] private bool isTransitioning = false;

    // Estado interno
    private Dictionary<AmbientType, AmbientSoundConfig> configDict;
    private Coroutine transitionCoroutine;
    private List<string> currentPlayingIDs = new();

    public override void Awake()
    {
        base.Awake();
        InitializeConfigs();
    }

    private void Start()
    {
        // Subscrever aos eventos do SoundManager para saber quando m�sicas s�o tocadas
        SoundManager.OnMusicPlay += OnMusicPlayed;
    }

    private void OnDestroy()
    {
        SoundManager.OnMusicPlay -= OnMusicPlayed;
    }

    private void InitializeConfigs()
    {
        configDict = new Dictionary<AmbientType, AmbientSoundConfig>();

        foreach (var config in ambientConfigs)
        {
            if (!configDict.ContainsKey(config.ambientType))
            {
                configDict.Add(config.ambientType, config);
            }
            else
            {
                Debug.LogWarning($"Configura��o duplicada encontrada para {config.ambientType}");
            }
        }
    }

    /// <summary>
    /// Muda para um novo som ambiente
    /// </summary>
    /// <param name="newAmbient">Tipo do novo ambiente</param>
    /// <param name="forceChange">For�a a mudan�a mesmo se j� estiver tocando o mesmo som</param>
    public void ChangeAmbient(AmbientType newAmbient, bool forceChange = false)
    {
        // Se j� est� tocando o mesmo ambiente e n�o � for�ado, ignora
        if (currentAmbient == newAmbient && !forceChange)
            return;

        // Se j� est� fazendo transi��o, para a atual
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(TransitionToAmbient(newAmbient));
    }

    /// <summary>
    /// Para o som ambiente atual
    /// </summary>
    public void StopAmbient()
    {
        ChangeAmbient(AmbientType.None);
    }

    /// <summary>
    /// Corrotina que faz a transi��o suave entre sons ambiente
    /// </summary>
    private IEnumerator TransitionToAmbient(AmbientType newAmbient)
    {
        isTransitioning = true;

        // Fade out do som atual se existir
        if (currentAmbient != AmbientType.None && configDict.ContainsKey(currentAmbient))
        {
            var currentConfig = configDict[currentAmbient];
            yield return StartCoroutine(FadeOutCurrent(currentConfig.fadeOutDuration));
        }

        // Atualiza o ambiente atual
        currentAmbient = newAmbient;

        // Fade in do novo som se n�o for None
        if (newAmbient != AmbientType.None && configDict.ContainsKey(newAmbient))
        {
            var newConfig = configDict[newAmbient];
            yield return StartCoroutine(FadeInNew(newConfig));
        }

        isTransitioning = false;
        transitionCoroutine = null;
    }

    /// <summary>
    /// Fade out do som atual
    /// </summary>
    private IEnumerator FadeOutCurrent(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        SoundManager.StopMusic(); // (voc� pode criar StopAllMusic se quiser gerenciar m�ltiplos audios separados)
        currentPlayingIDs.Clear();
    }

    /// <summary>
    /// Fade in do novo som
    /// </summary>
    private IEnumerator FadeInNew(AmbientSoundConfig config)
    {
        currentPlayingIDs.Clear();

        foreach (var id in config.audioIDs)
        {
            currentPlayingIDs.Add(id);

            SoundEventArgs args = new SoundEventArgs
            {
                Category = SoundEventArgs.SoundCategory.Music,
                AudioID = id,
                VolumeScale = config.volume,
                Pitch = config.pitch,
                Loop = true
            };

            SoundEvent.RequestSound(args);
            yield return new WaitForSeconds(0.1f); // pequena espera para evitar conflito de �udio se necess�rio
        }

        yield return new WaitForSeconds(config.fadeInDuration);
    }


    /// <summary>
    /// Callback para quando uma m�sica � tocada pelo SoundManager
    /// </summary>
    private void OnMusicPlayed(string audioID)
    {
        Debug.Log($"Som ambiente tocado: {audioID}");
    }

    /// <summary>
    /// Retorna o ambiente atual
    /// </summary>
    public AmbientType GetCurrentAmbient()
    {
        return currentAmbient;
    }

    /// <summary>
    /// Verifica se est� em transi��o
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    /// <summary>
    /// Adiciona uma nova configura��o em runtime
    /// </summary>
    public void AddAmbientConfig(AmbientSoundConfig config)
    {
        if (!configDict.ContainsKey(config.ambientType))
        {
            configDict.Add(config.ambientType, config);
            ambientConfigs.Add(config);
        }
        else
        {
            Debug.LogWarning($"Configura��o para {config.ambientType} j� existe!");
        }
    }

    /// <summary>
    /// Remove uma configura��o
    /// </summary>
    public void RemoveAmbientConfig(AmbientType ambientType)
    {
        if (configDict.ContainsKey(ambientType))
        {
            configDict.Remove(ambientType);
            ambientConfigs.RemoveAll(config => config.ambientType == ambientType);
        }
    }

    // M�todos de conveni�ncia para ambientes espec�ficos
    public void PlayFazenda() => ChangeAmbient(AmbientType.Fazenda);
    public void PlayCafeteria() => ChangeAmbient(AmbientType.Cafeteria);
    public void PlayFloresta() => ChangeAmbient(AmbientType.Floresta);

#if UNITY_EDITOR
    [Header("Debug - Editor Only")]
    [SerializeField] private AmbientType testAmbient = AmbientType.Fazenda;

    [ContextMenu("Test Change Ambient")]
    private void TestChangeAmbient()
    {
        ChangeAmbient(testAmbient);
    }

    [ContextMenu("Stop Ambient")]
    private void TestStopAmbient()
    {
        StopAmbient();
    }
#endif
}
