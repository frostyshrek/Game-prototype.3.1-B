using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BattleOrbitMovement : MonoBehaviour
{

    public Animator animator;

    [Header("References")]
    [SerializeField] private Transform enemy;

    [Header("Orbit Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float moveSpeedDegPerSec = 120f; // how fast A/D spins around enemy

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Dash Settings")]
    [SerializeField] private float dashAngle = 35f;      // how many degrees to snap around enemy
    [SerializeField] private float dashDuration = 0.15f; // how long dash "state" lasts
    [SerializeField] private float doubleTapTime = 0.25f;
    [SerializeField] private float dashCooldown =0.8f;

    [Header("Duck Settings")]
    [SerializeField] private float duckHeightScale = 0.5f;

    // state
    public bool IsJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsDucking { get; private set; }

    private float currentAngleDeg;
    private float verticalVelocity;
    private CharacterController controller;
    private Vector3 velocity;

    private float lastTapLeftTime = -999f;
    private float lastTapRightTime = -999f;
    private float lastDashTime = -999f;

    private Vector3 originalControllerCenter;
    private float originalControllerHeight;

    private bool canMove = true; // BattleManager can turn this off during enemy turn, etc.

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        originalControllerCenter = controller.center;
        originalControllerHeight = controller.height;
    }

    private void Start()
    {
        if (enemy == null)
        {
            Debug.LogError("BattleOrbitMovement: Enemy reference not set!");
            enabled = false;
            return;
        }

        // compute starting angle from enemy to player
        Vector3 toPlayer = transform.position - enemy.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.001f)
        {
            // if we're on top of enemy, just place us at radius in front
            currentAngleDeg = 0f;
            toPlayer = Vector3.forward * radius;
        }
        else
        {
            currentAngleDeg = Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg;
        }

        // snap player to exact radius
        Vector3 flatPos = toPlayer.normalized * radius;
        transform.position = enemy.position + flatPos;
        LookAtEnemy();
    }

    private void Update()
    {
        if (!canMove) return;

        HandleOrbitMovement();
        HandleJumpAndGravity();
        HandleDashInput();
        HandleDuck();

        if (animator != null)
        {
            bool isMoving = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
            float animSpeed = isMoving ? 1f : 0f;

            animator.SetFloat("Speed", animSpeed);
            animator.SetBool("IsDucking", IsDucking);
        }

        LookAtEnemy();
    }

    private void HandleOrbitMovement()
    {
        // A/D movement around enemy
        float horiz = 0f;
        if (Input.GetKey(KeyCode.A)) horiz = -1f;
        if (Input.GetKey(KeyCode.D)) horiz = 1f;

        if (!IsDashing) // during dash, we might override movement
        {
            currentAngleDeg -= horiz * moveSpeedDegPerSec * Time.deltaTime;
        }

        // compute orbit position
        float rad = currentAngleDeg * Mathf.Deg2Rad;
        Vector3 orbitPos = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * radius;
        Vector3 targetPos = enemy.position + orbitPos;

        // CharacterController needs a delta move vector
        Vector3 move = targetPos - new Vector3(transform.position.x, enemy.position.y, transform.position.z);
        move.y = 0f; // horizontal only here, vertical handled in gravity

        // add vertical component
        move.y = verticalVelocity * Time.deltaTime;

        controller.Move(move);
    }

    private void HandleJumpAndGravity()
    {
        bool grounded = controller.isGrounded;

        if (grounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
            IsJumping = false;
        }

        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            // v = sqrt(2 * h * -g)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            IsJumping = true;
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void HandleDashInput()
    {
        // Left dash (A double tap)
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (Time.time - lastTapLeftTime <= doubleTapTime)
            {
                StartCoroutine(DashCoroutine(1)); // left
            }
            lastTapLeftTime = Time.time;
        }

        // Right dash (D double tap)
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (Time.time - lastTapRightTime <= doubleTapTime)
            {
                StartCoroutine(DashCoroutine(-1)); // right
            }
            lastTapRightTime = Time.time;
        }
    }

    private System.Collections.IEnumerator DashCoroutine(int direction)
    {
        // respect cooldown
        if (IsDashing) yield break;
        if (Time.time < lastDashTime + dashCooldown) yield break;

        IsDashing = true;
        lastDashTime = Time.time;   // start cooldown

        // snap angle by dashAngle instantly (feels snappy)
        currentAngleDeg += direction * dashAngle;

        float t = 0f;
        while (t < dashDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        IsDashing = false;
    }

    private void HandleDuck()
    {
        bool duckInput = Input.GetKey(KeyCode.S);

        if (duckInput && !IsDucking)
        {
            // enter duck
            IsDucking = true;
            controller.height = originalControllerHeight * duckHeightScale;
            controller.center = originalControllerCenter - new Vector3(0f, originalControllerHeight * (1f - duckHeightScale) * 0.5f, 0f);
        }
        else if (!duckInput && IsDucking)
        {
            // exit duck
            IsDucking = false;
            controller.height = originalControllerHeight;
            controller.center = originalControllerCenter;
        }
    }

    private void LookAtEnemy()
    {
        Vector3 lookPos = enemy.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
    }

    // Optional API for BattleManager / EnemyController:
    public void SetCanMove(bool value) => canMove = value;
}
