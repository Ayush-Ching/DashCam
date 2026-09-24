using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {

    [Header("Stats")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float spawnDistance;

    [Space, Header("References")]
    [SerializeField] private SpawnObject roadPrefab;

    Queue<SpawnObject> spawnedRoads;

    int count = 0;

    private void Start() {
        spawnedRoads = new Queue<SpawnObject>();
    }

    private void Update() {
        SpawnRoad();

        Debug.Log(count);
    }

    private void SpawnRoad() {
        if(spawnedRoads.Count <= 0) {
            float dist = -spawnDistance;
            while(dist <= spawnDistance) {
                SpawnObject spawnObj = Instantiate(roadPrefab, transform.position + new Vector3(0, 0, dist), Quaternion.identity);
                spawnedRoads.Enqueue(spawnObj);
                spawnObj.SetMoveSpeed(moveSpeed);
                count++;
                
                dist += spawnObj.transform.localScale.z;
            }
        }
        else if(spawnedRoads.Peek().transform.position.z < -spawnDistance) {
            SpawnObject obj = spawnedRoads.Peek();
            spawnedRoads.Dequeue();
            Destroy(obj.gameObject);
            SpawnObject spawnObj = Instantiate(roadPrefab, transform.position + new Vector3(0, 0, spawnDistance), Quaternion.identity);
            spawnedRoads.Enqueue(spawnObj);
            spawnObj.SetMoveSpeed(moveSpeed);
        }
        
    }

}
