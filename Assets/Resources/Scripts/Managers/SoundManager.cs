using UnityEngine;
using UnityEngine.UI;

namespace Tcp4
{
    public enum SoundType
    {
        coletar,
        interacao,
        passos,
        plantando,
        servindo,
        colocando,
        feedback,
        concluido,
        moendo,
        leite

    }

    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioClip[] soundList;
        private static SoundManager instance;
        private AudioSource audioSource;

        public AudioSource musicSource;
        public Toggle toggleMusic;
        public Toggle toggleSFX;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static void PlaySound(SoundType sound, float volume = 1)
        {
            if (instance != null && instance.audioSource != null)
            {
                instance.audioSource.PlayOneShot(instance.soundList[(int)sound], volume * instance.GetSFXVolume());
            }
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();

            bool isMusicOn = PlayerPrefs.GetInt("Music", 1) == 1;
            bool isSFXOn = PlayerPrefs.GetInt("SFX", 1) == 1;

            toggleMusic.isOn = !isMusicOn;
            toggleSFX.isOn = !isSFXOn;

            ToggleMusic(toggleMusic.isOn);
            ToggleSFX(toggleSFX.isOn);

            toggleMusic.onValueChanged.AddListener(ToggleMusic);
            toggleSFX.onValueChanged.AddListener(ToggleSFX);
        }

        public void ToggleMusic(bool isOn)
        {
            SoundManager.PlaySound(SoundType.feedback);
            musicSource.mute = isOn;
            PlayerPrefs.SetInt("Music", isOn ? 0 : 1);
        }

        public void ToggleSFX(bool isOn)
        {
            SoundManager.PlaySound(SoundType.feedback);
            SetSFXVolume(isOn ? 0 : 1);
            PlayerPrefs.SetInt("SFX", isOn ? 0 : 1);
        }

        private void SetSFXVolume(float volume)
        {
            audioSource.volume = volume;
        }

        private float GetSFXVolume()
        {
            return audioSource.volume;
        }

        public void feedbackSound()
        {
            SoundManager.PlaySound(SoundType.feedback, 0.5f);
        }
    }
}
