using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawnOnLoad : MonoBehaviour
{
    void Start()
    {
        // If we came back to the checkpoint scene, place the player there
        if (GameState.I == null) return;

        var active = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(GameState.I.CheckpointScene) &&
            GameState.I.CheckpointScene == active &&
            GameState.I.CheckpointRot != Quaternion.identity)
        {
            transform.SetPositionAndRotation(GameState.I.CheckpointPos, GameState.I.CheckpointRot);
        }
    }
}
