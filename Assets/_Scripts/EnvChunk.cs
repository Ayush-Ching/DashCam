using UnityEngine;

public class EnvChunk : MonoBehaviour {

    [SerializeField] protected float length;

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
