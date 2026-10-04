using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour {

    public void PlayGame() {
        SceneManager.LoadScene("MobileScene");
    }

    public void QuitGame() {
        Application.Quit();
    }
}
