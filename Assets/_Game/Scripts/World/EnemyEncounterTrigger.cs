using UnityEngine;
using UnityEngine.SceneManagement;
using BattleSystem;

[RequireComponent(typeof(Collider))]
public class EnemyEncounterTrigger : MonoBehaviour
{
    [Header("Encounter Id (unique per enemy)")]
    public string encounterId = "Skeleton_Foot_Soldier";

    [Header("What enemy to spawn in battle")]
    public EnemyData enemyData;

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
        if (GameState.I != null && GameState.I.IsEncounterDefeated(encounterId))
        {
            gameObject.SetActive(false);
        }
        Debug.Log($"[{name}] encounterId={encounterId} defeated? {GameState.I?.IsEncounterDefeated(encounterId)}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;

        triggered = true;
        gameObject.SetActive(false); // hide immediately to avoid re-trigger

        if (GameState.I != null)
        {
            // Do NOT set checkpoint here – otherwise it respawns inside the trigger.
            // GameState.I.SetCheckpoint(other.transform);
            GameState.I.SetLastEncounterId(encounterId);      // remember which encounter
            GameState.I.SetCurrentEncounter(enemyData);
        }
        else
        {
            Debug.LogError("No GameState found in scene!");
        }

        if (encounterEffect) Instantiate(encounterEffect, transform.position, Quaternion.identity);

        Invoke(nameof(LoadBattleScene), delayBeforeLoad);
    }

    void LoadBattleScene()
    {
        SceneManager.LoadScene(battleSceneName);
    }
}