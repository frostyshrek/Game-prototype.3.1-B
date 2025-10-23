using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class EnemyEncounterTrigger : MonoBehaviour
{
    [Header("Encounter Id (unique per enemy)")]
    public string encounterId = "Skeleton_01";

    [Header("Scene to load on encounter")]
    public string battleSceneName = "Battle";

    [Header("Optional: VFX or delay")]
    public float delayBeforeLoad = 0.5f;
    public GameObject encounterEffect;

    bool triggered;

    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void Start()
    {
        // If this encounter was already cleared, hide it
        if (GameState.I != null && GameState.I.IsEncounterDefeated(encounterId))
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;

        triggered = true;
        gameObject.SetActive(false); // hide immediately to avoid re-trigger

        if (GameState.I != null)
        {
            GameState.I.SetCheckpoint(other.transform);
            GameState.I.SetLastEncounterId(encounterId); // <-- remember who started this battle
        }

        if (encounterEffect) Instantiate(encounterEffect, transform.position, Quaternion.identity);

        Invoke(nameof(LoadBattleScene), delayBeforeLoad);
    }

    void LoadBattleScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(battleSceneName);
    }
}

