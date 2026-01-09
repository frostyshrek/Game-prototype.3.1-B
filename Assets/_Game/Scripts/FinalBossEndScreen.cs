using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalBossEndScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup endPanelGroup; // CanvasGroup on EndPanel
    [SerializeField] private TMP_Text endText;          // TMP on EndText

    [Header("Text")]
    [TextArea(2, 6)]
    [SerializeField] private string message =
        "you saved the world Thank you!\n" +
        "Spirit of the Dark Cards By Group 3.1";

    [Header("Timing")]
    [SerializeField] private float fadeInTime = 1.2f;
    [SerializeField] private float delayBeforeText = 0.3f;
    [SerializeField] private float typeSpeed = 0.02f;

    [Header("Optional")]
    [SerializeField] private bool freezeGame = true; // pauses gameplay when end shows

    private bool triggered;

    private void Awake()
    {
        if (endPanelGroup != null)
        {
            endPanelGroup.alpha = 0f;
            endPanelGroup.interactable = false;
            endPanelGroup.blocksRaycasts = false;
        }

        if (endText != null)
            endText.text = "";
    }

    // Call this when final boss is defeated
    public void TriggerEnd()
    {
        if (triggered) return;
        triggered = true;
        StartCoroutine(EndFlow());
    }

    private IEnumerator EndFlow()
    {
        if (freezeGame)
            Time.timeScale = 0f;

        if (endPanelGroup != null)
        {
            endPanelGroup.blocksRaycasts = true;
            endPanelGroup.interactable = true;
        }

        // Fade in (use unscaled time so it still works when timeScale = 0)
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.unscaledDeltaTime;
            if (endPanelGroup != null)
                endPanelGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInTime);
            yield return null;
        }

        if (endPanelGroup != null)
            endPanelGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(delayBeforeText);

        // Type text
        if (endText != null)
        {
            endText.text = "";
            for (int i = 0; i < message.Length; i++)
            {
                endText.text += message[i];
                yield return new WaitForSecondsRealtime(typeSpeed);
            }
        }
    }
}
