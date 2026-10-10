using UnityEngine;
using UnityEngine.UI;

public class TankHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int _currentHealth;

    [Header("UI")]
    public Image healthFillImage;
    public GameObject GameOverPanel;

    [HideInInspector] public bool godMode = false;

    private void Start()
    {
        _currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void takeDamage(int damage)
    {
        if (godMode) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);
        UpdateHealthBar();

        if (_currentHealth <= 0)
        {

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.TriggerDefeat();
            }
            else if (GameOverPanel != null)
            {
                GameOverPanel.SetActive(true);
            }

            Destroy(gameObject);
        }
    }

    private void UpdateHealthBar()
    {
        if (healthFillImage != null)
            healthFillImage.fillAmount = (float)_currentHealth / maxHealth;
    }
}
