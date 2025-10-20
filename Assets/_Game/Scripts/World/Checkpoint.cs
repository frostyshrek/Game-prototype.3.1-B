using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    public Transform marker; // optional exact spawn point; fallback to self

    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        GameState.I.SetCheckpoint(marker ? marker : transform);
        // TODO: show small UI toast "Checkpoint reached"
    }
}
