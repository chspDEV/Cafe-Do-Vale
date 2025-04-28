using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessManager : Singleton<PostProcessManager>
{
/*
    [Title("Configurações de Pós-Processamento")]
    [Required, AssetsOnly]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    public Volume volume; // Usando Volume do URP

    [ShowInInspector, ReadOnly]
    private ColorAdjustments colorAdjustments;

    void Start()
    {
        // Método CORRETO para obter o componente no URP
        if (volume.profile.TryGet(out colorAdjustments))
        {
            Debug.Log("ColorAdjustments encontrado!");
        }
        else
        {
            Debug.LogError("Adicione ColorAdjustments ao Volume!");
        }
    }

    public void SetExposure(float value)
    {
        if (colorAdjustments != null)
            colorAdjustments.postExposure.value = value;
    }

    public void SetTemperature(float value)
    {
        if (colorAdjustments != null)
            colorAdjustments.temperature.value = value;
    }

    [Button("Buscar Volume Automático")]
    private void FindVolume()
    {
        // Corrigido para buscar o Volume do URP
        volume = FindObjectOfType<Volume>();
    }

    [ButtonGroup("Ajustes Rápidos")]
    [Button("Resetar Exposição")]
    public void ResetExposure()
    {
        if (colorAdjustments != null) 
            colorAdjustments.postExposure.value = 0f;
    }
    */
}