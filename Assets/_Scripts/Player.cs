using UnityEngine;

public class Player : MonoBehaviour {

    [Header("Stats")]
    [SerializeField] private float laneSwitchSpeed = 15f;
    [SerializeField] private float laneWidth = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravityMultiplier = 3f;
    [SerializeField] private float rollDuration = 1f;

    [Space, Header("References")]
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private Animator animator;

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
        if (GameInput.Instance.WasLeftPressed()) currentLane = Mathf.Max(-1, currentLane - 1);
        if (GameInput.Instance.WasRightPressed()) currentLane = Mathf.Min(1, currentLane + 1);

        if (GameInput.Instance.WasJumpPressed()) Jump();
        if (GameInput.Instance.WasRollPressed()) Roll();

        if(isRolling) {
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

        animator.SetFloat(runBlendHash, EnvSpawner.Instance.GetSpeedFactor());
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

        animator.SetTrigger(jumpHash);

        if (isRolling) isRolling = false;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void UpdateGrounded() {
        isGrounded = groundCheck.IsGrounded();
    }

    private void Roll() {
        if (isRolling) return;

        animator.SetTrigger(rollHash);

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(-Vector3.up * jumpForce, ForceMode.Impulse);

        isRolling = true;
        rollTimer = 0f;
    }

}
