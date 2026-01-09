using UnityEngine;
using UnityEngine.SceneManagement;

public class Script_continue_Quit : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string fallbackScene = "Glade";

    // Called by Continue button
    public void OnClickContinue()
    {
        if (GameState.I == null)
        {
            SceneManager.LoadScene(fallbackScene);
            return;
        }

        GameState.I.QuickLoad();

        // Load saved scene if exists, otherwise Glade
        string scene = GameState.I.CheckpointScene;
        if (string.IsNullOrEmpty(scene))
            scene = fallbackScene;

        SceneManager.LoadScene(scene);
    }

    // Called by Quit button (optional if you already handle Quit elsewhere)
    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
