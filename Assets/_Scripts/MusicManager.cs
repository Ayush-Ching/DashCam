using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour {

    [SerializeField] private AudioClip mainMenuTrack;
    [SerializeField] private AudioClip gameTrack;
    enum SceneName {
        MainMenu,
        GameScene
    }

    private AudioSource audioSource;
    private SceneName currentScene;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        transform.position = Camera.main.transform.position;
        DontDestroyOnLoad(gameObject);

        currentScene = SceneName.MainMenu;
        audioSource.clip = mainMenuTrack;
        audioSource.Play();
    }

    private void Update() {
        if(currentScene != GetCurrentSceneName()) {
            if(GetCurrentSceneName() == SceneName.MainMenu) {
                currentScene = SceneName.MainMenu;
                audioSource.clip = mainMenuTrack;
                audioSource.volume = 1f;
                audioSource.Play();
            }
            else {
                currentScene = SceneName.GameScene;
                audioSource.clip = gameTrack;
                audioSource.Play();
            }
        }

        if(currentScene == SceneName.GameScene) {
            if(GameManager.Instance != null) {
                if (GameManager.Instance.IsPaused() || GameManager.Instance.IsGameOver()) audioSource.volume = 0.5f;
                else audioSource.volume = 1f;
            }
        }
    }

    private SceneName GetCurrentSceneName() {
        if(SceneManager.GetActiveScene().name == "MainMenu") return SceneName.MainMenu;
        else return SceneName.GameScene;
    }

}
