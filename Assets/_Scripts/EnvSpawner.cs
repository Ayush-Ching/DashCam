using JetBrains.Annotations;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class EnvSpawner : MonoBehaviour {

    public static EnvSpawner Instance { get; private set; }

    [Header("Stats")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float maxSpeedMult = 2f;

    [Space]
    [SerializeField] private float frontThreshold = 500f;
    [SerializeField] private float backThreshold = -50f;

    [Space, Header("References")]
    [SerializeField] private EnvChunk roadChunkPrefab;

    private float speedMult = 1f;
    private EnvChunk prevRoadChunk;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        float pos = backThreshold - roadChunkPrefab.GetLength();
        while(pos <= frontThreshold) {
            EnvChunk spawnedRoadChunk = Instantiate(roadChunkPrefab, new Vector3(transform.position.x, transform.position.y, pos), Quaternion.identity);
            prevRoadChunk = spawnedRoadChunk;
            pos += roadChunkPrefab.GetLength();
        }
    }

    private void Update() {
        SpawnRoad();
    }

    private void SpawnRoad() {
        if(prevRoadChunk.transform.position.z <= frontThreshold - roadChunkPrefab.GetLength()) {
            EnvChunk spawnedRoadChunk = Instantiate(roadChunkPrefab, new Vector3(transform.position.x, transform.position.y, prevRoadChunk.transform.position.z + roadChunkPrefab.GetLength()), Quaternion.identity);
            prevRoadChunk = spawnedRoadChunk;
        }
    }

    public float MoveSpeed() {
        return moveSpeed * speedMult;
    }

    public float BackThreshold() {
        return backThreshold;
    }

}
