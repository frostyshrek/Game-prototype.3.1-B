using System.Collections;
using TMPro;
using UnityEngine;

public class GladeFeedbackMessageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup group;

    public void Play(string msg, Color color, float holdTime, float fadeTime)
    {
        if (text == null) text = GetComponentInChildren<TMP_Text>(true);
        if (group == null) group = GetComponent<CanvasGroup>();

        text.text = msg;
        text.color = color;

        StopAllCoroutines();
        StartCoroutine(Flow(holdTime, fadeTime));
    }

    private IEnumerator Flow(float hold, float fade)
    {
        if (group != null) group.alpha = 1f;

        yield return new WaitForSeconds(hold);

        float t = 0f;
        while (t < fade)
        {
            t += Time.deltaTime;
            if (group != null) group.alpha = Mathf.Lerp(1f, 0f, t / fade);
            yield return null;
        }

        Destroy(gameObject);
    }
}
