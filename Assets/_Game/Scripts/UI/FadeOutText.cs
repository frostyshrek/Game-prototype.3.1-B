using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class FadeOutText : MonoBehaviour
{
    public float delay = 2f;       // how long to stay visible
    public float fadeDuration = 1f; // how long it takes to fade out

    CanvasGroup group;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f; // start invisible
    }

    void OnEnable()
    {
        StopAllCoroutines();
        group.alpha = 1f; // appear instantly
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(delay);
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        group.alpha = 0f;
        gameObject.SetActive(false); // optional: disable after fade
    }
}
