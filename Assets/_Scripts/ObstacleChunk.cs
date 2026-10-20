using UnityEngine;

public class ObstacleChunk : EnvChunk {

    [SerializeField] private Truck[] trucks;

    protected float lengthFactor = 1f;

    private void Start() {
        lengthFactor = EnvSpawner.Instance.GetObstacleLengthFactor();

        foreach(Transform child in GetComponentsInChildren<Transform>()) {
            if(child == transform) continue;

            child.transform.localPosition = new Vector3(child.transform.localPosition.x, child.transform.localPosition.y, child.transform.localPosition.z * lengthFactor);
        }

        foreach(Truck truck in trucks) {
            truck.transform.localPosition += new Vector3(0, 0, (transform.position.z + truck.transform.localPosition.z) * truck.GetRelativeLocalSpeed());
        }
    }

    protected override void Update() {
        Vector3 distance = -transform.forward * EnvSpawner.Instance.GetMoveSpeed() * Time.deltaTime;

        transform.position += distance;

        if (transform.position.z < EnvSpawner.Instance.BackThreshold() - length) {
            Destroy(gameObject);
            return;
        }

        foreach(Truck truck in trucks) {
            truck.transform.position += distance * truck.GetRelativeLocalSpeed();
        }
    }

    public override float GetLength() {
        return length * lengthFactor;
    }

}
