#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace GameResources.Project.Scripts.Utilities.Audio
{
    public static class SoundManager
    {
        private static SoundManagerSO _config;
        private static Queue<GameObject> _sfxPool = new();
        private static AudioSource _musicSource;
        private static GameObject _audioHost;

        private static readonly Dictionary<string, AudioClip> _musicClips = new();
        private static readonly Dictionary<string, AudioClip> _sfxClips = new();

        public delegate void AudioEvent(string audioID);
        public static event AudioEvent OnMusicPlay;
        public static event AudioEvent OnSFXPlay;

        static SoundManager()
        {
            Initialize();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            LoadConfig();

            if (_config)
            {
                LoadAudioClips();
                if (_audioHost == null || !_audioHost)
                {
                    _sfxPool.Clear(); 
                    CreateAudioHost();
                }
                else
                {
                    _sfxPool.Clear();
                }

                CreateSFXPool();
                LoadVolumeSettings();
            }
        }

        private static void LoadConfig()
        {
            _config = Resources.Load<SoundManagerSO>("Database/SoundManagerSO");
            if (_config == null)
            {
                Debug.LogError("SoundManagerConfig not found in Resources/Database folder!");
                return;
            }
        }

        private static void LoadAudioClips()
        {
            LoadClips(_config.musicFolderPath, _musicClips, "ost_");
            LoadClips(_config.sfxFolderPath, _sfxClips, "sfx_");
        }

        private static void LoadClips(string folderPath, Dictionary<string, AudioClip> targetDict, string prefix)
        {
            AudioClip[] clips = Resources.LoadAll<AudioClip>(folderPath);
            foreach (var clip in clips)
            {
                if (clip != null && clip.name.StartsWith(prefix))
                {
                    var id = clip.name.Substring(prefix.Length).ToLower();
                    targetDict[id] = clip;
                    Debug.Log($"CLIP: {clip} carregado!");
                }
            }
            Debug.Log($"Áudios de: {prefix} carregados corretamente!");
        }

        private static void CreateAudioHost()
        {
            if (_audioHost != null && _audioHost)
                return;

            _audioHost = new GameObject("AudioHost");
            _musicSource = _audioHost.AddComponent<AudioSource>();
            _musicSource.outputAudioMixerGroup = _config.audioMixer.FindMatchingGroups("Music")[0];
            Object.DontDestroyOnLoad(_audioHost);
        }

        private static void CreateSFXPool()
        {
            if (_config.SFXPrefab == null)
            {
                Debug.LogError("SFXPrefab not set in SoundManagerSO!");
                return;
            }

            for (int i = 0; i < _config.sfxPoolSize; i++)
            {
                var instance = Object.Instantiate(_config.SFXPrefab, _audioHost.transform);
                instance.SetActive(false);
                var pooledSource = instance.GetComponent<PooledAudioSource>();
                if (pooledSource != null)
                {
                    pooledSource.SetPool(_sfxPool, _audioHost.transform);
                }
                else
                {
                    Debug.LogError("SFXPrefab missing PooledAudioSource component!");
                }
                _sfxPool.Enqueue(instance);
            }
        }

        private static void IncreaseSFXPool()
        {
            if (_config.SFXPrefab == null)
            {
                Debug.LogError("SFXPrefab not set in SoundManagerSO!");
                return;
            }

            var instance = Object.Instantiate(_config.SFXPrefab, _audioHost.transform);
            instance.SetActive(false);
            var pooledSource = instance.GetComponent<PooledAudioSource>();
            if (pooledSource != null)
            {
                pooledSource.SetPool(_sfxPool, _audioHost.transform);
            }
            else
            {
                Debug.LogError("SFXPrefab missing PooledAudioSource component!");
            }
            _sfxPool.Enqueue(instance);
        }

        private static GameObject GetAvailableSFXSource()
        {
            var source = _sfxPool.Dequeue();
            _sfxPool.Enqueue(source);
            return source;
        }

        private static void LoadVolumeSettings()
        {
            SetMasterVolume(PlayerPrefs.GetFloat(_config.masterVolumeParam, 1f));
            SetMusicVolume(PlayerPrefs.GetFloat(_config.musicVolumeParam, 1f));
            SetSFXVolume(PlayerPrefs.GetFloat(_config.sfxVolumeParam, 1f));
        }

        public static void SetMasterVolume(float volume)
        {
            _config.audioMixer.SetFloat(_config.masterVolumeParam, LinearToDecibel(volume));
            PlayerPrefs.SetFloat(_config.masterVolumeParam, volume);
        }

        public static void SetMusicVolume(float volume)
        {
            _config.audioMixer.SetFloat(_config.musicVolumeParam, LinearToDecibel(volume));
            PlayerPrefs.SetFloat(_config.musicVolumeParam, volume);
        }

        public static void SetSFXVolume(float volume)
        {
            _config.audioMixer.SetFloat(_config.sfxVolumeParam, LinearToDecibel(volume));
            PlayerPrefs.SetFloat(_config.sfxVolumeParam, volume);
        }

        private static float LinearToDecibel(float linear)
        {
            return linear <= 0f ? -80f : Mathf.Log10(linear) * 20f;
        }

        public static void PlayOST(string id)
        {
            if (_config == null) return;

            if (_musicClips.TryGetValue(id.ToLower(), out AudioClip clip))
            {
                _musicSource.clip = clip;
                _musicSource.Play();
                OnMusicPlay?.Invoke(id);
            }
        }

        public static void PlaySFX(string id, Vector3 position, float volumeScale = 1f, float pitch = 1f)
        {
            if (_config == null)
            {
                Debug.LogError("SoundManagerSO Não Configurado!");
                return;
            }

            if (_sfxClips.TryGetValue(id.ToLower(), out AudioClip clip))
            {
                if (_sfxPool.Count == 0)
                {
                    Debug.LogWarning("SFX pool esgotado! Aumentando em 1!");
                    IncreaseSFXPool();
                }

                var sourceObj = _sfxPool.Dequeue();
                if (sourceObj == null || !sourceObj)
                {
                    Debug.LogWarning("Objeto do pool inválido, recriando pool.");
                    IncreaseSFXPool();
                    sourceObj = _sfxPool.Dequeue();
                }

                sourceObj.transform.position = position;
                var pooledSource = sourceObj.GetComponent<PooledAudioSource>();
                pooledSource.Play(clip, volumeScale);
                pooledSource._audioSource.pitch = pitch;
                OnSFXPlay?.Invoke(id);
            }
            else
            {
                Debug.LogError($"Áudio Não Executado: {id}");
            }
        }

        public static void PlaySFX(string id, Transform targetTransform, float volumeScale = 1f, float pitch = 1f)
        {
            PlaySFX(id, targetTransform.position, volumeScale, pitch);
        }

        public static void StopMusic()
        {
            if (_config == null) return;
            _musicSource.Stop();
        }

        public static void PauseMusic()
        {
            if (_config == null) return;
            _musicSource.Pause();
        }

        public static void ResumeMusic()
        {
            if (_config == null) return;
            _musicSource.UnPause();
        }
    }
}