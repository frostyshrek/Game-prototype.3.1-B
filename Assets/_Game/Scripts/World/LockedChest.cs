using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class LockedChest : MonoBehaviour
{
    public KeyItem requiredKey = KeyItem.AncientKey;
    public Animator anim;
    public UnityEvent onOpened;
    public UnityEvent onLockedFeedback;

    bool opened;
    bool playerInRange;
    Transform player;

    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        player = other.transform;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        player = null;
    }

    void Update()
    {
        if (!playerInRange || opened) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!GameState.I.HasKey(requiredKey))
            {
                onLockedFeedback?.Invoke();
                return;
            }

            opened = true;
            if (anim) anim.SetTrigger("Open");
            onOpened?.Invoke();
        }

        var aim = Camera.main ? Camera.main.GetComponent<AimRayProvider>() : null;
        if (aim && aim.TryGetAimHit(out var hit, 8f))
        {
            var myRoot = transform.root;
            if (!hit.collider.transform.IsChildOf(myRoot))
            {
                // Not actually aiming at THIS chest → ignore E
                return;
            }
        }
    }
}
