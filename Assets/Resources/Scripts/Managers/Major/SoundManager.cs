using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

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
        [SerializeField] private AudioMixer audioMixer;

        public Slider sliderMaster;
        public Slider sliderSFX;

        private static SoundManager instance;
        private AudioSource audioSource;

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

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();

            float masterDB = PlayerPrefs.GetFloat("MasterVolume", 0f);
            float sfxDB = PlayerPrefs.GetFloat("SFXVolume", 0f);

            sliderMaster.value = Mathf.Pow(10f, masterDB / 20f);
            sliderSFX.value = Mathf.Pow(10f, sfxDB / 20f);

            SetMasterVolume(sliderMaster.value);
            SetSFXVolume(sliderSFX.value);

            sliderMaster.onValueChanged.AddListener(SetMasterVolume);
            sliderSFX.onValueChanged.AddListener(SetSFXVolume);
        }

        public static void PlaySound(SoundType sound, float volume = 1)
        {
            if (instance != null && instance.audioSource != null)
            {
                instance.audioSource.PlayOneShot(instance.soundList[(int)sound], volume);
            }
        }

        public void SetMasterVolume(float value)
        {
            float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
            audioMixer.SetFloat("MasterVolume", dB);
            PlayerPrefs.SetFloat("MasterVolume", dB);
        }

        public void SetSFXVolume(float value)
        {
            float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
            audioMixer.SetFloat("SFXVolume", dB);
            PlayerPrefs.SetFloat("SFXVolume", dB);
        }

        public void feedbackSound()
        {
            PlaySound(SoundType.feedback, 0.5f);
        }
    }
}
