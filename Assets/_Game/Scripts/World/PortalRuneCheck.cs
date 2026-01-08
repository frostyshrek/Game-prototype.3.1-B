using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalRuneCheck : MonoBehaviour, IInteractable
{
    [SerializeField] private bool requireAllFiveRunes = true;

    [Header("Optional (auto-found if empty)")]
    [SerializeField] private GladeFeedbackUI gladeFeedback;

    [Header("Optional: Load a scene when portal works")]
    [SerializeField] private string sceneToLoad = ""; // e.g. "FinalBoss" (leave empty to do nothing)

    private void Awake()
    {
        // Auto-find if not assigned
        if (gladeFeedback == null)
            gladeFeedback = FindObjectOfType<GladeFeedbackUI>(true);
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

        // If locked
        if (requireAllFiveRunes && !GameState.I.HasAllRunes())
        {
            gladeFeedback.Show($"PORTAL LOCKED ({GameState.I.RuneCount}/5 RUNES)", FeedbackTypeGlade.Warning, 2.5f);

            foreach (GreatRune r in System.Enum.GetValues(typeof(GreatRune)))
            {
                // Skip None if you have it
                if (r.ToString() == "None") continue;

                if (!GameState.I.HasRune(r))
                    gladeFeedback.Show($"Missing: {r}", FeedbackTypeGlade.Info, 2.5f);
            }
            return;
        }

        // Success
        gladeFeedback.Show("PORTAL ACTIVATED! All Great Runes collected.", FeedbackTypeGlade.Success, 2.5f);

        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }
}
