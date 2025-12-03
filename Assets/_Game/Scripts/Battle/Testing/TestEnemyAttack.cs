using UnityEngine;
using System.Collections;
using BattleSystem;

public class TestEnemyAttack : MonoBehaviour
{
    [Header("References")]
    public BattleOrbitMovement playerMovement;   // drag Player (with BattleOrbitMovement)
    public PlayerEnergy playerEnergy;            // drag Player (with PlayerEnergy)
    public BattleCharacter playerCharacter;      // drag Player (PlayerController inherits this)

    [Header("Timing")]
    public float timeBetweenAttacks = 3f;
    public float telegraphTime = 1.2f;

    [Header("Attack")]
    public int damageOnHit = 10;
    public bool refillEnergyOnSuccessfulDodge = true;

    private RequiredDodge requiredDodge = RequiredDodge.Jump;

    private void Start()
    {
        if (playerMovement == null)
        {
            Debug.LogError("TestEnemyAttack: PlayerMovement reference not set!");
            enabled = false;
            return;
        }

        if (playerCharacter == null)
        {
            Debug.LogWarning("TestEnemyAttack: playerCharacter not set – drag the Player here in the inspector.");
        }

        StartCoroutine(AttackLoop());
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            // wait before starting next attack
            yield return new WaitForSeconds(timeBetweenAttacks);

            // choose a dodge type (for now random)
            requiredDodge = (RequiredDodge)Random.Range(1, 4); // Jump/Dash/Duck

            // TELEGRAPH
            Debug.Log($"Enemy telegraphs attack! Required dodge: {requiredDodge}");

            // TODO: later show UI icon / animation here

            // wait telegraph duration
            yield return new WaitForSeconds(telegraphTime);

            // HIT FRAME – check if player dodged correctly
            bool dodged = false;

            switch (requiredDodge)
            {
                case RequiredDodge.Jump:
                    dodged = playerMovement.IsJumping;
                    break;

                case RequiredDodge.Dash:
                    dodged = playerMovement.IsDashing;
                    break;

                case RequiredDodge.Duck:
                    dodged = playerMovement.IsDucking;
                    break;
            }

            if (dodged)
            {
                Debug.Log("Dodged successfully!");

                if (refillEnergyOnSuccessfulDodge && playerEnergy != null)
                {
                    playerEnergy.RefillFull();
                    Debug.Log($"Energy refilled to {playerEnergy.CurrentEnergy}");
                }
            }
            else
            {
                Debug.Log("Player was HIT!");

                if (playerCharacter != null)
                {
                    // CardAttribute has a default value of None, so this overload call is valid
                    playerCharacter.TakeDamage(damageOnHit);
                }
            }
        }
    }
}
