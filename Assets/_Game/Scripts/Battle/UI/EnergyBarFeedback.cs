using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnergyBarFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fillImage;        // the blue Fill image
    [SerializeField] private RectTransform barRect;  // the RectTransform of the slider or its parent

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.blue;
    [SerializeField] private Color errorColor = Color.red;

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeMagnitude = 6f;

    // [Header("Flash Settings")]
    // [SerializeField] private float flashDuration = 0.15f;

    private Coroutine feedbackRoutine;
    private Vector2 originalPos;   // 🔹 Vector2 instead of Vector3

    private void Awake()
    {
        if (fillImage != null)
            normalColor = fillImage.color;

        if (barRect == null)
            barRect = GetComponent<RectTransform>();

        if (barRect != null)
            originalPos = barRect.anchoredPosition;
    }

    public void PlayNotEnoughEnergyFeedback()
    {
        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackRoutine = StartCoroutine(FeedbackCoroutine());
    }

    private IEnumerator FeedbackCoroutine()
    {
        float elapsed = 0f;

        // flash red
        if (fillImage != null)
            fillImage.color = errorColor;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            if (barRect != null)
            {
                float offsetX = Random.Range(-shakeMagnitude, shakeMagnitude);
                float offsetY = Random.Range(-shakeMagnitude, shakeMagnitude);

                // originalPos is Vector2, so this is fine now
                barRect.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);
            }

            yield return null;
        }

        // restore position and color
        if (barRect != null)
            barRect.anchoredPosition = originalPos;

        if (fillImage != null)
            fillImage.color = normalColor;

        feedbackRoutine = null;
    }
}
