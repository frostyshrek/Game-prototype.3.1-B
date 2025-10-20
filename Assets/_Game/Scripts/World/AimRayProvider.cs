using UnityEngine;

public class AimRayProvider : MonoBehaviour
{
    Camera cam;

    void Awake() => cam = GetComponent<Camera>();

    public Ray GetAimRay()
    {
        Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        return cam.ScreenPointToRay(center);
    }

    public bool TryGetAimHit(out RaycastHit hit, float maxDistance = 1000f, int layerMask = Physics.DefaultRaycastLayers)
    {
        return Physics.Raycast(GetAimRay(), out hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
    }
}
