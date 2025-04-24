using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tcp4
{
    public class InitialMenu : MonoBehaviour
    {
        public GameObject settingsPanel;
        public GameObject creditsPanel;
        public GameObject loadingScreen;
        public Slider loadingBar;

        private void Start()
        {
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            loadingScreen.SetActive(false);
        }

        public void StartGame(int sceneId)
        {
            StartCoroutine(LoadSceneAsync(sceneId));
            SoundManager.PlaySound(SoundType.feedback);
        }

        IEnumerator LoadSceneAsync(int sceneId)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
            operation.allowSceneActivation = false;
            loadingScreen.SetActive(true);
            loadingBar.value = 0;
            float progress = 0;

            while (progress < 1f)
            {
                progress += Time.deltaTime * 0.5f;
                loadingBar.value = progress;

                yield return null;
            }

            operation.allowSceneActivation = true;
        }

        public void OpenSettings()
        {
            settingsPanel.SetActive(true);
            SoundManager.PlaySound(SoundType.feedback);
        }

        public void CloseSettings()
        {
            settingsPanel.SetActive(false);
            SoundManager.PlaySound(SoundType.feedback);
        }

        public void OpenCredits()
        {
            creditsPanel.SetActive(true);
            SoundManager.PlaySound(SoundType.feedback);
        }

        public void CloseCredits()
        {
            creditsPanel.SetActive(false);
            SoundManager.PlaySound(SoundType.feedback);
        }

        public void QuitGame()
        {
            Application.Quit();
            SoundManager.PlaySound(SoundType.feedback);

            // p ver no editor
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}