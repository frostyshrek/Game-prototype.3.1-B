using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public string message;
    public TutorialManager tutorialManager;
    public KeyCode keyToDismiss; // assign in inspector

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            tutorialManager.ShowMessage(message, keyToDismiss);
        }
    }
}
