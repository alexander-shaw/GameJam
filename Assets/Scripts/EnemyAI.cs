using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour {
    [Header("Settings")]
    public float speed = 3f;
    public float health = 2f;
    public float damage = 15f;
    public float chaseRange = 5f;

    [Header("Drops")]
    public GameObject timeCrystalPrefab;
    
    [Header("Scene Transition")]
    public string battleSceneName = "BattleScene";
    public string bossBattleSceneName = "BossBattleScene";
    public float fadeDuration = 0.5f;

    Rigidbody2D rb;
    Transform player;
    float currentHealth;
    bool isTransitioning = false;
    SpriteRenderer spriteRenderer;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = health;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start() {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) {
            player = playerObj.transform;
        }
    }

    void FixedUpdate() {
        if (player != null) {
            Vector2 toPlayer = (player.position - transform.position);
            float sqrDist = toPlayer.sqrMagnitude;
            float sqrRange = chaseRange * chaseRange;

            if (sqrDist <= sqrRange) {
                Vector2 direction = toPlayer.normalized;
                rb.linearVelocity = direction * speed;
            } else {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player") && !isTransitioning) {
            // Store the enemy sprite for battle scene
            if (spriteRenderer != null && spriteRenderer.sprite != null) {
                BattleData.SetPrimaryEnemy(spriteRenderer.sprite);
                Debug.Log($"EnemyAI: Storing enemy sprite '{spriteRenderer.sprite.name}' for battle");
                
                // Store this enemy's name so it can be removed when returning to WorldScene
                BattleData.SetEnemyToRemove(gameObject.name);
                
                // Check if this is a CyberDragon (boss battle)
                string spriteName = spriteRenderer.sprite.name.ToLower();
                if (spriteName.Contains("cyberdragon") || spriteName.Contains("cyber-dragon")) {
                    BattleData.isBossBattle = true;
                    Debug.Log("EnemyAI: CyberDragon detected! Routing to boss battle scene.");
                    isTransitioning = true;
                    StartCoroutine(TransitionToBattleScene(bossBattleSceneName));
                    return;
                } else {
                    BattleData.isBossBattle = false;
                }
            }
            
            isTransitioning = true;
            StartCoroutine(TransitionToBattleScene(battleSceneName));
        }
    }
    
    IEnumerator TransitionToBattleScene(string sceneName) {
        // Stop enemy movement
        if (rb != null) {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Find existing ScreenFade in scene, or create one
        ScreenFade fader = FindFirstObjectByType<ScreenFade>();
        
        if (fader == null || fader.canvasGroup == null) {
            // Create a ScreenFade overlay if one doesn't exist
            GameObject fadeObject = new GameObject("BattleTransitionFade");
            Canvas canvas = fadeObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            
            CanvasGroup canvasGroup = fadeObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            
            GameObject imageObject = new GameObject("FadeImage");
            imageObject.transform.SetParent(fadeObject.transform, false);
            UnityEngine.UI.Image image = imageObject.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.black;
            
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            fader = fadeObject.AddComponent<ScreenFade>();
            fader.canvasGroup = canvasGroup;
            fader.fadeDuration = fadeDuration;
        }
        
        // Start loading the battle scene asynchronously
        var async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;
        
        // Wait for scene to load
        while (async.progress < 0.9f)
            yield return null;
        
        // Fade out current scene
        yield return fader.StartCoroutine(fader.FadeOut());
        
        // Create persistent fade overlay for the new scene
        GameObject persistentFadeObject = new GameObject("PersistentBattleFade");
        DontDestroyOnLoad(persistentFadeObject);
        
        Canvas persistentCanvas = persistentFadeObject.AddComponent<Canvas>();
        persistentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        persistentCanvas.sortingOrder = 9999;
        
        CanvasGroup persistentCanvasGroup = persistentFadeObject.AddComponent<CanvasGroup>();
        persistentCanvasGroup.alpha = 1f;
        persistentCanvasGroup.blocksRaycasts = true;
        
        GameObject persistentImageObject = new GameObject("FadeImage");
        persistentImageObject.transform.SetParent(persistentFadeObject.transform, false);
        UnityEngine.UI.Image persistentImage = persistentImageObject.AddComponent<UnityEngine.UI.Image>();
        persistentImage.color = Color.black;
        
        RectTransform persistentRectTransform = persistentImageObject.GetComponent<RectTransform>();
        persistentRectTransform.anchorMin = Vector2.zero;
        persistentRectTransform.anchorMax = Vector2.one;
        persistentRectTransform.sizeDelta = Vector2.zero;
        persistentRectTransform.anchoredPosition = Vector2.zero;
        
        ScreenFade persistentFader = persistentFadeObject.AddComponent<ScreenFade>();
        persistentFader.canvasGroup = persistentCanvasGroup;
        persistentFader.fadeDuration = fadeDuration;
        
        // Activate the battle scene
        async.allowSceneActivation = true;
        
        // Wait for scene to fully load
        yield return null;
        yield return null;
        
        // Fade in the battle scene
        yield return persistentFader.StartCoroutine(persistentFader.FadeIn());
        
        // Clean up the persistent fade object
        Destroy(persistentFadeObject);
    }
}