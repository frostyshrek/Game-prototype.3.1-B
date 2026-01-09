using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;

    private bool isShowing = false;
    private KeyCode requiredKey; // The key required to hide the current tutorial

    private void Start()
    {
        tutorialText.text = "";
        tutorialText.gameObject.SetActive(false);
    }

    // Show a tutorial message and set the required key to dismiss it
    public void ShowMessage(string message, KeyCode keyToDismiss)
    {
        tutorialText.text = message;
        tutorialText.gameObject.SetActive(true);
        isShowing = true;
        requiredKey = keyToDismiss;
    }

    private void Update()
    {
        if (isShowing && Input.GetKeyDown(requiredKey))
        {
            HideMessage();
        }
    }

    private void HideMessage()
    {
        tutorialText.gameObject.SetActive(false);
        tutorialText.text = "";
        isShowing = false;
    }
}

