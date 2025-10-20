using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject crosshair;
    public KeyCode toggleKey = KeyCode.Escape;

    bool paused;

    void Awake()
    {
        if (pausePanel) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        UnlockCursor(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (paused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (paused) return;
        paused = true;

        Time.timeScale = 0f;

        if (pausePanel) pausePanel.SetActive(true);
        if (crosshair) crosshair.SetActive(false);

        UnlockCursor(true);
    }

    public void Resume()
    {
        if (!paused) return;
        paused = false;

        Time.timeScale = 1f;

        if (pausePanel) pausePanel.SetActive(false);
        if (crosshair) crosshair.SetActive(true);

        UnlockCursor(false);
    }

    public void SaveGame()
    {
        if (GameState.I != null)
        {
            GameState.I.QuickSave();
            Debug.Log("Game saved.");
        }
        else
        {
            Debug.LogWarning("No GameState in scene. Cannot save.");
        }
    }

    public void ExitGame()
    {
        // TODO: SceneManager.LoadScene("MainMenu");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void UnlockCursor(bool unlock)
    {
        Cursor.visible = unlock;
        Cursor.lockState = unlock ? CursorLockMode.None : CursorLockMode.Locked;
    }

    void OnDisable()
    {
        // Safety: restore time if object is disabled while paused
        if (paused) Time.timeScale = 1f;
    }
}
