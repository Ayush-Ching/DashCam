using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject loseMenu;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI gameOverText;

    private bool isPaused;
    private bool isGameOver;

    private float score;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Resume();
        loseMenu.SetActive(false);
        score = 0;
        isGameOver = false;
    }

    private void Update() {
        if (GameInput.Instance.WasPausePressed()) {
            if (isPaused) Resume();
            else if(!isGameOver) Pause();
        }

        if (!isPaused) {
            score += EnvSpawner.Instance.GetMoveSpeed() * Time.deltaTime;
            scoreText.text = Mathf.FloorToInt(score).ToString() + " m";
        }
    }

    public void GameOver() {
        isGameOver = true;
        gameOverText.text = "FinalScore: " + Mathf.FloorToInt(score).ToString() + " m";
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

    public bool IsPaused() {
        return isPaused;
    }

    public bool IsGameOver() {
        return isGameOver;
    }

}
