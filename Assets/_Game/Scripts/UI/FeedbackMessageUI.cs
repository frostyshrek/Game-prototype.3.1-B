using System.Collections;
using TMPro;
using UnityEngine;

public class FeedbackMessageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup group;

    private Coroutine routine;

    private void Awake()
    {
        if (text == null) text = GetComponentInChildren<TMP_Text>(true);
        if (group == null) group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
    }

    public void Play(string msg, float duration)
    {
        text.text = msg;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Co(duration));
    }

    private IEnumerator Co(float duration)
    {
        // fade in quickly
        group.alpha = 0f;
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, t / 0.15f);
            yield return null;
        }
        group.alpha = 1f;

        // hold
        yield return new WaitForSecondsRealtime(duration);

        // fade out
        t = 0f;
        while (t < 0.25f)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, t / 0.25f);
            yield return null;
        }

        Destroy(gameObject);
    }

    public void SetColor(Color c)
    {
        if (text != null)
            text.color = c;
    }
}
