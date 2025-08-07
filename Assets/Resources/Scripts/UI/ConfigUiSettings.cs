using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class GraphicsSettingsUI : MonoBehaviour
{
    [Header("Qualidade Gráfica")]
    public Dropdown qualityDropdown;

    [Header("Pós-Processamento")]
    public Toggle postProcessingToggle;
    public Volume globalPostProcessingVolume; // Volume global do PostProcessing (URP/HDRP)

    [Header("Resolução e Tela Cheia")]
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private Resolution[] resolutions;

    void Start()
    {
        SetupQualityDropdown();
        SetupResolutionDropdown();

        if (postProcessingToggle != null && globalPostProcessingVolume != null)
        {
            postProcessingToggle.isOn = globalPostProcessingVolume.enabled;
            postProcessingToggle.onValueChanged.AddListener(SetPostProcessing);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
    }

    private void SetupQualityDropdown()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();
        var names = QualitySettings.names;
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(names));
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
    }

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetQualityLevel(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("GraphicsQuality", index);
    }

    public void SetPostProcessing(bool isOn)
    {
        if (globalPostProcessingVolume != null)
            globalPostProcessingVolume.enabled = isOn;

        PlayerPrefs.SetInt("PostProcessing", isOn ? 1 : 0);
    }

    public void SetResolution(int index)
    {
        if (index >= 0 && index < resolutions.Length)
        {
            Resolution res = resolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            PlayerPrefs.SetInt("ResolutionIndex", index);
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }
}
