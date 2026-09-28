using UnityEngine;

public class EnvSpawner : MonoBehaviour {

    public static EnvSpawner Instance { get; private set; }

    [Header("Stats")]
    [SerializeField] private float minMoveSpeed = 30f;
    [SerializeField] private float maxMoveSpeed = 100f;
    [SerializeField] private float moveSpeedIncrementRate = 0.2f;
    [SerializeField] private float roadWidth = 30f;

    [Space]
    [SerializeField] private float frontThreshold = 500f;
    [SerializeField] private float backThreshold = -50f;

    [Space, Header("References")]
    [SerializeField] private EnvChunk[] roadChunks;
    [SerializeField] private EnvChunk[] leftEnvChunks;
    [SerializeField] private EnvChunk[] rightEnvChunks;

    private float moveSpeed;
    private EnvChunk prevRoadChunk;
    private EnvChunk prevLeftChunk;
    private EnvChunk prevRightChunk;

    private float halfRoadWidth;

    private void Awake() {
        Instance = this;

        halfRoadWidth = roadWidth / 2.0f;
        moveSpeed = minMoveSpeed;
    }

    private void Start() {
        InitializeChunks();
    }

    private void Update() {
        moveSpeed = Mathf.Min(maxMoveSpeed, moveSpeed + moveSpeedIncrementRate * Time.deltaTime);
    }

    private void LateUpdate() {
        ManageRoadSpawns();
        ManageLeftEnvSpawns();
        ManageRightEnvSpawns();
    }

    private void InitializeChunks() {
        float pos = backThreshold;
        // spawn roads
        while (roadChunks.Length > 0 && pos <= frontThreshold) {
            EnvChunk spawnedRoadChunk = Instantiate(roadChunks[Random.Range(0, roadChunks.Length)], new Vector3(0, 0, pos), Quaternion.identity);
            prevRoadChunk = spawnedRoadChunk;
            pos += spawnedRoadChunk.GetLength();
        }

        pos = backThreshold;
        // spawn left env
        while(leftEnvChunks.Length > 0 && pos <= frontThreshold) {
            EnvChunk spawnedLeftChunk = Instantiate(leftEnvChunks[Random.Range(0, leftEnvChunks.Length)], new Vector3(-halfRoadWidth, 0, pos), Quaternion.identity);
            prevLeftChunk = spawnedLeftChunk;
            pos += spawnedLeftChunk.GetLength();
        }

        pos = backThreshold;
        // spawn right env
        while (rightEnvChunks.Length > 0 && pos <= frontThreshold) {
            EnvChunk spawnedRightChunk = Instantiate(rightEnvChunks[Random.Range(0, rightEnvChunks.Length)], new Vector3(halfRoadWidth, 0, pos), Quaternion.identity);
            prevRightChunk = spawnedRightChunk;
            pos += spawnedRightChunk.GetLength();
        }
    }

    private void ManageRoadSpawns() {
        if (roadChunks.Length <= 0) return;

        if(prevRoadChunk.transform.position.z <= frontThreshold - prevRoadChunk.GetLength()) {
            EnvChunk spawnedRoadChunk = Instantiate(roadChunks[Random.Range(0, roadChunks.Length)], new Vector3(0, 0, prevRoadChunk.transform.position.z + prevRoadChunk.GetLength()), Quaternion.identity);
            prevRoadChunk = spawnedRoadChunk;
        }
    }

    private void ManageLeftEnvSpawns() {
        if (leftEnvChunks.Length <= 0) return;

        if (prevLeftChunk.transform.position.z <= frontThreshold - prevLeftChunk.GetLength()) {
            EnvChunk spawnedLeftChunk = Instantiate(leftEnvChunks[Random.Range(0, leftEnvChunks.Length)], new Vector3(-halfRoadWidth, 0, prevLeftChunk.transform.position.z + prevLeftChunk.GetLength()), Quaternion.identity);
            prevLeftChunk = spawnedLeftChunk;
        }
    }

    private void ManageRightEnvSpawns() {
        if (rightEnvChunks.Length <= 0) return;

        if (prevRightChunk.transform.position.z <= frontThreshold - prevRightChunk.GetLength()) {
            EnvChunk spawnedRightChunk = Instantiate(rightEnvChunks[Random.Range(0, rightEnvChunks.Length)], new Vector3(halfRoadWidth, 0, prevRightChunk.transform.position.z + prevRightChunk.GetLength()), Quaternion.identity);
            prevRightChunk = spawnedRightChunk;
        }
    }

    public float BackThreshold() {
        return backThreshold;
    }

    public float GetMoveSpeed() {
        return moveSpeed;
    }

}
