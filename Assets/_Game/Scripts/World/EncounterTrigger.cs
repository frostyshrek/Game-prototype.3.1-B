using UnityEngine;
using UnityEngine.SceneManagement;

// TODO: call this from the battle controller when enemies are defeated
//
// GameState.I.GiveKey(KeyItem.AncientKey);
// UnityEngine.SceneManagement.SceneManager.LoadScene("Glade");

[RequireComponent(typeof(Collider))]
public class EncounterTrigger : MonoBehaviour
{
    public string battleScene = "Battle";
    bool triggered;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        // Save current spot for respawn
        GameState.I.SetCheckpoint(other.transform);
        SceneManager.LoadScene(battleScene);
    }
}
