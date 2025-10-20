using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleTPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 720f;

    [Header("References")]
    public Transform cam;              
    Animator animator;                 
    CharacterController controller;

    [Header("Animation")]
    public string speedParam = "Speed";
    public float speedDampTime = 0.1f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator   = GetComponentInChildren<Animator>();
        if (cam == null && Camera.main) cam = Camera.main.transform;
    }

    void Update()
    {
        if (Time.timeScale == 0f || cam == null) return;

        // --- Input ---
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // --- Camera-relative move vector (flat on ground) ---
        Vector3 fwd = cam.forward;  fwd.y = 0f;  fwd.Normalize();
        Vector3 right = cam.right;  right.y = 0f; right.Normalize();
        Vector3 move = fwd * v + right * h;

        float inputMag = Mathf.Clamp01(move.magnitude);

        // --- Face move direction ---
        if (inputMag > 0.0001f)
        {
            Quaternion to = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, to, rotationSpeed * Time.deltaTime);
        }

        // --- Move (SimpleMove applies gravity) ---
        controller.SimpleMove(move.normalized * (moveSpeed * inputMag));

        // --- Animate ---
        if (animator)
            animator.SetFloat(speedParam, inputMag, speedDampTime, Time.deltaTime);
    }
}
