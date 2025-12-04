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
        if (telegraphUI != null)
            telegraphUI.Show(pattern, pattern.requiredDodge);

        yield return new WaitForSeconds(pattern.telegraphTime);

        // CHECK DODGE
        bool dodged = false;

        switch (pattern.requiredDodge)
        {
            case RequiredDodge.Jump:
                dodged = playerMovement != null && playerMovement.IsJumping;
                break;
            case RequiredDodge.Dash:
                dodged = playerMovement != null && playerMovement.IsDashing;
                break;
            case RequiredDodge.Duck:
                dodged = playerMovement != null && playerMovement.IsDucking;
                break;
        }

        if (telegraphUI != null)
            telegraphUI.Hide();

        if (dodged)
        {
            Debug.Log($"✅ Dodged {pattern.attackName}");

            if (refillEnergyOnDodge && playerEnergy != null)
            {
                playerEnergy.RefillFull();
            }
        }
        else
        {
            Debug.Log($"❌ Hit by {pattern.attackName}");

            if (playerCharacter != null)
            {
                playerCharacter.TakeDamage(pattern.damage, pattern.attribute);

                // if this hit kills the player, end the battle
                if (playerCharacter.IsDead() && battleManager != null)
                {
                    battleManager.GameOver(false);
                    yield break;
                }
            }
        }

        yield return new WaitForSeconds(pattern.recoverTime);
    }
}
