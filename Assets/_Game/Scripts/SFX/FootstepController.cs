using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FootstepController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GladeSFX gladeSFX;          // drag your GladeSFX here
    [SerializeField] private CharacterController cc;

    [Header("Step Timing")]
    [Tooltip("Seconds between steps when walking normally.")]
    [SerializeField] private float walkStepInterval = 0.45f;

    [Tooltip("Seconds between steps when moving fast (optional).")]
    [SerializeField] private float runStepInterval = 0.32f;

    [Tooltip("Speed threshold to count as moving.")]
    [SerializeField] private float moveThreshold = 0.15f;

    [Tooltip("If your CC velocity is unreliable, you can use input instead.")]
    [SerializeField] private bool useInputInsteadOfVelocity = false;

    private float nextStepTime;

    private void Awake()
    {
        if (cc == null) cc = GetComponent<CharacterController>();
        if (gladeSFX == null) gladeSFX = GetComponent<GladeSFX>();
    }

    private void Update()
    {
        if (gladeSFX == null || cc == null) return;

        // Must be on ground
        if (!cc.isGrounded) return;

        bool isMoving;
        float speed;

        if (useInputInsteadOfVelocity)
        {
            // Simple input-based (good if your controller.velocity is weird)
            isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                       Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
            speed = isMoving ? 1f : 0f;
        }
        else
        {
            Vector3 v = cc.velocity;
            v.y = 0f;
            speed = v.magnitude;
            isMoving = speed > moveThreshold;
        }

        if (!isMoving) return;

        // Choose interval (walk vs run)
        // If you don't have running, just keep walk interval.
        float interval = walkStepInterval;

        // Example "run" detection: speed > 3
        if (!useInputInsteadOfVelocity && speed > 3f)
            interval = runStepInterval;

        if (Time.time >= nextStepTime)
        {
            gladeSFX.PlayFootstep();
            nextStepTime = Time.time + interval;
        }
    }
}
