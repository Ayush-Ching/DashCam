using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour {

    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private GameObject buttonsContainer;

    private void Start() {
        QualitySettings.vSyncCount = 1;
        instructionsPanel.SetActive(false);
        buttonsContainer.SetActive(true);
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

    public void OpenInstructions() {
        instructionsPanel.SetActive(true);
        buttonsContainer.SetActive(false);
    }

    public void CloseInstructions() {
        instructionsPanel.SetActive(false);
        buttonsContainer.SetActive(true);
    }
}
