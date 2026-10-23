using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour {

    private void Start() {
        QualitySettings.vSyncCount = 1;
    }

    public void PlayCam() {
        SceneManager.LoadScene("CamScene");
    }

    public void PlayKey() {
        SceneManager.LoadScene("KeyScene");
    }

    public void QuitGame() {
        Application.Quit();
    }
}
