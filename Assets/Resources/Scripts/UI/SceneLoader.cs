using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Nome da cena que voc� deseja carregar
    [SerializeField] private string sceneToLoad = "MainScene";

    // M�todo p�blico para ser chamado por um bot�o
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
