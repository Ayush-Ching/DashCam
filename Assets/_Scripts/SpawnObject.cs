using UnityEngine;

public class SpawnObject : MonoBehaviour {

    private float moveSpeed;

    private void Update() {
        transform.position += -transform.forward * moveSpeed * Time.deltaTime;
    }

    public void SetMoveSpeed(float speed) {
        moveSpeed = speed;
    }

}
