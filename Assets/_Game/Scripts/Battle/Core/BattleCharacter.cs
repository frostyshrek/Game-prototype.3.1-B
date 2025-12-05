using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BattleSystem
{
    public class BattleCharacter : MonoBehaviour
    {
        [Header("charactor info")]
        public int maxHealth = 100;
        public int currentHealth = 100;
        public bool isPlayer = false;

        [Header("UI")]
        public Slider healthBarSlider;
        public TMP_Text healthText;
        public GameObject damageTextPrefab;

        [Header("Animation")]
        public Animator animator;
        public string hitTriggerName = "Hit";
        public string dieTriggerName = "Die";
        public string isDeadBoolName = "IsDead";

        // trigers events when health changes
        public System.Action<int, int> OnHealthChanged;

        void Start()
        {
            InitializeCharacter();
        }

        // InitializeCharacter
        public void InitializeCharacter()
        {
            currentHealth = maxHealth;
            UpdateHealthUI();
        }

        // apply damage
        public void TakeDamage(int damage, CardAttribute attribute = CardAttribute.Physical)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth); // make sure hp doesnt go below 0
            
            // update health ui
            UpdateHealthUI();
            
            // show dmg text
            // ShowDamageText(damage, attribute);
            
            // triger events
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (animator != null)
            {
                if (currentHealth <= 0)
                {
                    if (!string.IsNullOrEmpty(isDeadBoolName))
                        animator.SetBool(isDeadBoolName, true);

                    if (!string.IsNullOrEmpty(dieTriggerName))
                        animator.SetTrigger(dieTriggerName);
                }
                else if (damage > 0)
                {
                    if (!string.IsNullOrEmpty(hitTriggerName))
                        animator.SetTrigger(hitTriggerName);
                }
            }
            
            Debug.Log($"{gameObject.name} suffers {damage} damage, current hp: {currentHealth}");
        }

        // apply heal
        public void Heal(int healAmount)
        {
            currentHealth += healAmount;
            currentHealth = Mathf.Min(currentHealth, maxHealth); // make sure hp doesnt go above max hp
            
            // update ui
            UpdateHealthUI();
            
            // triger event
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            Debug.Log($"{gameObject.name} heals {healAmount} hp, current hp: {currentHealth}");
        }

        // update health ui
        void UpdateHealthUI()
        {
            if (healthBarSlider != null)
            {
                healthBarSlider.maxValue = maxHealth;

                var smooth = healthBarSlider.GetComponent<SmoothSlider>();
                if (smooth != null)
                {
                    smooth.SetTarget(currentHealth);
                }
                else
                {
                    healthBarSlider.value = currentHealth;
                }
            }
                    
            if (healthText != null)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
            }
        }

        // check is dead
        public bool IsDead()
        {
            return currentHealth <= 0;
        }
    }
}