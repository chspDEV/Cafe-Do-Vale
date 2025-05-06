using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessManager : Singleton<PostProcessManager>
{
    [Title("Configurações de Pós-Processamento")]
    [Required, AssetsOnly]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    public Volume volume;

    [ShowInInspector, ReadOnly]
    private ColorAdjustments colorAdjustments;

    void Start()
    {
        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out colorAdjustments);
        }
    }

    public void SetExposure(float value)
    {
        if (colorAdjustments != null)
            colorAdjustments.postExposure.value = value;
    }

    public void SetTemperature(Color temperatureColor)
    {
        if (colorAdjustments != null)
            colorAdjustments.colorFilter.value = temperatureColor;
    }

    [Button("Buscar Volume Automático")]
    [System.Obsolete]
    private void FindVolume()
    {
        volume = FindObjectOfType<Volume>();
    }

    [ButtonGroup("Ajustes Rápidos")]
    [Button("Resetar Exposição")]
    public void ResetExposure()
    {
        if (colorAdjustments != null)
            colorAdjustments.postExposure.value = 0f;
    }
}