using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameResources.Project.Scripts.Utilities.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class PooledAudioSource : MonoBehaviour
    {
        public AudioSource _audioSource;
        private Queue<GameObject> _pool;
        private Transform _poolParent;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        public void SetPool(Queue<GameObject> pool, Transform parent)
        {
            _pool = pool;
            _poolParent = parent;
        }

        public void Play(AudioClip clip, float volumeScale, float pitch = 1f, bool loop = false)
        {
            gameObject.SetActive(true);
            _audioSource.pitch = pitch;
            _audioSource.loop = loop;

            if (loop)
            {
                _audioSource.clip = clip;
                _audioSource.volume = volumeScale;
                _audioSource.Play();
            }
            else
            {
                _audioSource.PlayOneShot(clip, volumeScale);
                StartCoroutine(ReturnToPool(clip.length / Mathf.Abs(pitch)));
            }
        }

        public void Stop()
        {
            _audioSource.Stop();
            _audioSource.clip = null;
            _audioSource.loop = false;
            StopAllCoroutines();
            gameObject.SetActive(false);
            transform.SetParent(_poolParent);
            transform.localPosition = Vector3.zero;
            _pool.Enqueue(gameObject);
        }

        private IEnumerator ReturnToPool(float delay)
        {
            yield return new WaitForSeconds(delay);
            Stop();
        }
    }
}
