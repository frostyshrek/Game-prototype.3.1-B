using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleSystem
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("Root Overlay")]
        [SerializeField] private GameObject overlayRoot;

        [Header("Panels")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject deathPanel;

        private bool isShowing = false;

        private void Awake()
        {
            // If overlayRoot isn't set, assume this object *is* the root
            if (overlayRoot == null)
                overlayRoot = gameObject;

            if (overlayRoot != null)
                overlayRoot.SetActive(false);

            if (winPanel != null)
                winPanel.SetActive(false);

            if (deathPanel != null)
                deathPanel.SetActive(false);
        }

        public void ShowWin()
        {
            Debug.Log("[GameOverUI] ShowWin called");
            if (isShowing) return;
            isShowing = true;

            if (overlayRoot != null) overlayRoot.SetActive(true);
            if (winPanel != null)    winPanel.SetActive(true);
            if (deathPanel != null)  deathPanel.SetActive(false);
        }

        public void ShowDeath()
        {
            Debug.Log("[GameOverUI] ShowDeath called");
            if (isShowing) return;
            isShowing = true;

            if (overlayRoot != null) overlayRoot.SetActive(true);
            if (winPanel != null)    winPanel.SetActive(false);
            if (deathPanel != null)  deathPanel.SetActive(true);
        }

        // Called by both Win + Death button
        public void OnContinueButtonPressed()
        {
            Debug.Log("[GameOverUI] Continue pressed → loading Glade");
            SceneManager.LoadScene("Glade");
        }
    }
}
