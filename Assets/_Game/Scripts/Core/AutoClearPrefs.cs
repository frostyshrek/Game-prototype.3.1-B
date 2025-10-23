using UnityEngine;

public class AutoClearPrefs : MonoBehaviour
{
    void Awake()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs cleared at startup (prototype mode)");
    }
}