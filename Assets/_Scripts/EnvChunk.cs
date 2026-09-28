using UnityEngine;

public class EnvChunk : MonoBehaviour {

    [SerializeField, Min(0.001f)] protected float length = 1f;

    protected virtual void FixedUpdate() {
        transform.position += -transform.forward * EnvSpawner.Instance.GetMoveSpeed() * Time.fixedDeltaTime;

        if(transform.position.z < EnvSpawner.Instance.BackThreshold() - length) {
            Destroy(gameObject);
        }
    }


    public float GetLength() {
        return length;
    }

}
