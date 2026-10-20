using UnityEngine;

public class EnvChunk : MonoBehaviour {

    [SerializeField, Min(0.001f)] protected float length = 1f;

    protected virtual void Update() {
        transform.position += -transform.forward * EnvSpawner.Instance.GetMoveSpeed() * Time.deltaTime;

        if(transform.position.z < EnvSpawner.Instance.BackThreshold() - length) {
            Destroy(gameObject);
        }
    }


    public virtual float GetLength() {
        return length;
    }

}
