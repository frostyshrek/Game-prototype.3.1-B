using UnityEngine;
using System.Collections;
using BattleSystem;   // for BattleCharacter, CardAttribute

public class EnemyAttackController : MonoBehaviour
{
    [Header("References (auto-wired by BattleManager)")]
    [HideInInspector] public BattleOrbitMovement playerMovement;
    [HideInInspector] public PlayerEnergy playerEnergy;
    [HideInInspector] public BattleCharacter playerCharacter;
    [HideInInspector] public BattleManager battleManager;
    [HideInInspector] public BattleFeedbackUI feedbackUI;
    [HideInInspector] public BattleSFX battleSFX;


    [Header("Patterns")]
    public EnemyAttackPattern[] attackPatterns;

    [Header("UI (auto-wired by BattleManager)")]
    [HideInInspector] public EnemyTelegraphUI telegraphUI;

    [Header("Behaviour")]
    public bool autoStart = true;
    public bool refillEnergyOnDodge = true;

    [Header("Attack Timing")]
    [Tooltip("Base minimum time between attacks, in seconds, before speed multiplier.")]
    public float baseMinInterval = 2f;
    [Tooltip("Base maximum time between attacks, in seconds, before speed multiplier.")]
    public float baseMaxInterval = 4f;
    [Tooltip("Attack speed multiplier. 1 = normal, 2 = twice as fast, 0.5 = half speed.")]
    public float speedMultiplier = 1f;

    private bool isRunning;
    private Coroutine loopCoroutine;

    public Animator animator;

    private void Start()
    {
        if (autoStart)
        {
            BeginAttacks();
        }
    }

    // called by BattleManager at battle start if you don't want autoStart
    public void BeginAttacks()
    {
        if (isRunning) return;
        isRunning = true;
        loopCoroutine = StartCoroutine(AttackLoop());
    }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    // called by BattleManager when battle ends
    public void StopAttacks()
    {
        isRunning = false;
        if (loopCoroutine != null)
            StopCoroutine(loopCoroutine);
        loopCoroutine = null;

        if (telegraphUI != null)
            telegraphUI.Hide();
    }

    private IEnumerator AttackLoop()
    {
        while (isRunning)
        {
            if (playerCharacter != null && playerCharacter.IsDead())
            {
                // stop if player is already dead
                yield break;
            }

            if (attackPatterns == null || attackPatterns.Length == 0)
            {
                yield return null;
                continue;
            }

            // random delay BEFORE the next attack
            float interval = Random.Range(baseMinInterval, baseMaxInterval);
            float speed = Mathf.Max(0.1f, speedMultiplier);
            interval /= speed; // faster speed -> shorter interval

            yield return new WaitForSeconds(interval);

            // execute one attack pattern
            EnemyAttackPattern pattern = attackPatterns[Random.Range(0, attackPatterns.Length)];
            yield return StartCoroutine(ExecuteAttackPattern(pattern));
        }
    }

    private IEnumerator ExecuteAttackPattern(EnemyAttackPattern pattern)
    {
        if (!isRunning || pattern == null)
            yield break;

        // TELEGRAPH
        Debug.Log($"Enemy telegraphs: {pattern.attackName} (need {pattern.requiredDodge})");

        if (pattern.telegraphVFXPrefab != null)
            Instantiate(pattern.telegraphVFXPrefab, transform.position, Quaternion.identity);

        if (telegraphUI != null)
            telegraphUI.Show(pattern, pattern.requiredDodge);

        yield return new WaitForSeconds(pattern.telegraphTime);

        if (animator != null)
            animator.SetTrigger("Attack");

        // CHECK DODGE
        bool dodged = false;

        switch (pattern.requiredDodge)
        {
            case RequiredDodge.Jump:
                dodged = playerMovement != null && playerMovement.IsJumping;
                battleSFX?.PlayJump();
                break;

            case RequiredDodge.DashLeft:
                dodged = playerMovement != null &&
                        playerMovement.IsDashing &&
                        playerMovement.LastDashDirection == 1;   // 1 = left (A)
                battleSFX?.PlayDash();
                break;

            case RequiredDodge.DashRight:
                dodged = playerMovement != null &&
                        playerMovement.IsDashing &&
                        playerMovement.LastDashDirection == -1;  // -1 = right (D)
                battleSFX?.PlayDash();
                break;

            case RequiredDodge.Parry:
                dodged = playerMovement != null && playerMovement.IsParrying;
                battleSFX?.PlayParry();
                break;

            default:
                dodged = false;
                break;
        }

        if (telegraphUI != null)
            telegraphUI.Hide();

        if (dodged)
        {
            battleSFX?.PlayDodgeSuccess();

            // Feedback message
            if (feedbackUI != null)
            {
                switch (pattern.requiredDodge)
                {
                    case RequiredDodge.Parry:
                        feedbackUI.Show("PARRIED!", FeedbackType.Success, 1.2f);
                        break;

                    case RequiredDodge.DashLeft:
                    case RequiredDodge.DashRight:
                        feedbackUI.Show("DODGED!", FeedbackType.Success, 1.0f);
                        break;

                    case RequiredDodge.Jump:
                        feedbackUI.Show("EVADED!", FeedbackType.Success, 1.0f);
                        break;
                }
            }

            if (refillEnergyOnDodge && playerEnergy != null)
                playerEnergy.RefillFull();
        }
        else
        {
            feedbackUI?.Show("HIT!", FeedbackType.Warning, 0.9f);
            battleSFX?.PlayHit();

            if (playerCharacter != null)
            {
                playerCharacter.TakeDamage(pattern.damage, pattern.attribute);

                if (playerCharacter.IsDead() && battleManager != null)
                {
                    battleManager.GameOver(false);
                    yield break;
                }
            }

            if (pattern.hitVFXPrefab != null)
                Instantiate(pattern.hitVFXPrefab, playerCharacter.transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(pattern.recoverTime);
    }

}
