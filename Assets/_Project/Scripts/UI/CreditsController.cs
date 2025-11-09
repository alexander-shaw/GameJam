using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CreditsController : MonoBehaviour
{
    [Header("UI References")]
    public Button backButton;
    public TextMeshProUGUI teamNameText;

    void Start()
    {
        // Set team name
        if (teamNameText != null)
        {
            teamNameText.text = "Garbage";
        }
        
        // Setup Back button
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => {
                Debug.Log("Credits: Back button clicked - returning to MainMenu");
                SceneManager.LoadScene("MainMenu");
            });
        }
        else
        {
            Debug.LogError("CreditsController: Back button is not assigned!");
        }
    }
}

