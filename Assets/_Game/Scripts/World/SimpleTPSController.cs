using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleTPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 720f;

    [Header("Jump")]
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpForce = 6f;        // tweak
    public float gravity = -18f;        // tweak (more negative = heavier)
    public float groundedStick = -2f;   // keeps controller grounded nicely

    [Header("References")]
    public Transform cam;
    Animator animator;
    CharacterController controller;

    [Header("Animation")]
    public string speedParam = "Speed";
    public float speedDampTime = 0.1f;

    [Header("Interact")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 2.2f;
    public LayerMask interactMask = ~0; // everything by default
    public Vector3 interactOriginOffset = new Vector3(0f, 1.2f, 0f);

    // --- Jump state ---
    private float verticalVelocity;
    private bool isGrounded;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        if (cam == null && Camera.main) cam = Camera.main.transform;
    }

    void Update()
    {
        if (Time.timeScale == 0f || cam == null) return;

        // --- Interact ---
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }

        // --- Ground Check ---
        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = groundedStick;

        // --- Jump ---
        if (isGrounded && Input.GetKeyDown(jumpKey))
        {
            verticalVelocity = jumpForce;
            // If you later add a Jump animation trigger, you can do it here.
            // animator?.SetTrigger("Jump");
        }

        // --- Movement ---
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 fwd = cam.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = cam.right; right.y = 0f; right.Normalize();
        Vector3 move = fwd * v + right * h;

        float inputMag = Mathf.Clamp01(move.magnitude);

        if (inputMag > 0.0001f)
        {
            Quaternion to = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, to, rotationSpeed * Time.deltaTime);
        }

        // --- Gravity ---
        verticalVelocity += gravity * Time.deltaTime;

        // --- Apply final velocity ---
        Vector3 velocity = move.normalized * (moveSpeed * inputMag);
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        // --- Animate ---
        if (animator)
            animator.SetFloat(speedParam, inputMag, speedDampTime, Time.deltaTime);
    }

    private void TryInteract()
    {
        Vector3 origin = transform.position + interactOriginOffset;
        Vector3 dir = cam.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, interactRange, interactMask, QueryTriggerInteraction.Collide))
        {
            // Look for IInteractable on hit object or its parents
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (cam == null) return;
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position + interactOriginOffset;
        Gizmos.DrawLine(origin, origin + cam.forward * interactRange);
    }
#endif
}
