using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Automatically creates battle UI if it doesn't exist
/// Attach this to a GameObject in BattleScene - it will create all necessary UI elements
/// </summary>
public class BattleUISetup : MonoBehaviour {
    void Start() {
        // Find BattleManager
        BattleManager battleManager = FindFirstObjectByType<BattleManager>();
        if (battleManager == null) {
            Debug.LogWarning("BattleUISetup: BattleManager not found!");
            return;
        }
        
        // Setup UI elements
        CreateUIForBattleManager(battleManager);
    }
    
    /// <summary>
    /// Static method to create UI for a BattleManager (can be called directly)
    /// </summary>
    public static void CreateUIForBattleManager(BattleManager battleManager) {
        if (battleManager == null) {
            Debug.LogError("BattleUISetup: BattleManager is null!");
            return;
        }
        
        // Check for and destroy any persistent fade overlays that might block the UI
        GameObject persistentFade = GameObject.Find("PersistentScreenFade");
        if (persistentFade != null) {
            Debug.Log("BattleUISetup: Found persistent fade overlay, destroying it");
            Destroy(persistentFade);
        }
        
        // Also check for PersistentBattleFade or any other fade objects
        GameObject[] allFades = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject obj in allFades) {
            if (obj.name.Contains("Fade") || obj.name.Contains("Persistent")) {
                Canvas fadeCanvas = obj.GetComponent<Canvas>();
                if (fadeCanvas != null && fadeCanvas.name != "BattleCanvas") {
                    Debug.Log($"BattleUISetup: Destroying fade canvas: {obj.name}");
                    Destroy(obj);
                }
            }
        }
        
        // Always create a NEW BattleCanvas - don't reuse existing canvases
        // Find and destroy any existing BattleCanvas first
        GameObject existingBattleCanvas = GameObject.Find("BattleCanvas");
        if (existingBattleCanvas != null) {
            Debug.Log("BattleUISetup: Destroying existing BattleCanvas");
            Destroy(existingBattleCanvas);
        }
        
        // Create a fresh BattleCanvas
        GameObject canvasObj = new GameObject("BattleCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000; // High sorting order to ensure it's on top
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Ensure EventSystem exists (required for UI interactions)
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null) {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("BattleUISetup: Created EventSystem for UI interactions");
        } else {
            Debug.Log("BattleUISetup: EventSystem already exists");
        }
        
        Debug.Log("BattleUISetup: Created new BattleCanvas with sortingOrder 10000");
        
