using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleTPSController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 720f;
    public Transform cam;

    CharacterController controller;

    void Awake() => controller = GetComponent<CharacterController>();

    void Update()
    {
        Vector2 input = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector3 move = (cam.forward * input.y + cam.right * input.x);
        move.y = 0f;

        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotationSpeed * Time.deltaTime);
        }

        controller.SimpleMove(move.normalized * moveSpeed);
    }
}

