using UnityEngine;
using TMPro;

public class EnemyTelegraphUI : MonoBehaviour
{
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void Show(EnemyAttackPattern pattern, RequiredDodge dodge)
    {
        if (infoText != null)
        {
            string dodgeLabel = dodge switch
            {
                RequiredDodge.Jump => "JUMP",
                RequiredDodge.DashLeft => "DASH LEFT",
                RequiredDodge.DashRight => "DASH RIGHT",
                RequiredDodge.Parry => "PARRY",
                _ => "MOVE"
            };

            string attackName = pattern != null ? pattern.attackName.ToUpper() : "INCOMING";
            infoText.text = $"{attackName} — {dodgeLabel}";
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
}
