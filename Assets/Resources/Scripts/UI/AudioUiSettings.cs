using UnityEngine;
using UnityEngine.UI;
using GameResources.Project.Scripts.Utilities.Audio;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Sliders de Volume")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Salvar PlayerPrefs")]
    public bool saveSettings = true;

    private void Start()
    {
        // Carrega valores salvos
        if (masterSlider != null)
        {
            float saved = PlayerPrefs.GetFloat("MasterVolume", 1f);
            masterSlider.value = saved;
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicSlider != null)
        {
            float saved = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.value = saved;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            float saved = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.value = saved;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void SetMasterVolume(float volume)
    {
        SoundManager.SetMasterVolume(volume);
        if (saveSettings) PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        SoundManager.SetMusicVolume(volume);
        if (saveSettings) PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        SoundManager.SetSFXVolume(volume);
        if (saveSettings) PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    private void OnDestroy()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.RemoveListener(SetMasterVolume);
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
    }
}
