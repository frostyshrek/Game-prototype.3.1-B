using UnityEngine;
using UnityEngine.UI;

public class SmoothSlider : MonoBehaviour
{
    public Slider slider;
    [SerializeField] private float lerpSpeed = 10f;

    private float targetValue;

    private void Reset()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
    }

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        targetValue = slider != null ? slider.value : 0f;
    }

    private void Update()
    {
        if (slider == null) return;

        if (!Mathf.Approximately(slider.value, targetValue))
        {
            slider.value = Mathf.Lerp(slider.value, targetValue, lerpSpeed * Time.deltaTime);
        }
    }

    public void SetTarget(float value)
    {
        targetValue = value;
    }
}
