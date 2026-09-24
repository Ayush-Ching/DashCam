using UnityEngine;

public class EnvChunk : MonoBehaviour {

    [SerializeField] private float length;

    private void Update() {
        transform.position += -transform.forward * EnvSpawner.Instance.MoveSpeed() * Time.deltaTime;

        if(transform.position.z < EnvSpawner.Instance.BackThreshold() - length) {
            Destroy(gameObject);
        }
    }

    public float GetLength() {
        return length;
    }

}
