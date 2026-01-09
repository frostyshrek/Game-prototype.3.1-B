using UnityEngine;
using BattleSystem;

public class PortalRuneCheck : MonoBehaviour, IInteractable
{
    [SerializeField] private bool requireAllFiveRunes = true;

    [Header("Optional (auto-found if empty)")]
    [SerializeField] private GladeFeedbackUI gladeFeedback;

    [Header("Teleport")]
    [Tooltip("Where the player will be teleported in Glade.")]
    [SerializeField] private Transform teleportTarget;

    [Tooltip("If empty, auto-finds by tag Player.")]
    [SerializeField] private Transform playerRoot;

    [Tooltip("If you use CharacterController, disable it while teleporting to avoid weird physics.")]
    [SerializeField] private bool handleCharacterController = true;

    [Header("Optional: Trigger Final Boss Battle After Teleport")]
    [Tooltip("If assigned, we'll set GameState.CurrentEncounter to this enemy so your battle trigger can use it.")]
    [SerializeField] private EnemyData finalBossEnemyData;

    [Tooltip("If true, instantly start battle after teleport (loads Battle scene).")]
    [SerializeField] private bool startBattleImmediately = false;

    [Tooltip("Battle scene name if startBattleImmediately is true.")]
    [SerializeField] private string battleSceneName = "Battle";

    private void Awake()
    {
        if (gladeFeedback == null)
            gladeFeedback = FindObjectOfType<GladeFeedbackUI>(true);

        if (playerRoot == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerRoot = p.transform;
        }
    }

    public void Interact()
    {
        if (gladeFeedback == null)
        {
            Debug.LogWarning("[PortalRuneCheck] No GladeFeedbackUI in scene.");
            return;
        }

        if (GameState.I == null)
        {
            gladeFeedback.Show("No GameState found.", FeedbackTypeGlade.Error);
            return;
        }

        // Locked
        if (requireAllFiveRunes && !GameState.I.HasAllRunes())
        {
            gladeFeedback.Show($"PORTAL LOCKED ({GameState.I.RuneCount}/5 RUNES)", FeedbackTypeGlade.Warning, 2.5f);

            foreach (GreatRune r in System.Enum.GetValues(typeof(GreatRune)))
            {
                if (r.ToString() == "None") continue;

                if (!GameState.I.HasRune(r))
                    gladeFeedback.Show($"Missing: {r}", FeedbackTypeGlade.Info, 2.5f);
            }
            return;
        }

        // Success
        gladeFeedback.Show("PORTAL ACTIVATED! All Great Runes collected.", FeedbackTypeGlade.Success, 2.5f);

        if (teleportTarget == null)
        {
            gladeFeedback.Show("Portal target not set.", FeedbackTypeGlade.Error, 2.5f);
            return;
        }

        if (playerRoot == null)
        {
            gladeFeedback.Show("Player not found (tag Player).", FeedbackTypeGlade.Error, 2.5f);
            return;
        }

        // Optional: set the final boss encounter now so any trigger can read it
        if (finalBossEnemyData != null)
            GameState.I.SetCurrentEncounter(finalBossEnemyData);

        TeleportPlayer(playerRoot, teleportTarget.position, teleportTarget.rotation);

        // Optional: instantly start battle scene
        if (startBattleImmediately)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(battleSceneName);
        }
    }

    private void TeleportPlayer(Transform player, Vector3 pos, Quaternion rot)
    {
        CharacterController cc = null;

        if (handleCharacterController)
            cc = player.GetComponent<CharacterController>() ?? player.GetComponentInChildren<CharacterController>();

        if (cc != null) cc.enabled = false;

        player.SetPositionAndRotation(pos, rot);

        if (cc != null) cc.enabled = true;
    }
}
