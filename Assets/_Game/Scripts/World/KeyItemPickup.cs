using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KeyItemPickup : MonoBehaviour
{
    public KeyItem item = KeyItem.AncientKey;

    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        GameState.I.GiveKey(item);
        // TODO: play VFX / SFX / UI toast
        Destroy(gameObject);
    }
}
