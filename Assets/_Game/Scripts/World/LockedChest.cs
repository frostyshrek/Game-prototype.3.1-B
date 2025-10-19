using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class LockedChest : MonoBehaviour
{
    public KeyItem requiredKey = KeyItem.AncientKey;
    public Animator anim;                 // optional
    public UnityEvent onOpened;           // hook up UI panel / reward
    public UnityEvent onLockedFeedback;   // show "Need a key" prompt

    bool opened;

    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (opened || !other.CompareTag("Player")) return;

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
    }
}
