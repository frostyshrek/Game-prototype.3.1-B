using UnityEngine;

public class BattleSFX : MonoBehaviour
{
    [SerializeField] private AudioSource source;

    [Header("Clips")]
    [SerializeField] private AudioClip superEffective;
    [SerializeField] private AudioClip notEffective;
    [SerializeField] private AudioClip hit;
    [SerializeField] private AudioClip parryPattern;
    [SerializeField] private AudioClip dashPattern;
    [SerializeField] private AudioClip jumpPattern;
    [SerializeField] private AudioClip dodgeSuccessful;
    [SerializeField] private AudioClip winGameOver;
    [SerializeField] private AudioClip loseGameOver;

    private void Awake()
    {
        if (source == null)
            source = GetComponent<AudioSource>();

        if (source == null)
        {
            Debug.LogWarning("[BattleSFX] No AudioSource found.");
            return;
        }

        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    private void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null || source == null) return;
        source.PlayOneShot(clip, volume);
    }

    // ---------- PUBLIC API ----------

    public void PlaySuperEffective()    => Play(superEffective, 1f);
    public void PlayNotEffective()      => Play(notEffective, 0.9f);
    public void PlayHit()               => Play(hit, 0.9f);

    public void PlayParry()             => Play(parryPattern, 1f);
    public void PlayDash()              => Play(dashPattern, 0.85f);
    public void PlayJump()              => Play(jumpPattern, 0.85f);

    public void PlayDodgeSuccess()      => Play(dodgeSuccessful, 1f);

    public void PlayWinGameOver()        => Play(winGameOver, 1f);
    public void PlayLoseGameOver()       => Play(loseGameOver, 1f);
}
