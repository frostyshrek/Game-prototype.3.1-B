using UnityEngine;

public class FinalBossArenaLighting : MonoBehaviour
{
    [Header("Final Boss Light Settings")]
    public Color bossLightColor = new Color32(0xFF, 0xD6, 0x60, 0xFF); // #FFD660
    public float forcedIntensity = 2f;
    public float forcedRange = 20f;

    private void Start()
    {
        ApplyBossLighting();
    }

    private void ApplyBossLighting()
    {
        Light[] allLights = FindObjectsOfType<Light>(true);

        foreach (var light in allLights)
        {
            // We no longer disable lights — we reskin them for final-boss mode
            light.enabled = true;
            light.color = bossLightColor;

            // Optional, but usually needed to make it look good:
            light.intensity = forcedIntensity;
            light.range = forcedRange;
        }
    }
}
