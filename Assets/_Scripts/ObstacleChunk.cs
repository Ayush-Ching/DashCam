using UnityEngine;

public class ObstacleChunk : EnvChunk {

    [SerializeField] private Truck[] trucks;

    private void Start() {
        foreach(Truck truck in trucks) {
            truck.transform.localPosition += new Vector3(0, 0, (transform.position.z + truck.transform.localPosition.z) * truck.GetRelativeLocalSpeed());
        }
    }

    protected override void FixedUpdate() {
        Vector3 distance = -transform.forward * EnvSpawner.Instance.GetMoveSpeed() * Time.fixedDeltaTime;

        transform.position += distance;

        if (transform.position.z < EnvSpawner.Instance.BackThreshold() - length) {
            Destroy(gameObject);
            return;
        }

        foreach(Truck truck in trucks) {
            truck.transform.position += distance * truck.GetRelativeLocalSpeed();
        }
    }

}
