using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EnemyTelegraphUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text infoText;      // "EARTHQUAKE — JUMP"
    [SerializeField] private TMP_Text timerText;     // "0.8s"
    [SerializeField] private CanvasGroup canvasGroup;

    private Coroutine timerRoutine;

    private void Awake()
    {
        HideInstant();
    }

    public void Show(EnemyAttackPattern pattern, RequiredDodge dodge)
    {
        if (pattern == null) return;

        // text line
        if (infoText != null)
        {
            string dodgeLabel = DodgeLabel(dodge);
            infoText.text = $"{pattern.attackName} — {dodgeLabel}";
        }

        // start timer visuals
        StartTelegraphTimer(pattern.telegraphTime);
        FadeTo(1f);
    }

    public void Hide()
    {
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        FadeTo(0f);
    }

    private void HideInstant()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (timerText != null) timerText.text = "";
    }

    private void StartTelegraphTimer(float telegraphTime)
    {
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        timerRoutine = StartCoroutine(TelegraphTimerRoutine(Mathf.Max(0.05f, telegraphTime)));
    }

    private IEnumerator TelegraphTimerRoutine(float duration)
    {
        float t = 0f;


        while (t < duration)
        {
            t += Time.deltaTime;
            float remaining = Mathf.Max(0f, duration - t);

            // countdown text
            if (timerText != null)
                timerText.text = $"{remaining:0.0}s";

            yield return null;
        }

        // end state
        if (timerText != null) timerText.text = "0.0s";

        timerRoutine = null;
    }

    private void FadeTo(float a)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = a;
    }

    private string DodgeLabel(RequiredDodge dodge)
    {
        switch (dodge)
        {
            case RequiredDodge.Jump: return "JUMP";
            case RequiredDodge.DashLeft: return "DASH ←";
            case RequiredDodge.DashRight: return "DASH →";
            case RequiredDodge.Parry: return "PARRY";
            default: return "DODGE";
        }
    }
}
