using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CastButtonUI : MonoBehaviour
{
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;

    private Coroutine flashRoutine;

    private void Reset()
    {
        // auto-assign if this script is placed on the Button object
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
    }

    public void FlashError()
    {
        if (buttonImage == null) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        buttonImage.color = errorColor;
        yield return new WaitForSeconds(flashDuration);
        buttonImage.color = normalColor;
        flashRoutine = null;
    }
}
