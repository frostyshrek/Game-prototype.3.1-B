using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CameraOrbitFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 focusOffset = new Vector3(0f, 1.3f, 0f);

    [Header("Orbit")]
    public float mouseSensitivity = 120f;
    public float minPitch = -20f;
    public float maxPitch = 70f;

    [Header("Distance")]
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 8f;
    public float zoomSensitivity = 3f;

    [Header("Smoothing")]
    public float followSmooth = 10f;

    float yaw;
    float pitch;

    void Start()
    {
        var e = transform.eulerAngles;
        yaw = e.y;
        pitch = Mathf.Clamp(e.x, minPitch, maxPitch);
        // Optional:
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector2 look = ReadLookInput();      // Mouse delta
        float scroll = ReadScrollInput();    // Mouse wheel

        yaw   += look.x * mouseSensitivity * Time.deltaTime;
        pitch -= look.y * mouseSensitivity * Time.deltaTime;
        pitch  = Mathf.Clamp(pitch, minPitch, maxPitch);

        distance = Mathf.Clamp(distance - scroll * zoomSensitivity, minDistance, maxDistance);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + focusOffset;
        Vector3 desiredPos = focus - rot * Vector3.forward * distance;

        // (Optional) simple camera collision: keep clear of walls
        if (Physics.SphereCast(focus, 0.2f, (desiredPos - focus).normalized, out RaycastHit hit, distance))
            desiredPos = hit.point + hit.normal * 0.2f;

        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, followSmooth * Time.deltaTime);
    }

    Vector2 ReadLookInput()
    {
        #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.delta.ReadValue();
        #endif
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    float ReadScrollInput()
    {
        #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.scroll.ReadValue().y / 120f; // normalize like old system
        #endif
        return Input.mouseScrollDelta.y;
    }
}
