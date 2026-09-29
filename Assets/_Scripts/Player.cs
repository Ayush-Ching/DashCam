using UnityEngine;

public class Player : MonoBehaviour {

    [Header("Stats")]
    [SerializeField] private float laneSwitchSpeed = 15f;
    [SerializeField] private float laneWidth = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravityMultiplier = 3f;
    [SerializeField] private float slideDuration = 1f;

    [Space, Header("References")]
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private GameObject longBod;
    [SerializeField] private GameObject rollBod;

    private int currentLane = 0;
    private bool isGrounded;
    private bool isSliding;
    private float slideTimer;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private float initCapHeight;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        initCapHeight = capsuleCollider.height;

    }

    private void Start() {
        currentLane = 0;
        isSliding = false;
        slideTimer = 0f;
    }

    private void Update() {
        if (GameInput.Instance.WasLeftPressed()) currentLane = Mathf.Max(-1, currentLane - 1);
        if (GameInput.Instance.WasRightPressed()) currentLane = Mathf.Min(1, currentLane + 1);

        if (GameInput.Instance.WasJumpPressed()) Jump();
        if (GameInput.Instance.WasSlidePressed()) Slide();

        if(isSliding) {
            slideTimer += Time.deltaTime;
            if(slideTimer > slideDuration) {
                slideTimer = 0;
                isSliding = false;
            }
        }

        if (isSliding) {
            capsuleCollider.height = initCapHeight / 2;
            capsuleCollider.center = new Vector3(0, initCapHeight / 4, 0);

            longBod.SetActive(false);
            rollBod.SetActive(true);
        } else {
            capsuleCollider.height = initCapHeight;
            capsuleCollider.center = new Vector3(0, initCapHeight / 2, 0);

            longBod.SetActive(true);
            rollBod.SetActive(false);
        }
    }

    private void FixedUpdate() {
        LerpToLane();
        UpdateGrounded();

        rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Death") GameManager.Instance.GameOver();
    }

    private void LerpToLane() {
        Vector3 targetPos = new Vector3(currentLane * laneWidth, rb.position.y, rb.position.z);

        Vector3 newPos = Vector3.Lerp(rb.position, targetPos, laneSwitchSpeed * Time.fixedDeltaTime);

        rb.MovePosition(newPos);
    }

    public void Jump() {
        if(!isGrounded) return;

        if (isSliding) isSliding = false;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void UpdateGrounded() {
        isGrounded = groundCheck.IsGrounded();
    }

    private void Slide() {
        if (isSliding) return;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(-Vector3.up * jumpForce, ForceMode.Impulse);

        isSliding = true;
        slideTimer = 0f;
    }

}
