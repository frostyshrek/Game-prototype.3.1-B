using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnergyUI : MonoBehaviour
{
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private Image energyFillImage;   // fillAmount 0–1
    [SerializeField] private TMP_Text energyText;     // optional

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
        if (energyFillImage != null)
            energyFillImage.fillAmount = (float)current / max;

        if (energyText != null)
            energyText.text = $"{current}/{max}";
    }
}
