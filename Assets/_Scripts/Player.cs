using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour {

    [Header("Stats")]
    [SerializeField] private float laneSwitchSpeed = 15f;
    [SerializeField] private float laneWidth = 10f;
    [SerializeField] private float jumpForce = 10f;

    [Space, Header("References")]
    [SerializeField] private GroundCheck groundCheck;

    private int currentLane = 0;
    private bool isGrounded;

    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void Start() {
        currentLane = 0;
    }

    private void Update() {
        if (GameInput.Instance.WasLeftPressed()) currentLane = Mathf.Max(-1, currentLane - 1);
        if (GameInput.Instance.WasRightPressed()) currentLane = Mathf.Min(1, currentLane + 1);

        if (GameInput.Instance.WasJumpPressed()) Jump();
    }

    private void FixedUpdate() {
        LerpToLane();
        UpdateGrounded();
    }

    private void LerpToLane() {
        Vector3 targetPos = new Vector3(currentLane * laneWidth, rb.position.y, rb.position.z);

        Vector3 newPos = Vector3.Lerp(rb.position, targetPos, laneSwitchSpeed * Time.fixedDeltaTime);

        rb.MovePosition(newPos);
    }

    public void Jump() {
        if(!isGrounded) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void UpdateGrounded() {
        isGrounded = groundCheck.IsGrounded();
    }

}
