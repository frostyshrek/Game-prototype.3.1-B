using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyTelegraphUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text infoText;      // single line: "Static Wave (DUCK)"
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void Show(EnemyAttackPattern pattern, RequiredDodge dodge)
    {
        // icon
        if (pattern != null && iconImage != null)
        {
            iconImage.sprite = pattern.icon;
            iconImage.enabled = (pattern.icon != null);
        }

        // text: "AttackName (DODGE)"
        if (infoText != null)
        {
            string dodgeLabel = "";
            switch (dodge)
            {
                case RequiredDodge.Jump: dodgeLabel = "JUMP"; break;
                case RequiredDodge.Dash: dodgeLabel = "DASH"; break;
                case RequiredDodge.Duck: dodgeLabel = "DUCK"; break;
            }

            string attackName = pattern != null ? pattern.attackName : "";
            infoText.text = $"{attackName} ({dodgeLabel})";
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
