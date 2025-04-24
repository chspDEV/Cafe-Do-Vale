using UnityEngine;
using Sirenix.OdinInspector;

public class PostProcessManager : Singleton<PostProcessManager>
{
    [Title("Configurações de Pós-Processamento")]
    [Required, AssetsOnly]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    public PostProcessVolume volume;

    [ShowInInspector, ReadOnly]
    private ColorGrading colorGrading;


    void Start()
    {
        volume.profile.TryGetSettings(out colorGrading);
    }

    public void SetExposure(float value)
    {
        if(colorGrading != null)
            colorGrading.postExposure.value = value;
    }

    public void SetTemperature(float value)
    {
        if(colorGrading != null)
            colorGrading.temperature.value = value;
    }

    [Button("Buscar Volume Automático")]
    private void FindVolume()
    {
        volume = FindObjectOfType<PostProcessVolume>();
    }

    [ButtonGroup("Ajustes Rápidos")]
    [Button("Resetar Exposição")]
    public void ResetExposure()
    {
        if(colorGrading != null) colorGrading.postExposure.value = 0f;
    }

}