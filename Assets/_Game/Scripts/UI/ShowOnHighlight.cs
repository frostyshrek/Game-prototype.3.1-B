using UnityEngine;

public class ShowOnHighlight : MonoBehaviour, IHighlightable
{
    public GameObject uiToShow;

    void Awake()
    {
        if (uiToShow) uiToShow.SetActive(false);
    }

    public void SetHighlighted(bool on)
    {
        if (uiToShow) uiToShow.SetActive(on);
    }
}
