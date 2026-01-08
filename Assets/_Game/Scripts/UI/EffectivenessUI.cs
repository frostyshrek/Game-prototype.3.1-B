using System.Collections;
using TMPro;
using UnityEngine;

public class EffectivenessUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup group; // can be on parent (Battle_Feedback)

    [Header("Timing")]
    [SerializeField] private float fadeIn = 0.08f;
    [SerializeField] private float hold = 0.35f;
    [SerializeField] private float fadeOut = 0.15f;

    Coroutine routine;

    private void Awake()
    {
        if (text == null) text = GetComponent<TMP_Text>();

        // If CanvasGroup not assigned, try find on this OR parent
        if (group == null)
        {
            group = GetComponent<CanvasGroup>();
            if (group == null) group = GetComponentInParent<CanvasGroup>();
        }

        if (group != null) group.alpha = 0f;
        if (text != null) text.text = "";
    }

    public void Show(string message)
    {
        if (text == null || group == null) return;

        text.text = message;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return FadeTo(1f, fadeIn);
        yield return new WaitForSeconds(hold);
        yield return FadeTo(0f, fadeOut);

        if (text != null) text.text = "";
        routine = null;
    }

    private IEnumerator FadeTo(float target, float time)
    {
        float start = group.alpha;
        float t = 0f;

        if (time <= 0f)
        {
            group.alpha = target;
            yield break;
        }

        while (t < time)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, target, t / time);
            yield return null;
        }

        group.alpha = target;
    }
}
