using UnityEngine;

public class GladeSFX : MonoBehaviour
{
    [SerializeField] private AudioSource source;

    [Header("Clips")]
    [SerializeField] private AudioClip footstep;
    [SerializeField] private AudioClip interact;
    [SerializeField] private AudioClip pickup;
    [SerializeField] private AudioClip portal;
    [SerializeField] private AudioClip encounter;

    private void Awake()
    {
        if (source == null) source = GetComponent<AudioSource>();

        if (source == null)
        {
            Debug.LogWarning("[GladeSFX] No AudioSource found.");
            return;
        }

        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D
    }

    private void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null || source == null) return;
        source.PlayOneShot(clip, volume);
    }

    public void PlayFootstep()  => Play(footstep, 0.4f);
    public void PlayInteract()  => Play(interact, 0.9f);
    public void PlayPickup()    => Play(pickup, 1f);
    public void PlayPortal()    => Play(portal, 1f);
    public void PlayEncounter() => Play(encounter, 1f);
}
