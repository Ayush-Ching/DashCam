using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject loseMenu;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Toggle vSyncToggle;

    private bool isPaused;
    private bool isGameOver;

    private float score;

    private const string VSyncKey = "VSyncCount";

    private void Awake() {
        Instance = this;

    }

    private void Start() {
        Resume();
        loseMenu.SetActive(false);
        score = 0;
        isGameOver = false;

        int savedVsync = PlayerPrefs.GetInt(VSyncKey, 1);
        QualitySettings.vSyncCount = savedVsync;
        vSyncToggle.isOn = savedVsync > 0;

        vSyncToggle.onValueChanged.AddListener(SetVSync);
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

    void OnDestroy() {
        vSyncToggle?.onValueChanged.RemoveListener(SetVSync);
    }

    public void GameOver() {
        isGameOver = true;
        gameOverText.text = "FinalScore: " + Mathf.FloorToInt(score).ToString() + " m";
        Time.timeScale = 0f;
        loseMenu.SetActive(true);
    }

    public void SetVSync(bool enabled) {
        int value = enabled ? 1 : 0;

        QualitySettings.vSyncCount = value;

        PlayerPrefs.SetInt(VSyncKey, value);
        PlayerPrefs.Save();
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
