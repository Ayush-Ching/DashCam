using UnityEngine;

public class Player : MonoBehaviour {

    [Header("Stats")]
    [SerializeField] private float laneSwitchSpeed = 15f;
    [SerializeField] private float laneWidth = 10f;

    private int currentLane = 0;

    private void Start() {
        currentLane = 0;
    }

    private void Update() {
        if (GameInput.Instance.WasLeftPressed()) currentLane = Mathf.Max(-1, currentLane - 1);
        if (GameInput.Instance.WasRightPressed()) currentLane = Mathf.Min(1, currentLane + 1);
    }

    private void FixedUpdate() {
        LerpToLane();
    }

    private void LerpToLane() {
        Vector3 targetPos = new Vector3(currentLane * laneWidth, transform.position.y, transform.position.z);

        if(Mathf.Abs(transform.position.x - (currentLane * laneWidth)) < 0.001f) transform.position = targetPos;
        else transform.position = Vector3.Lerp(transform.position, targetPos, laneSwitchSpeed * Time.deltaTime);
    }

}
