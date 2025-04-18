using TMPro;
using UnityEngine;

public class StatusUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI staminaText;

    private Stats localPlayerStats;

    void Update()
    {
        if (localPlayerStats == null)
        {
            TryFindLocalPlayer();
            return;
        }

        healthText.text = $"Health: {Mathf.CeilToInt(localPlayerStats.currentHealth)}";
        staminaText.text = $"Stamina: {Mathf.CeilToInt(localPlayerStats.currentStamina)}";
    }

    private void TryFindLocalPlayer()
    {
        foreach (var character in FindObjectsByType<Character>(FindObjectsSortMode.None))
        {
            if (character.IsOwner)
            {
                localPlayerStats = character.GetComponent<Stats>();
                break;
            }
        }
    }
}
