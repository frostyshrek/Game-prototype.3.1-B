using UnityEngine;

public class AimHighlighter : MonoBehaviour
{
    public float maxDistance = 8f;
    public LayerMask interactMask = ~0;

    AimRayProvider aim;
    IHighlightable current;

    void Awake()
    {
        aim = GetComponent<AimRayProvider>();
        if (aim == null) aim = gameObject.AddComponent<AimRayProvider>();
    }

    void Update()
    {
        IHighlightable next = null;

        if (aim.TryGetAimHit(out var hit, maxDistance, interactMask))
        {
            next = hit.collider.GetComponentInParent<IHighlightable>();
        }

        if (!ReferenceEquals(next, current))
        {
            if (current != null) current.SetHighlighted(false);
            current = next;
            if (current != null) current.SetHighlighted(true);
        }
    }

    void OnDisable()
    {
        if (current != null) current.SetHighlighted(false);
        current = null;
    }
}