        // Setup UI elements
        SetupUI(canvas, battleManager);
    }
    
    static void SetupUI(Canvas canvas, BattleManager battleManager) {
        Debug.Log("BattleUISetup: Setting up UI elements on Canvas");
        
        // Create main UI container
        GameObject uiContainer = new GameObject("BattleUI");
        uiContainer.transform.SetParent(canvas.transform, false);
        RectTransform containerRect = uiContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.sizeDelta = Vector2.zero;
        containerRect.anchoredPosition = Vector2.zero;
        
        Debug.Log($"BattleUISetup: UI Container created, parent: {canvas.name}, position: {containerRect.position}");
        
        // Create Battle Log (top left) - larger size
        GameObject battleLogObj = CreateText("BattleLog", containerRect, new Vector2(10, -10), new Vector2(600, 300), TextAnchor.UpperLeft, 16);
        battleManager.battleLogText = battleLogObj.GetComponent<Text>();
        battleLogObj.SetActive(true);
        battleManager.battleLogText.text = "=== BATTLE STARTED ===\n";
        Debug.Log("BattleUISetup: Created BattleLog");
        
        // Create Hero Status (bottom left)
        GameObject heroStatusObj = CreateText("HeroStatus", containerRect, new Vector2(10, 10), new Vector2(300, 150), TextAnchor.LowerLeft, 12);
        battleManager.heroStatusText = heroStatusObj.GetComponent<Text>();
        heroStatusObj.SetActive(true);
        Debug.Log("BattleUISetup: Created HeroStatus");
        
        // Create Enemy Status (bottom right)
        GameObject enemyStatusObj = CreateText("EnemyStatus", containerRect, new Vector2(-10, 10), new Vector2(300, 150), TextAnchor.LowerRight, 12);
        battleManager.enemyStatusText = enemyStatusObj.GetComponent<Text>();
        enemyStatusObj.SetActive(true);
        Debug.Log("BattleUISetup: Created EnemyStatus");
        
        // Create Countdown Text (center, large)
        GameObject countdownObj = CreateText("Countdown", containerRect, Vector2.zero, new Vector2(200, 100), TextAnchor.MiddleCenter, 72);
        battleManager.countdownText = countdownObj.GetComponent<Text>();
        countdownObj.SetActive(false);
        countdownObj.GetComponent<Text>().color = Color.yellow;
        Debug.Log("BattleUISetup: Created Countdown");
        
        // Create Current Hero Text (top center, below battle log area)
        GameObject currentHeroObj = CreateText("CurrentHero", containerRect, new Vector2(0, -320), new Vector2(500, 60), TextAnchor.UpperCenter, 20);
        battleManager.currentHeroText = currentHeroObj.GetComponent<Text>();
        currentHeroObj.GetComponent<Text>().color = new Color(1f, 0.8f, 0.2f); // Gold/yellow color
        currentHeroObj.GetComponent<Text>().fontStyle = FontStyle.Bold;
        currentHeroObj.SetActive(true);
        Debug.Log("BattleUISetup: Created CurrentHero");
        
        // Create Action Selection Panel (center)
        GameObject actionPanel = CreateActionSelectionPanel(containerRect, battleManager);
        battleManager.actionSelectionPanel = actionPanel;
        actionPanel.SetActive(false);
        
        // Create Hero Selection Buttons (left side)
        CreateHeroSelectionButtons(containerRect, battleManager);
    }
    
    static GameObject CreateText(string name, RectTransform parent, Vector2 position, Vector2 size, TextAnchor anchor, int fontSize) {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = anchor == TextAnchor.LowerLeft || anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.UpperLeft ? new Vector2(0, 0) :
                        anchor == TextAnchor.LowerRight || anchor == TextAnchor.MiddleRight || anchor == TextAnchor.UpperRight ? new Vector2(1, 0) :
                        new Vector2(0.5f, 0.5f);
        rect.anchorMax = rect.anchorMin;
        rect.pivot = new Vector2(
            anchor == TextAnchor.LowerLeft || anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.UpperLeft ? 0 :
            anchor == TextAnchor.LowerRight || anchor == TextAnchor.MiddleRight || anchor == TextAnchor.UpperRight ? 1 : 0.5f,
            anchor == TextAnchor.LowerLeft || anchor == TextAnchor.LowerCenter || anchor == TextAnchor.LowerRight ? 0 :
            anchor == TextAnchor.UpperLeft || anchor == TextAnchor.UpperCenter || anchor == TextAnchor.UpperRight ? 1 : 0.5f
        );
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        
        Text text = textObj.AddComponent<Text>();
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) {
            // Try Arial as fallback
            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        text.font = defaultFont;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = anchor;
        text.text = name; // Set initial text so we can see it
        text.raycastTarget = false; // Don't block clicks
        
        // Ensure it's active and visible
        textObj.SetActive(true);
        
        Debug.Log($"BattleUISetup: Created text '{name}' at position {position}, size {size}, font: {(defaultFont != null ? defaultFont.name : "NULL")}");
        
        return textObj;
    }
    
    static GameObject CreateActionSelectionPanel(RectTransform parent, BattleManager battleManager) {
        GameObject panel = new GameObject("ActionSelectionPanel");
        panel.transform.SetParent(parent, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(400, 300);
        panelRect.anchoredPosition = Vector2.zero;
        
        // Add background
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        
        // Create buttons
        float buttonWidth = 150;
        float buttonHeight = 40;
        float spacing = 50;
        float startY = 100;
        
        battleManager.attackButton = CreateButton("AttackButton", panel.transform, new Vector2(0, startY), new Vector2(buttonWidth, buttonHeight), "Attack");
        battleManager.ability1Button = CreateButton("Ability1Button", panel.transform, new Vector2(0, startY - spacing), new Vector2(buttonWidth, buttonHeight), "Ability 1");
        battleManager.ability2Button = CreateButton("Ability2Button", panel.transform, new Vector2(0, startY - spacing * 2), new Vector2(buttonWidth, buttonHeight), "Ability 2");
        battleManager.checkButton = CreateButton("CheckButton", panel.transform, new Vector2(0, startY - spacing * 3), new Vector2(buttonWidth, buttonHeight), "Check");
        battleManager.potionButton = CreateButton("PotionButton", panel.transform, new Vector2(0, startY - spacing * 4), new Vector2(buttonWidth, buttonHeight), "Potion");
        
        // Create target selection container (using buttons instead of dropdown for reliability)
        GameObject targetContainerObj = new GameObject("TargetSelectionContainer");
        targetContainerObj.transform.SetParent(panel.transform, false);
        RectTransform targetContainerRect = targetContainerObj.AddComponent<RectTransform>();
        targetContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
        targetContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
        targetContainerRect.pivot = new Vector2(0.5f, 0.5f);
        targetContainerRect.sizeDelta = new Vector2(200, 150);
        targetContainerRect.anchoredPosition = new Vector2(0, -150);
        
        // Add VerticalLayoutGroup for stacking buttons
        VerticalLayoutGroup targetLayout = targetContainerObj.AddComponent<VerticalLayoutGroup>();
        targetLayout.spacing = 10;
        targetLayout.childControlHeight = false;
        targetLayout.childControlWidth = true;
        targetLayout.childForceExpandWidth = true;
        targetLayout.childForceExpandHeight = false;
        
        battleManager.targetSelectionContainer = targetContainerObj;
        targetContainerObj.SetActive(false);
        
        Debug.Log("BattleUISetup: Created target selection container");
        
        // Create confirm button
        battleManager.confirmButton = CreateButton("ConfirmButton", panel.transform, new Vector2(0, -200), new Vector2(buttonWidth, buttonHeight), "Confirm");
        battleManager.confirmButton.gameObject.SetActive(false);
        
        return panel;
    }
    
    public static Button CreateButton(string name, Transform parent, Vector2 position, Vector2 size, string text) {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        image.raycastTarget = true; // Ensure image can receive raycasts
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;
        button.interactable = true; // Ensure button is interactable
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        colors.selectedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        button.colors = colors;
        
        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 14;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        return button;
    }
    
    static void CreateHeroSelectionButtons(RectTransform parent, BattleManager battleManager) {
        GameObject buttonContainer = new GameObject("HeroSelectionButtons");
        buttonContainer.transform.SetParent(parent, false);
        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        // Position at top left
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.sizeDelta = new Vector2(250, 180);
        containerRect.anchoredPosition = new Vector2(10, -10);
        
        // Store reference to container in BattleManager
        battleManager.heroSelectionButtonsContainer = buttonContainer;
        
        battleManager.heroSelectionButtons = new Button[3];
        for (int i = 0; i < 3; i++) {
            battleManager.heroSelectionButtons[i] = CreateButton(
                $"HeroButton_{i + 1}",
                buttonContainer.transform,
                new Vector2(0, -10 - i * 60), // Stack from top
                new Vector2(220, 50),
                $"Hero {i + 1}"
            );
            // Initially hide buttons - they'll be shown when battle starts
            battleManager.heroSelectionButtons[i].gameObject.SetActive(false);
            Debug.Log($"BattleUISetup: Created hero button {i + 1}");
        }
        
        Debug.Log("BattleUISetup: Hero selection buttons container created and active");
    }
}

