using UnityEngine;

[DisallowMultipleComponent]
public class ChestHighlightable : MonoBehaviour, IHighlightable
{
    [Header("Renderers to highlight (auto if empty)")]
    public Renderer[] renderers;

    [Header("Emission")]
    public Color highlightColor = new Color(0.25f, 0.9f, 1f, 1f);
    public float intensity = 2.5f;

    // keep instances so it don't mutate shared materials
    MaterialPropertyBlock mpb;
    bool highlighted;

    // Some shaders use _EmissionColor, some need keyword enabled
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);

        mpb = new MaterialPropertyBlock();
        SetHighlighted(false); // ensure off at start
    }

    public void SetHighlighted(bool on)
    {
        if (highlighted == on) return;
        highlighted = on;

        foreach (var r in renderers)
        {
            if (!r) continue;

            // Read current property block so we don't clobber other values
            r.GetPropertyBlock(mpb);

            if (on)
            {
                // Turn on emission
                var c = highlightColor * Mathf.LinearToGammaSpace(intensity);
                mpb.SetColor(EmissionColorID, c);
                EnableEmissionKeyword(r, true);
            }
            else
            {
                // Set emission to black (off)
                mpb.SetColor(EmissionColorID, Color.black);
                EnableEmissionKeyword(r, false);
            }

            r.SetPropertyBlock(mpb);
        }
    }

    static void EnableEmissionKeyword(Renderer r, bool enable)
    {
        // Toggle keyword on all materials used by this renderer
        var mats = r.sharedMaterials;
        for (int i = 0; i < mats.Length; i++)
        {
            if (!mats[i]) continue;
            if (enable) mats[i].EnableKeyword("_EMISSION");
            else mats[i].DisableKeyword("_EMISSION");
        }
    }
}
