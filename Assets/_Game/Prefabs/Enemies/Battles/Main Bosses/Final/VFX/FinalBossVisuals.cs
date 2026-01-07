using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using BattleSystem;   // for BattleCharacter, BattleManager, GameState etc

public class FinalBossVisuals : MonoBehaviour
{
    [Header("Refs")]
    public BattleCharacter bossCharacter;     // EnemyController inherits this
    public Light orbLight;
    public Transform orbVisual;               // the sphere mesh

    [Header("Hit Flicker")]
    public float hitScaleMultiplier = 1.25f;
    public float hitLightMultiplier = 1.5f;
    public float hitDuration = 0.15f;

    [Header("Death Sequence")]
    public float shrinkDuration = 1.2f;
    public CanvasGroup blackFadeCanvas;     // full-screen black image
    public float fadeDuration = 1.0f;
    public string sceneAfterDeath = "Glade";

    private float baseLightIntensity;
    private Vector3 baseScale;
    private bool deathStarted = false;
    private BattleManager battleManager;

    private void Awake()
    {
        if (bossCharacter == null)
            bossCharacter = GetComponent<BattleCharacter>();

        if (orbVisual == null)
            orbVisual = transform;

        if (orbLight != null)
            baseLightIntensity = orbLight.intensity;
        else
            baseLightIntensity = 1f;

        baseScale = orbVisual.localScale;
        battleManager = FindObjectOfType<BattleManager>();

        if (blackFadeCanvas != null)
        {
            blackFadeCanvas.gameObject.SetActive(false); // start hidden
            blackFadeCanvas.alpha = 0f;
        }
    }

    private void OnEnable()
    {
        if (bossCharacter != null)
            bossCharacter.OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        if (bossCharacter != null)
            bossCharacter.OnHealthChanged -= OnHealthChanged;
    }

    private int lastHealth = -1;
    private void OnHealthChanged(int current, int max)
    {
        if (lastHealth < 0) lastHealth = current;

        // took damage
        if (current < lastHealth && current > 0)
            StartCoroutine(HitFlicker());

        // died
        if (current <= 0 && !deathStarted)
        {
            deathStarted = true;
            StartCoroutine(DeathSequence());
        }

        lastHealth = current;
    }

    private IEnumerator HitFlicker()
    {
        float time = 0f;
        while (time < hitDuration)
        {
            time += Time.deltaTime;
            float n = time / hitDuration;

            float s = Mathf.Lerp(hitScaleMultiplier, 1f, n);
            orbVisual.localScale = baseScale * s;

            if (orbLight != null)
            {
                float intensity = Mathf.Lerp(baseLightIntensity * hitLightMultiplier,
                                             baseLightIntensity, n);
                orbLight.intensity = intensity;
            }

            yield return null;
        }

        orbVisual.localScale = baseScale;
        if (orbLight != null) orbLight.intensity = baseLightIntensity;
    }

    public IEnumerator DeathSequence()
    {
        // stop gameplay things
        if (battleManager != null)
        {
            if (battleManager.playerMovement != null)
                battleManager.playerMovement.SetCanMove(false);

            if (battleManager.enemyAttackController != null)
                battleManager.enemyAttackController.StopAttacks();
        }

        // 1) shrink orb
        Vector3 startScale = orbVisual.localScale;
        float time = 0f;
        while (time < shrinkDuration)
        {
            time += Time.deltaTime;
            float n = Mathf.Clamp01(time / shrinkDuration);

            orbVisual.localScale = Vector3.Lerp(startScale, Vector3.zero, n);

            // optional: dim light as well
            if (orbLight != null)
                orbLight.intensity = Mathf.Lerp(baseLightIntensity, 0f, n);

            yield return null;
        }

        // 2) fade screen to black
        if (blackFadeCanvas != null)
        {
            blackFadeCanvas.gameObject.SetActive(true);
            blackFadeCanvas.alpha = 0f;

            float tim = 0f;
            while (tim < fadeDuration)
            {
                tim += Time.deltaTime;
                float n = Mathf.Clamp01(tim / fadeDuration);
                blackFadeCanvas.alpha = n;
                yield return null;
            }

            blackFadeCanvas.alpha = 1f;
        }

        // 3) mark encounter defeated + load next scene
        if (GameState.I != null)
        {
            GameState.I.MarkEncounterDefeated(GameState.I.LastEncounterId);
            GameState.I.GiveKey(KeyItem.AncientKey); // or whatever reward
        }

        SceneManager.LoadScene(sceneAfterDeath);
    }
}
