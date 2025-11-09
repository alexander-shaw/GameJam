using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI statusText; // Combined "score: NUM | health: NUM" text
    public GameObject gameOverPanel;
    public Button restartButton;

    void Start()
    {
        // Subscribe to GameManager events
        if (GameManager.I != null)
        {
            GameManager.I.OnStabilityChanged += UpdateStatus;
            GameManager.I.OnScoreChanged += UpdateStatus;
            GameManager.I.OnGameOver += ShowGameOver;
            GameManager.I.OnGameRestart += HideGameOver;
            
            // Initialize with current values from GameManager
            UpdateStatus(GameManager.I.CurrentStability, GameManager.I.maxStability);
            UpdateStatus(0);
        }
        
        // Setup restart button (goes back to main menu)
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() => {
                if (GameManager.I != null)
                {
                    GameManager.I.RestartGame(); // This now goes to main menu
                }
            });
        }
        
        // Hide game over panel initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.I != null)
        {
            GameManager.I.OnStabilityChanged -= UpdateStatus;
            GameManager.I.OnScoreChanged -= UpdateStatus;
            GameManager.I.OnGameOver -= ShowGameOver;
            GameManager.I.OnGameRestart -= HideGameOver;
        }
    }

    float currentHealth = 100f;
    float maxHealth = 100f;
    int currentScore = 0;

    void UpdateStatus(float health, float max)
    {
        currentHealth = health;
        maxHealth = max;
        UpdateStatusText();
    }

    void UpdateStatus(int score)
    {
        currentScore = score;
        UpdateStatusText();
    }

    void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = $"score: {currentScore} | health: {Mathf.RoundToInt(currentHealth)}";
            
            // Color code based on health percentage
            float healthPercent = maxHealth > 0 ? currentHealth / maxHealth : 0f;
            if (healthPercent > 0.6f)
            {
                statusText.color = Color.green; // Healthy
            }
            else if (healthPercent > 0.3f)
            {
                statusText.color = Color.yellow; // Warning
            }
            else
            {
                statusText.color = Color.red; // Critical
            }
        }
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
}

