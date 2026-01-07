using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleSystem
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("Root Overlay (parent of both panels)")]
        [SerializeField] private GameObject overlayRoot;

        [Header("Panels")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject deathPanel;

        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 0.8f;

        private bool isShowing = false;
        private CanvasGroup overlayGroup;

        private void Awake()
        {
            // If overlayRoot isn't set, assume this object is the root
            if (overlayRoot == null)
                overlayRoot = gameObject;

            // Ensure CanvasGroup exists on root
            overlayGroup = overlayRoot.GetComponent<CanvasGroup>();
            if (overlayGroup == null)
                overlayGroup = overlayRoot.AddComponent<CanvasGroup>();

            // Start fully hidden
            overlayRoot.SetActive(false);        // root starts OFF
            overlayGroup.alpha = 0f;
            overlayGroup.interactable = false;
            overlayGroup.blocksRaycasts = false;

            if (winPanel != null)   winPanel.SetActive(false);
            if (deathPanel != null) deathPanel.SetActive(false);
        }

        public void ShowWin()
        {
            Debug.Log("[GameOverUI] ShowWin called");
            if (isShowing) return;
            isShowing = true;

            // Show the root + ONLY win panel
            overlayRoot.SetActive(true);
            if (winPanel != null)   winPanel.SetActive(true);
            if (deathPanel != null) deathPanel.SetActive(false);

            StartCoroutine(FadeInOverlay());
        }

        public void ShowDeath()
        {
            Debug.Log("[GameOverUI] ShowDeath called");
            if (isShowing) return;
            isShowing = true;

            // Show the root + ONLY death panel
            overlayRoot.SetActive(true);
            if (winPanel != null)   winPanel.SetActive(false);
            if (deathPanel != null) deathPanel.SetActive(true);

            StartCoroutine(FadeInOverlay());
        }

        private System.Collections.IEnumerator FadeInOverlay()
        {
            if (overlayGroup == null) yield break;

            overlayGroup.alpha = 0f;
            overlayGroup.interactable = false;
            overlayGroup.blocksRaycasts = false;

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;           // works even if you pause time later
                float normalized = Mathf.Clamp01(t / fadeDuration);
                overlayGroup.alpha = normalized;
                yield return null;
            }

            overlayGroup.alpha = 1f;
            overlayGroup.interactable = true;
            overlayGroup.blocksRaycasts = true;
        }

        // Your button callback
        public void OnContinueButtonPressed()
        {
            Debug.Log("[GameOverUI] Continue pressed → loading Glade");
            Time.timeScale = 1f;  // just in case
            SceneManager.LoadScene("Glade");
        }
    }
}
