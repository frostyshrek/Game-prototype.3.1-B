using UnityEngine;
using UnityEngine.UI;

public class WorldPrompt : MonoBehaviour
{
    public CanvasGroup group;
    public Text text;

    public void Show(string msg)
    {
        text.text = msg;
        group.alpha = 1;
        group.interactable = false; group.blocksRaycasts = false;
    }

    public void Hide() => group.alpha = 0;
}
