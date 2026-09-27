using UnityEngine;

public class EnvChunk : MonoBehaviour {

    [SerializeField] private float length;

    private void FixedUpdate() {
        transform.position += -transform.forward * EnvSpawner.Instance.MoveSpeed() * Time.fixedDeltaTime;

        if(transform.position.z < EnvSpawner.Instance.BackThreshold() - length) {
            Destroy(gameObject);
        }
    }

    public float GetLength() {
        return length;
    }

}
