using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnergyBarUI : MonoBehaviour
{
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private Slider energySlider;
    [SerializeField] private TMP_Text energyText;  // optional

    private void OnEnable()
    {
        if (playerEnergy != null)
            playerEnergy.OnEnergyChanged += HandleEnergyChanged;
    }

    private void OnDisable()
    {
        if (playerEnergy != null)
            playerEnergy.OnEnergyChanged -= HandleEnergyChanged;
    }

    private void Start()
    {
        if (playerEnergy != null)
            HandleEnergyChanged(playerEnergy.CurrentEnergy, playerEnergy.maxEnergy);
    }

    private void HandleEnergyChanged(int current, int max)
    {
        if (energySlider != null)
        {
            energySlider.maxValue = max;

            var smooth = energySlider.GetComponent<SmoothSlider>();
            if (smooth != null)
            {
                smooth.SetTarget(current);
            }
            else
            {
                energySlider.value = current;
            }
        }

        if (energyText != null)
        {
            energyText.text = $"{current}/{max}";
        }
    }
}
