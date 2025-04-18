using UnityEngine;

public class Stats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 2f;    // per second
    public float dashStaminaCost = 25f;

    void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    void Update()
    {
        RegenerateStamina();
    }

    public bool CanDash()
    {
        return currentStamina >= dashStaminaCost;
    }

    public void UseStaminaForDash()
    {
        if (CanDash())
        {
            currentStamina -= dashStaminaCost;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }
    }

    private void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
    }
}
