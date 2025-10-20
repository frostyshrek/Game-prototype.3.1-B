using UnityEngine;
using UnityEngine.SceneManagement;

// TODO: when the battle ends, you can just do UnityEngine.SceneManagement.SceneManager.LoadScene("Glade");

[RequireComponent(typeof(Collider))]
public class EnemyEncounterTrigger : MonoBehaviour
{
    [Header("Scene to load on encounter")]
    public string battleSceneName = "Battle";

    [Header("Optional: VFX or delay")]
    public float delayBeforeLoad = 0.5f;
    public GameObject encounterEffect;

    bool triggered;

    void Reset()
    {
        // make sure collider is a trigger
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        gameObject.SetActive(false);

        // Save checkpoint for respawn (optional)
        if (GameState.I != null)
            GameState.I.SetCheckpoint(other.transform);

        // Optional effect or sound
        if (encounterEffect) Instantiate(encounterEffect, transform.position, Quaternion.identity);

        // Delay load if you want a flash/fade
        Invoke(nameof(LoadBattleScene), delayBeforeLoad);
    }

    void LoadBattleScene()
    {
        SceneManager.LoadScene(battleSceneName);
    }
}
