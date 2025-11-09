using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameSetup : EditorWindow
{
    /// <summary>
    /// Get the custom font asset from Assets/_Project/Fonts/Font.ttf
    /// Returns null if font not found
    /// </summary>
    static TMP_FontAsset GetCustomFont()
    {
        // Try to load as TMP_FontAsset first (if already converted)
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Font SDF.asset");
        if (fontAsset != null)
        {
            return fontAsset;
        }
        
        // Try alternative path
        fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/Font.asset");
        if (fontAsset != null)
        {
            return fontAsset;
        }
        
        // If TTF exists but no font asset, try to find any font asset in Fonts folder
        string[] fontAssets = AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { "Assets/_Project/Fonts" });
        if (fontAssets.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(fontAssets[0]);
            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (fontAsset != null)
            {
                Debug.Log($"Using font asset: {path}");
                return fontAsset;
            }
        }
        
        Debug.LogWarning("Custom font not found! Please convert Font.ttf to TMP Font Asset: " +
            "Right-click Font.ttf → Create → TextMeshPro → Font Asset");
        return null;
    }
    
    /// <summary>
    /// Assign custom font to a TextMeshProUGUI component
    /// </summary>
    static void AssignFont(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;
        
        TMP_FontAsset font = GetCustomFont();
        if (font != null)
        {
            textComponent.font = font;
        }
    }
    
    [MenuItem("Tools/Time Collapse/Setup All Scenes")]
    static void SetupAllScenes()
    {
        SetupMainMenuScene();
        SetupCreditsScene();
        SetupGameScene();
        Debug.Log("All scenes created! MainMenu, Credits, and Game scenes are ready.");
    }
    
    [MenuItem("Tools/Time Collapse/Setup Main Menu Scene")]
    static void SetupMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Create EventSystem (REQUIRED for UI buttons to work!)
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "TIME COLLAPSE";
        titleText.fontSize = 72;
        titleText.alignment = TextAlignmentOptions.Center;
        AssignFont(titleText); // Assign custom font
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0, 150);
        
        // Play Button
        GameObject playBtnObj = CreateUIButton("PlayButton", canvasObj, new Vector2(0, 50));
        Button playBtn = playBtnObj.GetComponent<Button>();
        TextMeshProUGUI playText = playBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (playText != null)
        {
            playText.text = "Play";
            AssignFont(playText); // Assign custom font
        }
        
        // Credits Button
        GameObject creditsBtnObj = CreateUIButton("CreditsButton", canvasObj, new Vector2(0, -50));
        Button creditsBtn = creditsBtnObj.GetComponent<Button>();
        TextMeshProUGUI creditsText = creditsBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (creditsText != null)
        {
            creditsText.text = "Credits";
            AssignFont(creditsText); // Assign custom font
        }
        
        // NO QUIT BUTTON - Removed per requirements (quit only on pause screen)
        
        // Setup MainMenuController
        GameObject menuCtrlObj = new GameObject("MainMenuController");
        MainMenuController menuCtrl = menuCtrlObj.AddComponent<MainMenuController>();
        menuCtrl.playButton = playBtn;
        menuCtrl.creditsButton = creditsBtn;
        menuCtrl.quitButton = null; // No quit button on main menu
        
        // Save scene
        string scenePath = "Assets/_Project/Scenes/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("MainMenu scene created at: " + scenePath);
    }
    
    [MenuItem("Tools/Time Collapse/Setup Credits Scene")]
    static void SetupCreditsScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Create EventSystem (REQUIRED for UI buttons to work!)
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Team Name Text
        GameObject teamNameObj = new GameObject("TeamNameText");
        teamNameObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI teamText = teamNameObj.AddComponent<TextMeshProUGUI>();
        teamText.text = "Garbage";
        teamText.fontSize = 96;
        teamText.alignment = TextAlignmentOptions.Center;
        AssignFont(teamText); // Assign custom font
        RectTransform teamRect = teamNameObj.GetComponent<RectTransform>();
        teamRect.anchorMin = new Vector2(0.5f, 0.5f);
        teamRect.anchorMax = new Vector2(0.5f, 0.5f);
        teamRect.pivot = new Vector2(0.5f, 0.5f);
        teamRect.anchoredPosition = new Vector2(0, 100);
        
        // Back Button
        GameObject backBtnObj = CreateUIButton("BackButton", canvasObj, new Vector2(0, -100));
        Button backBtn = backBtnObj.GetComponent<Button>();
        TextMeshProUGUI backText = backBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (backText != null)
        {
            backText.text = "Back";
            AssignFont(backText); // Assign custom font
        }
        
        // Setup CreditsController
        GameObject creditsCtrlObj = new GameObject("CreditsController");
        CreditsController creditsCtrl = creditsCtrlObj.AddComponent<CreditsController>();
        creditsCtrl.backButton = backBtn;
        creditsCtrl.teamNameText = teamText;
        
        // Save scene
        string scenePath = "Assets/_Project/Scenes/Credits.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Credits scene created at: " + scenePath);
    }
    
    [MenuItem("Tools/Time Collapse/Setup Game Scene")]
    static void SetupGameScene()
    {
        // Create new scene
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Setup camera for top-down view with follow script
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0, 0, -10);
            mainCam.orthographic = true;
            mainCam.orthographicSize = 10f;
            mainCam.gameObject.AddComponent<CameraFollow>();
        }
        
        // Create Player
        GameObject player = CreatePlayer();
        
        // Create Background
        GameObject bgObj = new GameObject("BackgroundGenerator");
        bgObj.AddComponent<BackgroundGenerator>();
        
        // Create Enemy Spawner
        GameObject spawner = new GameObject("EnemySpawner");
        EnemySpawner enemySpawner = spawner.AddComponent<EnemySpawner>();
        
        // Create Canvas and UI
        CreateUI();
        
        // Setup AudioManager
        SetupAudioManager();
        
        // AUTO-ASSIGN PREFABS (so they don't get reset when regenerating scene)
        AssignPrefabReferences(player, enemySpawner);
        
        // Save scene
        string scenePath = "Assets/_Project/Scenes/Game.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Game scene created at: " + scenePath);
        
        AssetDatabase.Refresh();
        Debug.Log("Setup complete! Prefabs have been automatically assigned.");
        Debug.Log("Next step: Configure Physics2D collision matrix (Tools menu)");
    }
    
    static GameObject CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        
        // Transform
        player.transform.position = Vector3.zero;
        
        // SpriteRenderer (white square)
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.color = Color.white;
        // Create a simple white square sprite
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        texture.SetPixels(pixels);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        sr.sprite = sprite;
        
        // Rigidbody2D - Fix for jittering/stuck movement
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f; // No drag to prevent jittering
        rb.angularDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smooth movement
        
        // Collider
        CircleCollider2D col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        
        // PlayerController
        player.AddComponent<PlayerController>();
        
        return player;
    }
    
    static void CreateUI()
    {
        // Create EventSystem (REQUIRED for UI buttons to work!)
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create Status Text (combined "score: NUM | health: NUM")
        GameObject statusTextObj = new GameObject("StatusText");
        statusTextObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI statusText = statusTextObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "score: 0 | health: 100";
        statusText.fontSize = 28;
        statusText.color = Color.green;
        AssignFont(statusText); // Assign custom font
        RectTransform statusRect = statusTextObj.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 1);
        statusRect.anchorMax = new Vector2(0, 1);
        statusRect.pivot = new Vector2(0, 1);
        statusRect.anchoredPosition = new Vector2(10, -10);
        
        // Create Game Over Panel
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform, false);
        Image panelBg = gameOverPanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        gameOverPanel.SetActive(false);
        
        // Game Over Text
        GameObject gameOverTextObj = new GameObject("GameOverText");
        gameOverTextObj.transform.SetParent(gameOverPanel.transform, false);
        TextMeshProUGUI gameOverText = gameOverTextObj.AddComponent<TextMeshProUGUI>();
        gameOverText.text = "GAME OVER";
        gameOverText.fontSize = 48;
        gameOverText.alignment = TextAlignmentOptions.Center;
        AssignFont(gameOverText); // Assign custom font
        RectTransform gameOverTextRect = gameOverTextObj.GetComponent<RectTransform>();
        gameOverTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        gameOverTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        gameOverTextRect.pivot = new Vector2(0.5f, 0.5f);
        gameOverTextRect.anchoredPosition = new Vector2(0, 50);
        
        // Restart Button
        GameObject restartBtnObj = new GameObject("RestartButton");
        restartBtnObj.transform.SetParent(gameOverPanel.transform, false);
        Image btnBg = restartBtnObj.AddComponent<Image>();
        btnBg.color = Color.gray;
        Button restartBtn = restartBtnObj.AddComponent<Button>();
        RectTransform btnRect = restartBtnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0, -50);
        btnRect.sizeDelta = new Vector2(200, 50);
        
        GameObject restartTextObj = new GameObject("Text");
        restartTextObj.transform.SetParent(restartBtnObj.transform, false);
        TextMeshProUGUI restartText = restartTextObj.AddComponent<TextMeshProUGUI>();
        restartText.text = "Restart";
        restartText.fontSize = 24;
        restartText.alignment = TextAlignmentOptions.Center;
        AssignFont(restartText); // Assign custom font
        RectTransform restartTextRect = restartTextObj.GetComponent<RectTransform>();
        restartTextRect.anchorMin = Vector2.zero;
        restartTextRect.anchorMax = Vector2.one;
        restartTextRect.sizeDelta = Vector2.zero;
        
        // Create Pause Panel
        GameObject pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvasObj.transform, false);
        Image pauseBg = pausePanel.AddComponent<Image>();
        pauseBg.color = new Color(0, 0, 0, 0.8f);
        RectTransform pauseRect = pausePanel.GetComponent<RectTransform>();
        pauseRect.anchorMin = Vector2.zero;
        pauseRect.anchorMax = Vector2.one;
        pauseRect.sizeDelta = Vector2.zero;
        pausePanel.SetActive(false);
        
        // Pause Text
        GameObject pauseTextObj = new GameObject("PauseText");
        pauseTextObj.transform.SetParent(pausePanel.transform, false);
        TextMeshProUGUI pauseText = pauseTextObj.AddComponent<TextMeshProUGUI>();
        pauseText.text = "PAUSED";
        pauseText.fontSize = 48;
        pauseText.alignment = TextAlignmentOptions.Center;
        AssignFont(pauseText); // Assign custom font
        RectTransform pauseTextRect = pauseTextObj.GetComponent<RectTransform>();
        pauseTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        pauseTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        pauseTextRect.pivot = new Vector2(0.5f, 0.5f);
        pauseTextRect.anchoredPosition = new Vector2(0, 100);
        
        // Resume Button
        GameObject resumeBtnObj = CreateUIButton("ResumeButton", pausePanel, new Vector2(0, 50));
        Button resumeBtn = resumeBtnObj.GetComponent<Button>();
        TextMeshProUGUI resumeText = resumeBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (resumeText != null)
        {
            resumeText.text = "Resume";
            AssignFont(resumeText); // Assign custom font
        }
        
        // Quit to Menu Button
        GameObject quitBtnObj = CreateUIButton("QuitButton", pausePanel, new Vector2(0, -50));
        Button quitBtn = quitBtnObj.GetComponent<Button>();
        TextMeshProUGUI quitText = quitBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (quitText != null)
        {
            quitText.text = "Quit to Menu";
            AssignFont(quitText); // Assign custom font
        }
        
        // Create Visible Pause Button (top-right corner)
        GameObject pauseBtnObj = CreateUIButton("PauseButton", canvasObj, new Vector2(-10, -10));
        Button pauseBtn = pauseBtnObj.GetComponent<Button>();
        TextMeshProUGUI pauseBtnText = pauseBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (pauseBtnText != null)
        {
            pauseBtnText.text = "Pause";
            AssignFont(pauseBtnText); // Assign custom font
        }
        RectTransform pauseBtnRect = pauseBtnObj.GetComponent<RectTransform>();
        pauseBtnRect.anchorMin = new Vector2(1, 1);
        pauseBtnRect.anchorMax = new Vector2(1, 1);
        pauseBtnRect.pivot = new Vector2(1, 1);
        pauseBtnRect.anchoredPosition = new Vector2(-10, -10);
        pauseBtnRect.sizeDelta = new Vector2(120, 40);
        
        // Setup GameHUD
        GameObject hudObj = new GameObject("GameHUD");
        GameHUD hud = hudObj.AddComponent<GameHUD>();
        hud.statusText = statusText; // Combined score and health text
        hud.gameOverPanel = gameOverPanel;
        hud.restartButton = restartBtn;
        
        // Setup PauseController
        GameObject pauseCtrlObj = new GameObject("PauseController");
        PauseController pauseCtrl = pauseCtrlObj.AddComponent<PauseController>();
        pauseCtrl.pausePanel = pausePanel;
        pauseCtrl.pauseButton = pauseBtn; // Visible pause button
        pauseCtrl.resumeButton = resumeBtn;
        pauseCtrl.quitButton = quitBtn; // Quit to menu button
    }
    
    static GameObject CreateUIButton(string name, GameObject parent, Vector2 position)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);
        Image btnBg = btnObj.AddComponent<Image>();
        btnBg.color = Color.gray;
        Button button = btnObj.AddComponent<Button>();
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = new Vector2(200, 50);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = name.Replace("Button", "");
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        AssignFont(text); // Assign custom font
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        return btnObj;
    }
    
    static void SetupAudioManager()
    {
        // Find or create AudioManager
        AudioManager audioMgr = Object.FindObjectOfType<AudioManager>();
        if (audioMgr == null)
        {
            GameObject audioObj = new GameObject("AudioManager");
            audioMgr = audioObj.AddComponent<AudioManager>();
        }
        
        // Add AudioSources if missing
        AudioSource[] sources = audioMgr.GetComponents<AudioSource>();
        if (sources.Length < 2)
        {
            if (sources.Length == 0)
            {
                audioMgr.musicSource = audioMgr.gameObject.AddComponent<AudioSource>();
                audioMgr.sfxSource = audioMgr.gameObject.AddComponent<AudioSource>();
            }
            else if (sources.Length == 1)
            {
                audioMgr.sfxSource = audioMgr.gameObject.AddComponent<AudioSource>();
            }
        }
        else
        {
            audioMgr.musicSource = sources[0];
            audioMgr.sfxSource = sources[1];
        }
    }
    
    [MenuItem("Tools/Time Collapse/Create All Prefabs")]
    static void CreatePrefabs()
    {
        // Create Prefabs folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
        }
        
        // Create Player Prefab
        GameObject playerPrefab = CreatePlayerPrefab();
        PrefabUtility.SaveAsPrefabAsset(playerPrefab, "Assets/_Project/Prefabs/Player.prefab");
        DestroyImmediate(playerPrefab);
        
        // Create Enemy Prefab
        GameObject enemyPrefab = CreateEnemyPrefab();
        PrefabUtility.SaveAsPrefabAsset(enemyPrefab, "Assets/_Project/Prefabs/Enemy.prefab");
        DestroyImmediate(enemyPrefab);
        
        // Create Projectile Prefab
        GameObject projectilePrefab = CreateProjectilePrefab();
        PrefabUtility.SaveAsPrefabAsset(projectilePrefab, "Assets/_Project/Prefabs/Projectile.prefab");
        DestroyImmediate(projectilePrefab);
        
        // Create TimeCrystal Prefab
        GameObject crystalPrefab = CreateCrystalPrefab();
        PrefabUtility.SaveAsPrefabAsset(crystalPrefab, "Assets/_Project/Prefabs/TimeCrystal.prefab");
        DestroyImmediate(crystalPrefab);
        
        AssetDatabase.Refresh();
        Debug.Log("All prefabs created successfully!");
    }
    
    static GameObject CreatePlayerPrefab()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        
        // SpriteRenderer
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.color = Color.white;
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        texture.SetPixels(pixels);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        sr.sprite = sprite;
        
        // Rigidbody2D - Fix for jittering/stuck movement
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f; // No drag to prevent jittering
        rb.angularDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smooth movement
        
        // Collider
        CircleCollider2D col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        
        // PlayerController
        player.AddComponent<PlayerController>();
        
        return player;
    }
    
    static GameObject CreateEnemyPrefab()
    {
        GameObject enemy = new GameObject("Enemy");
        enemy.tag = "Enemy";
        
        // SpriteRenderer (red)
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.color = Color.red;
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.red;
        texture.SetPixels(pixels);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        sr.sprite = sprite;
        
        // Rigidbody2D
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Collider
        CircleCollider2D col = enemy.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        
        // EnemyAI
        enemy.AddComponent<EnemyAI>();
        
        return enemy;
    }
    
    static GameObject CreateProjectilePrefab()
    {
        GameObject projectile = new GameObject("Projectile");
        projectile.tag = "Projectile";
        projectile.layer = LayerMask.NameToLayer("Projectile");
        
        // SpriteRenderer (yellow)
        SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
        sr.color = Color.yellow;
        Texture2D texture = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.yellow;
        texture.SetPixels(pixels);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        sr.sprite = sprite;
        
        // Rigidbody2D
        Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Collider (trigger)
        CircleCollider2D col = projectile.AddComponent<CircleCollider2D>();
        col.radius = 0.2f;
        col.isTrigger = true;
        
        // Projectile script
        projectile.AddComponent<Projectile>();
        
        return projectile;
    }
    
    static GameObject CreateCrystalPrefab()
    {
        GameObject crystal = new GameObject("TimeCrystal");
        crystal.tag = "Collectible";
        
        // SpriteRenderer (blue)
        SpriteRenderer sr = crystal.AddComponent<SpriteRenderer>();
        sr.color = Color.cyan;
        Texture2D texture = new Texture2D(24, 24);
        Color[] pixels = new Color[24 * 24];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.cyan;
        texture.SetPixels(pixels);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 24, 24), new Vector2(0.5f, 0.5f), 24);
        sr.sprite = sprite;
        
        // Collider (trigger)
        CircleCollider2D col = crystal.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;
        col.isTrigger = true;
        
        // TimeCrystal script
        crystal.AddComponent<TimeCrystal>();
        
        return crystal;
    }
    
    [MenuItem("Tools/Time Collapse/Fix Prefab Sprites")]
    static void FixPrefabSprites()
    {
        // Fix Enemy prefab
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemy.prefab");
        if (enemyPrefab != null)
        {
            SpriteRenderer enemySR = enemyPrefab.GetComponent<SpriteRenderer>();
            if (enemySR != null && enemySR.sprite == null)
            {
                Texture2D texture = new Texture2D(32, 32);
                Color[] pixels = new Color[32 * 32];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.red;
                texture.SetPixels(pixels);
                texture.Apply();
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
                enemySR.sprite = sprite;
                enemySR.color = Color.red;
                EditorUtility.SetDirty(enemyPrefab);
                PrefabUtility.SavePrefabAsset(enemyPrefab);
                Debug.Log("Fixed Enemy prefab sprite!");
            }
        }
        
        // Fix Projectile prefab
        GameObject projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Projectile.prefab");
        if (projectilePrefab != null)
        {
            SpriteRenderer projSR = projectilePrefab.GetComponent<SpriteRenderer>();
            if (projSR != null && projSR.sprite == null)
            {
                Texture2D texture = new Texture2D(16, 16);
                Color[] pixels = new Color[16 * 16];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.yellow;
                texture.SetPixels(pixels);
                texture.Apply();
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
                projSR.sprite = sprite;
                projSR.color = Color.yellow;
                EditorUtility.SetDirty(projectilePrefab);
                PrefabUtility.SavePrefabAsset(projectilePrefab);
                Debug.Log("Fixed Projectile prefab sprite!");
            }
        }
        
        // Fix TimeCrystal prefab
        GameObject crystalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/TimeCrystal.prefab");
        if (crystalPrefab != null)
        {
            SpriteRenderer crystalSR = crystalPrefab.GetComponent<SpriteRenderer>();
            if (crystalSR != null && crystalSR.sprite == null)
            {
                Texture2D texture = new Texture2D(24, 24);
                Color[] pixels = new Color[24 * 24];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.cyan;
                texture.SetPixels(pixels);
                texture.Apply();
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 24, 24), new Vector2(0.5f, 0.5f), 24);
                crystalSR.sprite = sprite;
                crystalSR.color = Color.cyan;
                EditorUtility.SetDirty(crystalPrefab);
                PrefabUtility.SavePrefabAsset(crystalPrefab);
                Debug.Log("Fixed TimeCrystal prefab sprite!");
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log("All prefab sprites fixed! Press Play to see enemies.");
    }
    
    [MenuItem("Tools/Time Collapse/Configure Physics2D Collision Matrix")]
    static void ConfigureCollisionMatrix()
    {
        // This needs to be done manually in Unity Editor:
        // Edit → Project Settings → Physics 2D → Layer Collision Matrix
        // Uncheck Projectile layer from Player layer
        Debug.Log("Please configure Physics2D collision matrix manually:");
        Debug.Log("Edit → Project Settings → Physics 2D → Layer Collision Matrix");
        Debug.Log("Uncheck: Projectile layer should NOT collide with Player layer");
    }
    
    [MenuItem("Tools/Time Collapse/Convert Font to TMP Font Asset")]
    static void ConvertFontToTMP()
    {
        // Find the TTF font file
        string fontPath = "Assets/_Project/Fonts/Font.ttf";
        Font font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        
        if (font == null)
        {
            Debug.LogError($"Font not found at {fontPath}. Please ensure Font.ttf exists in Assets/_Project/Fonts/");
            return;
        }
        
        // Check if font asset already exists
        string fontAssetPath = "Assets/_Project/Fonts/Font SDF.asset";
        TMP_FontAsset existingAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
        
        if (existingAsset != null)
        {
            Debug.Log($"Font asset already exists at {fontAssetPath}. Using existing asset.");
            return;
        }
        
        // Create TMP Font Asset from the font
        // Note: This requires the font to be imported as a Font asset first
        Debug.Log("To convert Font.ttf to TMP Font Asset:");
        Debug.Log("1. Select Font.ttf in Project panel");
        Debug.Log("2. In Inspector, set 'Font Size' and other import settings");
        Debug.Log("3. Right-click Font.ttf → Create → TextMeshPro → Font Asset");
        Debug.Log("4. Save it as 'Font SDF' in Assets/_Project/Fonts/");
        Debug.Log("5. The font will then be automatically assigned to all UI text!");
        
        // Try to select the font file
        Object fontObj = AssetDatabase.LoadAssetAtPath<Object>(fontPath);
        if (fontObj != null)
        {
            Selection.activeObject = fontObj;
            EditorGUIUtility.PingObject(fontObj);
        }
    }
    
    /// <summary>
    /// Automatically assign prefab references to prevent them from resetting
    /// </summary>
    static void AssignPrefabReferences(GameObject player, EnemySpawner enemySpawner)
    {
        // Load prefabs from Prefabs folder
        GameObject projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Projectile.prefab");
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemy.prefab");
        GameObject timeCrystalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/TimeCrystal.prefab");
        
        // Assign Projectile prefab to PlayerController
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                if (projectilePrefab != null)
                {
                    playerController.projectilePrefab = projectilePrefab;
                    Debug.Log("✓ Assigned Projectile prefab to PlayerController");
                }
                else
                {
                    Debug.LogWarning("Projectile prefab not found! Run 'Tools/Time Collapse/Create All Prefabs' first.");
                }
            }
        }
        
        // Assign Enemy prefab to EnemySpawner
        if (enemySpawner != null)
        {
            if (enemyPrefab != null)
            {
                enemySpawner.enemyPrefab = enemyPrefab;
                Debug.Log("✓ Assigned Enemy prefab to EnemySpawner");
            }
            else
            {
                Debug.LogWarning("Enemy prefab not found! Run 'Tools/Time Collapse/Create All Prefabs' first.");
            }
        }
        
        // Assign TimeCrystal prefab to Enemy prefab's EnemyAI component
        if (enemyPrefab != null)
        {
            EnemyAI enemyAI = enemyPrefab.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                if (timeCrystalPrefab != null)
                {
                    enemyAI.timeCrystalPrefab = timeCrystalPrefab;
                    EditorUtility.SetDirty(enemyPrefab); // Mark prefab as modified
                    PrefabUtility.SavePrefabAsset(enemyPrefab); // Save the prefab
                    Debug.Log("✓ Assigned TimeCrystal prefab to Enemy prefab's EnemyAI component");
                }
                else
                {
                    Debug.LogWarning("TimeCrystal prefab not found! Run 'Tools/Time Collapse/Create All Prefabs' first.");
                }
            }
        }
        
        // Mark scene as dirty so changes are saved
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}

