using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem
{
    public class CharacterController : MonoBehaviour
    {
        [Header("charactor info")]
        public int maxHealth = 100;
        public int currentHealth = 100;
        public bool isPlayer = false;

        [Header("UI")]
        public Slider healthBarSlider;
        public Text healthText;
        public GameObject damageTextPrefab;

        // [Header("视觉效果")]
        // public ParticleSystem damageEffect;
        // public ParticleSystem healEffect;

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
        public void TakeDamage(int damage, CardAttribute attribute = CardAttribute.None)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth); // make sure hp doesnt go below 0
            
            // update health ui
            UpdateHealthUI();
            
            // show dmg text
            // ShowDamageText(damage, attribute);
            
            // triger events
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
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
                healthBarSlider.value = currentHealth;
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