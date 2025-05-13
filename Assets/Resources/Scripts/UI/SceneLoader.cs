using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Nome da cena que você deseja carregar
    [SerializeField] private string sceneToLoad = "MainScene";

    // Método público para ser chamado por um botão
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
