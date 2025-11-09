using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public Button creditsButton;
    public Button quitButton;

    void Start()
    {
        // Ensure time is running
        Time.timeScale = 1f;
        
        // Debug: Check if buttons are assigned
        if (playButton == null) Debug.LogError("MainMenuController: PlayButton is not assigned!");
        if (creditsButton == null) Debug.LogError("MainMenuController: CreditsButton is not assigned!");
        // Quit button removed - only on pause screen
        
        // Setup Play button
        if (playButton != null)
        {
            playButton.onClick.AddListener(() => {
                Debug.Log("Play button clicked!");
                if (GameManager.I != null)
                {
                    GameManager.I.StartNewGame();
                }
                else
                {
                    Debug.Log("Loading Game scene directly...");
                    SceneManager.LoadScene("Game");
                }
            });
        }
        else
        {
            Debug.LogError("Cannot setup Play button - button reference is null!");
        }
        
        // Setup Credits button
        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(() => {
                Debug.Log("Credits button clicked!");
                SceneManager.LoadScene("Credits");
            });
        }
        else
        {
            Debug.LogError("Cannot setup Credits button - button reference is null!");
        }
        
        // Quit button removed - only available on pause screen per requirements
    }
}

