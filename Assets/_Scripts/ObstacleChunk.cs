using UnityEngine;

public class ObstacleChunk : EnvChunk {

    [SerializeField] private GameObject[] trucks;

    private int trucksLocalSpeed;

    private void Start() {
        trucksLocalSpeed = EnvSpawner.Instance.GetTrucksLocalSpeed();

        foreach(GameObject truck in trucks) {
            truck.transform.localPosition += new Vector3(0, 0, (transform.position.z + truck.transform.localPosition.z) * trucksLocalSpeed);
        }
    }

    protected override void FixedUpdate() {
        Vector3 distance = -transform.forward * EnvSpawner.Instance.GetMoveSpeed() * Time.fixedDeltaTime;

        transform.position += distance;

        if (transform.position.z < EnvSpawner.Instance.BackThreshold() - length) {
            Destroy(gameObject);
            return;
        }

        foreach(GameObject truck in trucks) {
            truck.transform.position += distance * trucksLocalSpeed;
        }
    }

}
