using UnityEngine;

public class Player : MonoBehaviour {

    [Header("Stats")]
    [SerializeField] private float laneSwitchSpeed = 15f;
    [SerializeField] private float laneWidth = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravityMultiplier = 3f;
    [SerializeField] private float rollDuration = 1f;
    [SerializeField] private float inputBufferTime = 0.15f;

    [Space, Header("References")]
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private Animator animator;
    [SerializeField] private PoseGestureClassifier classifier;

    private float jumpBufferTimer;
    private float rollBufferTimer;

    private int currentLane = 0;
    private bool isGrounded;
    private bool isRolling;
    private float rollTimer;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private float initCapHeight;

    private int runBlendHash;
    private int jumpHash;
    private int rollHash;

    private void OnEnable() {
        if (classifier == null) return;

        classifier.LaneChanged += OnLaneChanged;
        classifier.JumpDetected += OnJump;
        classifier.DuckChanged += OnDuckChanged;
    }

    private void OnDisable() {
        if (classifier == null) return;

        classifier.LaneChanged -= OnLaneChanged;
        classifier.JumpDetected -= OnJump;
        classifier.DuckChanged -= OnDuckChanged;
    }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Start() {
        currentLane = 0;
        isRolling = false;
        rollTimer = 0f;
        initCapHeight = capsuleCollider.height;

        runBlendHash = Animator.StringToHash("RunBlend");
        jumpHash = Animator.StringToHash("Jump");
        rollHash = Animator.StringToHash("Roll");
    }

    private void Update() {
        if (!GameManager.Instance.IsPaused() && !GameManager.Instance.IsGameOver()) {
            if (GameInput.Instance.WasLeftPressed()) currentLane = Mathf.Max(-1, currentLane - 1);
            if (GameInput.Instance.WasRightPressed()) currentLane = Mathf.Min(1, currentLane + 1);
        }

        //if (GameInput.Instance.WasJumpPressed()) Jump();
        //if (GameInput.Instance.WasRollPressed()) Roll();

        if (GameInput.Instance.WasJumpPressed()) jumpBufferTimer = inputBufferTime;
        else jumpBufferTimer = Mathf.Max(jumpBufferTimer - Time.deltaTime, -1f);

        if (GameInput.Instance.WasRollPressed()) rollBufferTimer = inputBufferTime;
        else rollBufferTimer = Mathf.Max(rollBufferTimer - Time.deltaTime, -1f);

        if (jumpBufferTimer > 0f) Jump();
        if (rollBufferTimer > 0f) Roll();

        if (isRolling) {
            rollTimer += Time.deltaTime;
            if(rollTimer > rollDuration) {
                rollTimer = 0;
                isRolling = false;
            }
        }

        if (isRolling) {
            capsuleCollider.height = initCapHeight / 2;
            capsuleCollider.center = new Vector3(0, initCapHeight / 4, 0);
        } else {
            capsuleCollider.height = initCapHeight;
            capsuleCollider.center = new Vector3(0, initCapHeight / 2, 0);
        }

        animator.SetFloat(runBlendHash, EnvSpawner.Instance.GetSpeedAnimFactor());
    }

    private void FixedUpdate() {
        LerpToLane();
        UpdateGrounded();

        rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Death") GameManager.Instance.GameOver();
    }

    private void OnLaneChanged(int lane) {
        currentLane = lane;
    }

    private void OnJump() {
        Jump();
    }

    private void OnDuckChanged(bool ducking) {
        Roll();
    }

    private void LerpToLane() {
        Vector3 targetPos = new Vector3(currentLane * laneWidth, rb.position.y, rb.position.z);

        Vector3 newPos = Vector3.Lerp(rb.position, targetPos, laneSwitchSpeed * Time.fixedDeltaTime);

        rb.MovePosition(newPos);
    }

    public void Jump() {
        if (GameManager.Instance.IsPaused() || GameManager.Instance.IsGameOver()) return;
        if (!isGrounded) return;

        jumpBufferTimer = -1f;

        animator.SetTrigger(jumpHash);

        if (isRolling) isRolling = false;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void UpdateGrounded() {
        isGrounded = groundCheck.IsGrounded();
    }

    private void Roll() {
        if (GameManager.Instance.IsPaused() || GameManager.Instance.IsGameOver()) return;
        if (isRolling) return;

        rollBufferTimer = -1f;

        animator.SetTrigger(rollHash);

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(-Vector3.up * jumpForce, ForceMode.Impulse);

        isRolling = true;
        rollTimer = 0f;
    }

}
