using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public Button pauseButton; // Visible pause button in game
    public Button resumeButton;
    public Button quitButton; // Quit to menu button

    void Update()
    {
        // Toggle pause with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void Start()
    {
        // Debug: Check button assignments
        if (pauseButton == null) Debug.LogError("PauseController: PauseButton is not assigned!");
        if (resumeButton == null) Debug.LogError("PauseController: ResumeButton is not assigned!");
        if (quitButton == null) Debug.LogError("PauseController: QuitButton is not assigned!");
        if (pausePanel == null) Debug.LogError("PauseController: PausePanel is not assigned!");
        
        // Setup visible pause button
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(() => {
                Debug.Log("Pause button clicked!");
                TogglePause();
            });
        }
        
        // Setup resume button
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(() => {
                Debug.Log("Resume button clicked!");
                Resume();
            });
        }
        
        // Setup quit to menu button
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => {
                Debug.Log("Quit to menu button clicked!");
                if (GameManager.I != null)
                {
                    GameManager.I.isPaused = false;
                    Time.timeScale = 1f;
                    GameManager.I.ToMenu();
                }
                else
                {
                    SceneManager.LoadScene("MainMenu");
                }
            });
        }
        
        // Hide pause panel initially
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    void TogglePause()
    {
        if (GameManager.I == null) return;
        
        if (GameManager.I.isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    void Pause()
    {
        if (GameManager.I != null)
        {
            GameManager.I.isPaused = true;
        }
        Time.timeScale = 0f;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    void Resume()
    {
        if (GameManager.I != null)
        {
            GameManager.I.isPaused = false;
        }
        Time.timeScale = 1f;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }
}

