using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject loseMenu;

    private bool isPaused;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Resume();
        loseMenu.SetActive(false);
    }

    private void Update() {
        if (GameInput.Instance.WasPausePressed()) {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void GameOver() {
        Time.timeScale = 0f;
        loseMenu.SetActive(true);
    }

    public void Pause() {
        isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume() {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

}
