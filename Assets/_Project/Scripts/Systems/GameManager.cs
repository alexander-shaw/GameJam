using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }
    
    [Header("Stability")]
    public float maxStability = 100f;
    
    [Header("Game State")]
    public bool isGameOver = false;
    public bool isPaused = false;
    
    [Header("Scoring System")]
    public int pointsPerKill = 10; // Points awarded per enemy killed
    
    // Current values
    float currentStability;
    int score;
    int enemiesKilled;
    
    // Public properties for accessing game state
    public int EnemiesKilled => enemiesKilled;
    public float CurrentStability => currentStability;
    
    // Events for UI updates
    public event Action<float, float> OnStabilityChanged; // current, max
    public event Action<int> OnScoreChanged;
    public event Action OnGameOver;
    public event Action OnGameRestart;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap() => new GameObject("GameManager").AddComponent<GameManager>();

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (isGameOver || isPaused) return;
        
        // Update stability UI (every frame for smooth updates)
        OnStabilityChanged?.Invoke(currentStability, maxStability);
        
        // Check game over (exactly 0, not less than)
        if (currentStability == 0f && !isGameOver)
        {
            GameOver();
        }
    }

    void InitializeGame()
    {
        currentStability = maxStability;
        score = 0;
        enemiesKilled = 0;
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;
        
        // Initialize UI
        OnStabilityChanged?.Invoke(currentStability, maxStability);
        OnScoreChanged?.Invoke(score);
    }

    public void RestoreStability(float amount)
    {
        currentStability = Mathf.Min(maxStability, currentStability + amount);
        OnStabilityChanged?.Invoke(currentStability, maxStability);
    }

    public void TakeDamage(float amount)
    {
        currentStability -= amount;
        currentStability = Mathf.Max(0f, currentStability);
        OnStabilityChanged?.Invoke(currentStability, maxStability);
    }

    public void OnEnemyKilled()
    {
        enemiesKilled++;
        score = enemiesKilled * pointsPerKill;
        OnScoreChanged?.Invoke(score);
    }

    void GameOver()
    {
        if (isGameOver) return; // Prevent multiple calls
        
        isGameOver = true;
        
        // Immediately return to main menu (no delay, no panel)
        OnGameOver?.Invoke();
        StartCoroutine(ReturnToMenuAfterDelay(0.1f)); // Brief delay to ensure UI updates
    }
    
    IEnumerator ReturnToMenuAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        ToMenu();
    }

    public void RestartGame()
    {
        OnGameRestart?.Invoke();
        Time.timeScale = 1f; // Ensure time is running
        ToMenu(); // Go back to main menu instead of restarting scene
    }
    
    public void StartNewGame()
    {
        InitializeGame();
        Time.timeScale = 1f;
        isGameOver = false;
        isPaused = false;
        
        // Re-enable player (in case it was disabled from previous game)
        // This will happen when Game scene loads, but good to ensure here
        Play();
    }

    public void Play() => SceneManager.LoadScene("Game");
    public void ToMenu() => SceneManager.LoadScene("MainMenu");
}



