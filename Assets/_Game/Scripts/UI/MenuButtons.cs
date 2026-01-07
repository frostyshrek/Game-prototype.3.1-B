using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public void NewGame()
    {
        SceneManager.LoadScene("Glade");
    }

    public void LoadGame()
    {
        Debug.Log("Load Game");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

