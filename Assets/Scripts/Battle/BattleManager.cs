using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleSystem;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour {
    [Header("Battle Setup")]
    public Party party = new Party();
    public EnemyGroup enemyGroup = new EnemyGroup();

    [Header("UI References")]
    public Text battleLogText;
    public Text heroStatusText;
    public Text enemyStatusText;
    public Text countdownText;
    public Text currentHeroText;
    
    [Header("Action Selection UI")]
    public GameObject actionSelectionPanel;
    public Button attackButton;
    public Button ability1Button;
    public Button ability2Button;
    public Button checkButton;
    public Button potionButton;
    public Button confirmButton;
    public Dropdown targetDropdown; // Deprecated - using targetButtons instead
    [Header("Target Selection UI")]
    public GameObject targetSelectionContainer; // Container for target selection buttons
    private Button[] targetButtons = new Button[3]; // Buttons for selecting enemy targets
    
    [Header("Hero Selection UI")]
    public Button[] heroSelectionButtons = new Button[3];
    public GameObject heroSelectionButtonsContainer; // Reference to the container GameObject
    
    [Header("Entity GameObjects")]
    [Tooltip("Hero GameObjects (Protag_1, Protag_2, Protag_3)")]
    public GameObject[] heroGameObjects = new GameObject[3];
    [Tooltip("Enemy GameObjects (Antag_1, Antag_2, Antag_3)")]
    public GameObject[] enemyGameObjects = new GameObject[3];
    
    [Header("Audio")]
    private AudioSource audioSource;
    private AudioClip attackClip;

    private List<Action> heroActions = new List<Action>();
    private List<Action> enemyActions = new List<Action>();
    private Hero currentHeroSelecting = null;
    private int currentHeroIndex = 0;
    private ActionType pendingActionType = ActionType.None;
    private bool allHeroesSelected = false;
    private bool battleActive = false;
    
    // Mapping from Entity to GameObject for visual effects
    private Dictionary<Entity, GameObject> entityToGameObject = new Dictionary<Entity, GameObject>();

    void Start() {
        // Don't auto-initialize - let BattleSetup handle it
        
        // Setup audio for attack sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Load attack audio clip from Resources/Audio folder
        attackClip = Resources.Load<AudioClip>("Audio/attack");
        
        if (attackClip == null) {
            Debug.LogWarning("BattleManager: Could not load attack audio clip!");
        }
    }

    public void InitializeBattle() {
        Debug.Log("BattleManager: InitializeBattle called");
        
        if (enemyGroup.enemies.Count == 0) {
            Debug.LogWarning("BattleManager: No enemies set up! Adding defaults.");
            enemyGroup.AddEnemy(new Goblin());
            enemyGroup.AddEnemy(new Orc());
        }
        
        if (party.heroes.Count == 0) {
            Debug.LogWarning("BattleManager: No heroes set up! Adding defaults.");
            party.AddHero(new BlinkerBell());
            party.AddHero(new Blandroid());
            party.AddHero(new Jachariah());
        }

        Debug.Log($"BattleManager: Party has {party.heroes.Count} heroes, EnemyGroup has {enemyGroup.enemies.Count} enemies");

        // Setup UI buttons
        SetupUIButtons();
        Debug.Log("BattleManager: UI buttons setup complete");
        
        // Build entity to GameObject mapping
        BuildEntityMapping();
        Debug.Log("BattleManager: Entity mapping built");
        
        // Update UI immediately
        UpdateUI();
        
        // Show hero selection UI immediately
        ShowHeroSelection();
        
        battleActive = true;
        Debug.Log("BattleManager: Starting battle loop");
        StartCoroutine(BattleLoop());
    }
    
    void BuildEntityMapping() {
        entityToGameObject.Clear();
        
        // Map heroes to GameObjects
        for (int i = 0; i < party.heroes.Count && i < heroGameObjects.Length; i++) {
            if (heroGameObjects[i] != null && party.heroes[i] != null) {
                entityToGameObject[party.heroes[i]] = heroGameObjects[i];
                
                // Link the BattleEntityLinker if it exists
                BattleEntityLinker linker = heroGameObjects[i].GetComponent<BattleEntityLinker>();
                if (linker != null) {
                    linker.linkedEntity = party.heroes[i];
                    linker.isHero = true;
                    linker.entityIndex = i;
                }
            }
        }
        
        // Map enemies to GameObjects
        for (int i = 0; i < enemyGroup.enemies.Count && i < enemyGameObjects.Length; i++) {
            if (enemyGameObjects[i] != null && enemyGroup.enemies[i] != null) {
                entityToGameObject[enemyGroup.enemies[i]] = enemyGameObjects[i];
                
                // Link the BattleEntityLinker if it exists
                BattleEntityLinker linker = enemyGameObjects[i].GetComponent<BattleEntityLinker>();
                if (linker != null) {
                    linker.linkedEntity = enemyGroup.enemies[i];
                    linker.isHero = false;
                    linker.entityIndex = i;
                }
            }
        }
        
        // Auto-find GameObjects if not assigned
        AutoFindEntityGameObjects();
    }
    
    void AutoFindEntityGameObjects() {
        // Auto-find hero GameObjects
        for (int i = 0; i < heroGameObjects.Length; i++) {
            if (heroGameObjects[i] == null) {
                GameObject heroObj = GameObject.Find($"Protag_{i + 1}");
                if (heroObj != null) {
                    heroGameObjects[i] = heroObj;
                    if (i < party.heroes.Count && party.heroes[i] != null) {
                        entityToGameObject[party.heroes[i]] = heroObj;
                    }
                }
            }
        }
        
        // Auto-find enemy GameObjects
        for (int i = 0; i < enemyGameObjects.Length; i++) {
            if (enemyGameObjects[i] == null) {
                GameObject enemyObj = GameObject.Find($"Antag_{i + 1}");
                if (enemyObj != null) {
                    enemyGameObjects[i] = enemyObj;
                    if (i < enemyGroup.enemies.Count && enemyGroup.enemies[i] != null) {
                        entityToGameObject[enemyGroup.enemies[i]] = enemyObj;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Get the GameObject representing an Entity
    /// </summary>
    GameObject GetEntityGameObject(Entity entity) {
        if (entity == null) return null;
        
        if (entityToGameObject.ContainsKey(entity)) {
            return entityToGameObject[entity];
        }
        
        // Fallback: search by name
        if (entity is Hero) {
            // Try to find by index in party
            for (int i = 0; i < party.heroes.Count; i++) {
                if (party.heroes[i] == entity) {
                    GameObject heroObj = GameObject.Find($"Protag_{i + 1}");
                    if (heroObj != null) {
                        entityToGameObject[entity] = heroObj;
                        return heroObj;
                    }
                }
            }
        } else if (entity is Enemy) {
            // Try to find by index in enemy group
            for (int i = 0; i < enemyGroup.enemies.Count; i++) {
                if (enemyGroup.enemies[i] == entity) {
                    GameObject enemyObj = GameObject.Find($"Antag_{i + 1}");
                    if (enemyObj != null) {
                        entityToGameObject[entity] = enemyObj;
                        return enemyObj;
                    }
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Flash an entity's GameObject when it takes damage
    /// </summary>
    void FlashEntity(Entity entity) {
        if (entity == null) return;
        
        GameObject entityObj = GetEntityGameObject(entity);
        if (entityObj != null) {
            BattleEntityLinker linker = entityObj.GetComponent<BattleEntityLinker>();
            if (linker != null) {
                linker.FlashOnDamage();
            } else {
                // Fallback: try to get DamageFlashEffect directly
                DamageFlashEffect flash = entityObj.GetComponent<DamageFlashEffect>();
                if (flash != null) {
                    flash.Flash();
                }
            }
        }
    }

    void SetupUIButtons() {
        // Clear existing listeners first to avoid duplicates
        if (attackButton) {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(() => OnActionSelected(ActionType.Attack));
        }
        if (ability1Button) {
            ability1Button.onClick.RemoveAllListeners();
            ability1Button.onClick.AddListener(() => OnActionSelected(ActionType.Ability1));
        }
        if (ability2Button) {
            ability2Button.onClick.RemoveAllListeners();
            ability2Button.onClick.AddListener(() => OnActionSelected(ActionType.Ability2));
        }
        if (checkButton) {
            checkButton.onClick.RemoveAllListeners();
            checkButton.onClick.AddListener(() => OnActionSelected(ActionType.Check));
        }
        if (potionButton) {
            potionButton.onClick.RemoveAllListeners();
            potionButton.onClick.AddListener(() => OnActionSelected(ActionType.Potion));
        }
        if (confirmButton) {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmSelection);
        }
        if (targetDropdown) {
            targetDropdown.onValueChanged.RemoveAllListeners();
            targetDropdown.onValueChanged.AddListener(OnTargetSelected);
        }
        
        // Setup hero selection buttons
        for (int i = 0; i < heroSelectionButtons.Length && i < party.heroes.Count; i++) {
            int heroIndex = i; // Capture for closure
            if (heroSelectionButtons[i] != null) {
                heroSelectionButtons[i].onClick.RemoveAllListeners();
                heroSelectionButtons[i].onClick.AddListener(() => {
                    Debug.Log($"BattleManager: Hero button {heroIndex} clicked!");
                    SelectHero(heroIndex);
                });
                heroSelectionButtons[i].interactable = true; // Ensure button is interactable
            }
        }
        
        Debug.Log("BattleManager: UI buttons setup complete with listeners");
    }

    IEnumerator BattleLoop() {
        while (battleActive && party.HasAliveHeroes() && enemyGroup.HasAliveEnemies()) {
            yield return StartCoroutine(RunTurn());
            yield return new WaitForSeconds(1f); // Brief pause between turns
        }

        // Battle ended
        if (!enemyGroup.HasAliveEnemies()) {
            Debug.Log("You win!");
            if (battleLogText) battleLogText.text += "\n\nYou win!";
            
            // Wait a moment to show victory message
            yield return new WaitForSeconds(2f);
            
            // Return to WorldScene
            StartCoroutine(ReturnToWorldScene());
        } else if (!party.HasAliveHeroes()) {
            Debug.Log("You lose...");
            if (battleLogText) battleLogText.text += "\n\nYou lose...";
            // Game over - could add game over screen here
        }
    }
    
    IEnumerator ReturnToWorldScene() {
        // Find or create ScreenFade for transition
        ScreenFade fader = FindFirstObjectByType<ScreenFade>();
        
        if (fader == null || fader.canvasGroup == null) {
            // Create a ScreenFade overlay if one doesn't exist
            GameObject fadeObject = new GameObject("BattleToWorldFade");
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
            fader.fadeDuration = 0.5f;
        }
        
        // Fade out
        yield return fader.StartCoroutine(fader.FadeOut());
        
        // Create persistent fade for scene transition
        GameObject persistentFadeObject = new GameObject("PersistentScreenFade");
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
        persistentFader.fadeDuration = fader.fadeDuration;
        
        // Load WorldScene
        var async = SceneManager.LoadSceneAsync("WorldScene");
        async.allowSceneActivation = true;
        
        yield return null;
        yield return null;
        
        // Fade in
        yield return persistentFader.StartCoroutine(persistentFader.FadeIn());
        
        // Clean up
        Destroy(persistentFadeObject);
    }

    IEnumerator RunTurn() {
        heroActions.Clear();
        enemyActions.Clear();
        allHeroesSelected = false;
        
        // Reset hero selection state for new turn
        currentHeroSelecting = null;
        currentHeroIndex = 0;
        pendingActionType = ActionType.None;
        
        UpdateUI();
        
        // --- SELECTION PHASE: Get all hero actions ---
        if (battleLogText) battleLogText.text = "=== SELECT YOUR ACTIONS ===\n";
        
        // Show hero selection UI
        ShowHeroSelection();
        
        // Wait for all heroes to have actions selected
        while (!allHeroesSelected) {
            yield return null;
        }
        
        HideHeroSelection();
        HideActionSelection();
        
        // --- ENEMY AI: Enemies select random heroes to target ---
        for (int i = enemyGroup.enemies.Count - 1; i >= 0; i--) {
            Enemy e = enemyGroup.enemies[i];
            if (!e.IsAlive()) {
                enemyGroup.enemies.RemoveAt(i);
                continue;
            }
            
            // Enemies always target random heroes
            Hero randomHero = EnemyAIHelpers.GetRandomHero(party);
            if (randomHero != null && randomHero.IsAlive()) {
                // Decide action type (attack or ability)
                Action enemyAction = e.DecideAction(party);
                
                // Special handling for Dragon boss (AOE attacks)
                if (e is Dragon dragon) {
                    // Dragon's Flame Roar targets all heroes (target can be null or any hero)
                    if (enemyAction.description == "Flame Roar") {
                        // Keep target as null for AOE, BattleManager will handle it
                        enemyAction.target = null;
                    } else {
                        // Other dragon attacks target random hero
                        enemyAction.target = randomHero;
                    }
                } else {
                    // All other enemies target random heroes
                    enemyAction.target = randomHero;
                }
                
                enemyActions.Add(enemyAction);
            }
        }
        
        // --- COUNTDOWN PHASE ---
        if (battleLogText) battleLogText.text = "=== ALL ACTIONS SELECTED ===\n";
        if (countdownText) {
            countdownText.gameObject.SetActive(true);
            countdownText.fontSize = 72; // Make it large and visible
        }
        
        // 3 second countdown
        for (int i = 3; i > 0; i--) {
            if (countdownText) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        
        if (countdownText) {
            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
            countdownText.gameObject.SetActive(false);
        }
        
        // --- EXECUTION PHASE: All actions happen simultaneously ---
        if (battleLogText) battleLogText.text += "\n=== EXECUTING ACTIONS ===\n";
        Debug.Log($"BattleManager: Executing {heroActions.Count} hero actions and {enemyActions.Count} enemy actions");
        
        // Play attack sound when actions execute
        if (audioSource != null && attackClip != null) {
            audioSource.PlayOneShot(attackClip);
        }
        
        // Combine all actions
        List<Action> allActions = new List<Action>();
        allActions.AddRange(heroActions);
        allActions.AddRange(enemyActions);
        
        // Execute all actions simultaneously
        List<ActionResult> results = new List<ActionResult>();
        HashSet<Entity> damagedEntities = new HashSet<Entity>(); // Track which entities took damage
        
        foreach (Action a in allActions) {
            if (a.user == null || !a.user.IsAlive()) {
                Debug.Log($"BattleManager: Skipping action - user is null or dead");
                continue;
            }
            if ((a.type == ActionType.Attack || a.type == ActionType.Ability1 || a.type == ActionType.Ability2)
                && (a.target == null || !a.target.IsAlive())) {
                Debug.Log($"BattleManager: Skipping action - target is null or dead");
                continue;
            }
            
            int beforeHP = (a.target != null ? a.target.GetHealth() : 0);
            Debug.Log($"BattleManager: Executing action - {a.user.GetType().Name} uses {a.type} on {a.target?.GetType().Name} (target HP before: {beforeHP})");
            ResolveAction(a);
            int afterHP = (a.target != null ? a.target.GetHealth() : 0);
            int damageDealt = beforeHP - afterHP;
            Debug.Log($"BattleManager: Action result - target HP after: {afterHP}, damage dealt: {damageDealt}");
            
            // Track if target took damage
            if (damageDealt > 0 && a.target != null) {
                damagedEntities.Add(a.target);
            }
            
            results.Add(new ActionResult {
                action = a,
                damageDealt = damageDealt
            });
        }
        
        // Flash all entities that took damage
        foreach (Entity damagedEntity in damagedEntities) {
            FlashEntity(damagedEntity);
        }
        
        // Hide dead entities (make them disappear)
        foreach (Hero hero in party.heroes) {
            if (!hero.IsAlive()) {
                GameObject heroObj = GetEntityGameObject(hero);
                if (heroObj != null) {
                    heroObj.SetActive(false);
                    Debug.Log($"BattleManager: Hiding dead hero {hero.GetType().Name}");
                }
            }
        }
        
        foreach (Enemy enemy in enemyGroup.enemies) {
            if (!enemy.IsAlive()) {
                GameObject enemyObj = GetEntityGameObject(enemy);
                if (enemyObj != null) {
                    enemyObj.SetActive(false);
                    Debug.Log($"BattleManager: Hiding dead enemy {enemy.GetType().Name}");
                }
            }
        }
        
        // Display all results
        foreach (var result in results) {
            Action a = result.action;
            string actorName = a.user.GetType().Name;
            string desc = string.IsNullOrEmpty(a.description) ? "performs an action" : a.description;
            string logEntry = actorName + " uses " + desc;
            
            if (a.type == ActionType.Attack || a.type == ActionType.Ability1 || a.type == ActionType.Ability2) {
                if (a.target != null)
                    logEntry += " on " + a.target.GetType().Name;
                if (result.damageDealt > 0)
                    logEntry += " dealing " + result.damageDealt + " damage";
                else
                    logEntry += " (no damage)";
            }
            else if (a.type == ActionType.Potion) {
                logEntry += " and heals 3 HP (" + party.potions + " left)";
            }
            
            if (battleLogText) battleLogText.text += logEntry + "\n";
        }
        
        // Remove dead enemies
        enemyGroup.enemies.RemoveAll(e => !e.IsAlive());
        
        UpdateUI();
    }

    void ShowHeroSelection() {
        Debug.Log("BattleManager: Showing hero selection UI");
        
        // Show which heroes need actions
        if (currentHeroText) {
            currentHeroText.gameObject.SetActive(true);
            UpdateCurrentHeroText();
            Debug.Log("BattleManager: Current hero text shown");
        } else {
            Debug.LogWarning("BattleManager: currentHeroText is null!");
        }
        
        // Show hero selection buttons container first
        if (heroSelectionButtonsContainer != null) {
            heroSelectionButtonsContainer.SetActive(true);
            Debug.Log("BattleManager: Hero selection buttons container shown");
        } else {
            Debug.LogWarning("BattleManager: HeroSelectionButtons container not found! Trying GameObject.Find as fallback...");
            GameObject buttonContainer = GameObject.Find("HeroSelectionButtons");
            if (buttonContainer != null) {
                heroSelectionButtonsContainer = buttonContainer;
                buttonContainer.SetActive(true);
                Debug.Log("BattleManager: Found container via GameObject.Find");
            } else {
                Debug.LogError("BattleManager: HeroSelectionButtons container not found even with GameObject.Find!");
            }
        }
        
        // Enable hero selection buttons for alive heroes
        for (int i = 0; i < heroSelectionButtons.Length && i < party.heroes.Count; i++) {
            if (heroSelectionButtons[i] != null) {
                bool isAlive = party.heroes[i].IsAlive();
                heroSelectionButtons[i].gameObject.SetActive(isAlive);
                if (isAlive) {
                    Text buttonText = heroSelectionButtons[i].GetComponentInChildren<Text>();
                    if (buttonText) {
                        string heroName = party.heroes[i].GetType().Name;
                        bool hasAction = heroActions.Exists(a => a.user == party.heroes[i]);
                        buttonText.text = heroName + (hasAction ? " ✓" : "");
                        Debug.Log($"BattleManager: Hero button {i} activated for {heroName}");
                    }
                }
            } else {
                Debug.LogWarning($"BattleManager: heroSelectionButtons[{i}] is null!");
            }
        }
    }

    void HideHeroSelection() {
        if (currentHeroText) currentHeroText.gameObject.SetActive(false);
        foreach (var btn in heroSelectionButtons) {
            if (btn != null) btn.gameObject.SetActive(false);
        }
        
        // Hide hero selection buttons container
        if (heroSelectionButtonsContainer != null) {
            heroSelectionButtonsContainer.SetActive(false);
        } else {
            // Fallback
            GameObject buttonContainer = GameObject.Find("HeroSelectionButtons");
            if (buttonContainer != null) {
                buttonContainer.SetActive(false);
            }
        }
    }

    void SelectHero(int heroIndex) {
        Debug.Log($"BattleManager: SelectHero called with index {heroIndex}");
        
        if (heroIndex < 0 || heroIndex >= party.heroes.Count) {
            Debug.LogWarning($"BattleManager: Invalid hero index {heroIndex}");
            return;
        }
        
        Hero hero = party.heroes[heroIndex];
        if (!hero.IsAlive()) {
            Debug.LogWarning($"BattleManager: Hero {heroIndex} is not alive");
            return;
        }
        
        // Check if this hero already has an action
        if (heroActions.Exists(a => a.user == hero)) {
            Debug.Log($"{hero.GetType().Name} already has an action selected!");
            return;
        }
        
        // If we're already selecting for the same hero, don't allow it
        if (currentHeroSelecting == hero) {
            Debug.Log($"BattleManager: Already selecting for {hero.GetType().Name}");
            return;
        }
        
        // Allow switching to a different hero (clear previous selection)
        currentHeroSelecting = hero;
        currentHeroIndex = heroIndex;
        pendingActionType = ActionType.None;
        
        Debug.Log($"BattleManager: Selected hero {heroIndex}: {hero.GetType().Name}");
        
        // Show action selection UI
        ShowActionSelection();
        UpdateCurrentHeroText();
    }

    void ShowActionSelection() {
        if (actionSelectionPanel) actionSelectionPanel.SetActive(true);
        
        if (currentHeroSelecting != null) {
            // Update ability button text
            if (ability1Button) {
                Text text = ability1Button.GetComponentInChildren<Text>();
                if (text) text.text = currentHeroSelecting.Ability1Name();
            }
            if (ability2Button) {
                Text text = ability2Button.GetComponentInChildren<Text>();
                if (text) text.text = currentHeroSelecting.Ability2Name();
            }
        }
        
        // Populate target dropdown
        if (targetDropdown != null) {
            targetDropdown.ClearOptions();
            List<string> options = new List<string>();
            foreach (Enemy e in enemyGroup.enemies) {
                if (e.IsAlive())
                    options.Add(e.GetName() + " (HP: " + e.GetHealth() + ")");
            }
            targetDropdown.AddOptions(options);
            targetDropdown.gameObject.SetActive(true);
        }
        
        if (confirmButton) confirmButton.gameObject.SetActive(false);
    }

    void HideActionSelection() {
        if (actionSelectionPanel) actionSelectionPanel.SetActive(false);
        if (targetDropdown) targetDropdown.gameObject.SetActive(false);
        if (targetSelectionContainer) targetSelectionContainer.SetActive(false);
        if (confirmButton) confirmButton.gameObject.SetActive(false);
    }

    void OnActionSelected(ActionType actionType) {
        Debug.Log($"BattleManager: OnActionSelected called with actionType: {actionType}");
        
        if (currentHeroSelecting == null) {
            Debug.LogWarning("BattleManager: OnActionSelected called but no hero is selecting!");
            return;
        }
        
        pendingActionType = actionType;
        
        // Some actions don't need targets
        if (actionType == ActionType.Potion || actionType == ActionType.Check) {
            Debug.Log($"BattleManager: Action {actionType} doesn't need target, confirming immediately");
            OnConfirmSelection();
            return;
        }
        
        // Show target selection buttons for attacks and abilities
        ShowTargetSelection();
    }
    
    void ShowTargetSelection() {
        if (targetSelectionContainer == null) {
            Debug.LogError("BattleManager: targetSelectionContainer is null! Cannot show target selection.");
            return;
        }
        
        // Clear existing buttons
        foreach (Transform child in targetSelectionContainer.transform) {
            Destroy(child.gameObject);
        }
        
        // Create buttons for each alive enemy
        List<Enemy> aliveEnemies = new List<Enemy>();
        foreach (Enemy e in enemyGroup.enemies) {
            if (e.IsAlive()) {
                aliveEnemies.Add(e);
            }
        }
        
        if (aliveEnemies.Count == 0) {
            Debug.LogWarning("BattleManager: No alive enemies to target!");
            return;
        }
        
        // Create buttons for each enemy
        for (int i = 0; i < aliveEnemies.Count; i++) {
            Enemy enemy = aliveEnemies[i];
            int enemyIndex = i; // Capture for closure
            
            Button targetButton = BattleUISetup.CreateButton(
                $"TargetButton_{i}",
                targetSelectionContainer.transform,
                Vector2.zero, // Position handled by layout group
                new Vector2(180, 40),
                $"{enemy.GetName()} (HP: {enemy.GetHealth()})"
            );
            
            // Set up button click to select this target
            targetButton.onClick.RemoveAllListeners();
            targetButton.onClick.AddListener(() => {
                Debug.Log($"BattleManager: Target button clicked for enemy {enemyIndex}: {enemy.GetName()}");
                SelectTarget(enemy);
            });
        }
        
        targetSelectionContainer.SetActive(true);
        Debug.Log($"BattleManager: Target selection shown with {aliveEnemies.Count} enemies");
    }
    
    void SelectTarget(Enemy target) {
        if (target == null || !target.IsAlive()) {
            Debug.LogWarning("BattleManager: Invalid target selected!");
            return;
        }
        
        Debug.Log($"BattleManager: Target selected: {target.GetName()}");
        
        // Hide target selection and action selection panel
        if (targetSelectionContainer) {
            targetSelectionContainer.SetActive(false);
        }
        if (actionSelectionPanel) {
            actionSelectionPanel.SetActive(false);
        }
        
        // Create action with selected target
        string desc = "";
        if (pendingActionType == ActionType.Attack) desc = "attacks";
        else if (pendingActionType == ActionType.Ability1) desc = currentHeroSelecting.Ability1Name();
        else if (pendingActionType == ActionType.Ability2) desc = currentHeroSelecting.Ability2Name();
        
        Action action = new Action(currentHeroSelecting, pendingActionType, target, desc);
        heroActions.Add(action);
        
        // Clear current hero selection
        Hero completedHero = currentHeroSelecting;
        currentHeroSelecting = null;
        pendingActionType = ActionType.None;
        
        Debug.Log($"BattleManager: {completedHero.GetType().Name} action confirmed, moving to next hero");
        
        // Check if all heroes have actions now
        CheckAllHeroesSelected();
        
        if (allHeroesSelected) {
            // All heroes have actions, hide UI
            HideActionSelection();
            UpdateCurrentHeroText();
            Debug.Log("BattleManager: All heroes have selected actions!");
        } else {
            // Find next hero that needs an action (start from beginning to find first without action)
            currentHeroIndex = 0;
            while (currentHeroIndex < party.heroes.Count) {
                if (party.heroes[currentHeroIndex].IsAlive() && 
                    !heroActions.Exists(a => a.user == party.heroes[currentHeroIndex])) {
                    // Found a hero that needs an action
                    Debug.Log($"BattleManager: Next hero needing action: {party.heroes[currentHeroIndex].GetType().Name} (index {currentHeroIndex})");
                    ShowHeroSelection();
                    UpdateCurrentHeroText();
                    return;
                }
                currentHeroIndex++;
            }
            
            // If we get here, all heroes should have actions
            CheckAllHeroesSelected();
            if (allHeroesSelected) {
                HideActionSelection();
                Debug.Log("BattleManager: All heroes have selected actions (after search)!");
            }
            UpdateCurrentHeroText();
        }
    }

    void OnTargetSelected(int targetIndex) {
        // Target selection handled in OnConfirmSelection
    }

    void OnConfirmSelection() {
        // This method is now only used for actions that don't need targets (Potion, Check)
        // Target selection is handled by SelectTarget() when a target button is clicked
        if (currentHeroSelecting == null || pendingActionType == ActionType.None) return;
        
        Entity target = null;
        
        // For actions that don't need targets, target can be null
        if (pendingActionType != ActionType.Potion && pendingActionType != ActionType.Check) {
            // This shouldn't happen - target selection should use SelectTarget()
            Debug.LogWarning($"BattleManager: OnConfirmSelection called for action {pendingActionType} that requires target. This should use SelectTarget() instead.");
            return;
        }
        
        // Create action
        string desc = "";
        if (pendingActionType == ActionType.Check) desc = "check enemy";
        else if (pendingActionType == ActionType.Potion) desc = "uses a potion";
        
        Action action = new Action(currentHeroSelecting, pendingActionType, target, desc);
        heroActions.Add(action);
        
        Hero completedHero = currentHeroSelecting;
        Debug.Log($"{completedHero.GetType().Name} selected: {desc}" + (target != null ? $" on {target.GetType().Name}" : ""));
        
        // Reset UI and hero selection state
        HideActionSelection();
        pendingActionType = ActionType.None;
        currentHeroSelecting = null;
        
        // Check if all heroes have actions now
        CheckAllHeroesSelected();
        
        if (allHeroesSelected) {
            // All heroes have actions
            UpdateCurrentHeroText();
            Debug.Log("BattleManager: All heroes have selected actions (from OnConfirmSelection)!");
        } else {
            // Find next hero that needs an action (start from beginning to find first without action)
            currentHeroIndex = 0;
            while (currentHeroIndex < party.heroes.Count) {
                if (party.heroes[currentHeroIndex].IsAlive() && 
                    !heroActions.Exists(a => a.user == party.heroes[currentHeroIndex])) {
                    // Found a hero that needs an action
                    Debug.Log($"BattleManager: Next hero needing action: {party.heroes[currentHeroIndex].GetType().Name} (index {currentHeroIndex})");
                    ShowHeroSelection();
                    UpdateCurrentHeroText();
                    return;
                }
                currentHeroIndex++;
            }
            
            // If we get here, all heroes should have actions
            CheckAllHeroesSelected();
            UpdateCurrentHeroText();
        }
    }

    void CheckAllHeroesSelected() {
        int aliveHeroCount = 0;
        int selectedCount = 0;
        
        foreach (Hero h in party.heroes) {
            if (h.IsAlive()) {
                aliveHeroCount++;
                if (heroActions.Exists(a => a.user == h)) {
                    selectedCount++;
                }
            }
        }
        
        allHeroesSelected = (selectedCount == aliveHeroCount && aliveHeroCount > 0);
        
        if (allHeroesSelected && currentHeroText) {
            currentHeroText.text = "All heroes ready!";
        }
    }

    void UpdateCurrentHeroText() {
        if (currentHeroText == null) return;
        
        if (currentHeroSelecting != null) {
            currentHeroText.text = $"Selecting action for: {currentHeroSelecting.GetType().Name}";
        } else {
            int selected = heroActions.Count;
            int total = 0;
            foreach (Hero h in party.heroes) {
                if (h.IsAlive()) total++;
            }
            currentHeroText.text = $"Select actions ({selected}/{total} heroes ready)";
        }
    }

    void ResolveAction(Action a) {
        if (a.user == null || !a.user.IsAlive()) return;
        if ((a.type == ActionType.Attack || a.type == ActionType.Ability1 || a.type == ActionType.Ability2)
            && (a.target == null || !a.target.IsAlive())) return;

        switch (a.type) {
            case ActionType.Attack:
                a.user.Attack(a.target);
                break;

            case ActionType.Ability1:
                if (a.user is Hero) {
                    UseAbility1(a.user, a.target);
                }
                else if (a.user is Goblin g) g.Ability(a.target);
                else if (a.user is Orc o) o.Ability(a.target);
                else if (a.user is Ghoul gh) gh.Ability(a.target);
                else if (a.user is Warlock w) w.Ability(a.target);
                else if (a.user is Dragon d) {
                    // Dragon's Flame Roar is AOE - flash all heroes after damage
                    int beforeHP = 0;
                    if (a.target == null && a.description == "Flame Roar") {
                        // AOE attack - track all heroes' HP before
                        foreach (Hero h in party.heroes) {
                            if (h.IsAlive()) {
                                beforeHP = h.GetHealth();
                            }
                        }
                    }
                    d.Ability(a.target, party);
                    // Flash all heroes if it was Flame Roar
                    if (a.target == null && a.description == "Flame Roar") {
                        foreach (Hero h in party.heroes) {
                            if (h.IsAlive()) {
                                FlashEntity(h);
                            }
                        }
                    }
                }
                break;

            case ActionType.Ability2:
                if (a.user is Hero)
                    UseAbility2(a.user, a.target);
                break;

            case ActionType.Check:
                if (battleLogText) {
                    if (a.target != null && a.target.IsAlive()) {
                        battleLogText.text += "Scan result:\n";
                        battleLogText.text += "Enemy HP: " + a.target.GetHealth()
                            + ", Atk: " + a.target.GetAttack()
                            + ", Def: " + a.target.GetDefense() + "\n";
                    } else {
                        battleLogText.text += "No valid target to scan.\n";
                    }
                }
                break;

            case ActionType.Potion:
                if (party.potions > 0) {
                    party.potions--;
                    a.user.ModifyHealth(+3);
                }
                break;
        }
    }

    void UseAbility1(Entity user, Entity target) {
        if (user is Warrior w) w.Battlecry();
        else if (user is Mage m) m.Shield();
        else if (user is Thief t) t.Pierce(target);
        else if (user is BlinkerBell b) b.Fairyduster(target);
        else if (user is Blandroid d) d.Overdrive(target);
        else if (user is Jachariah j) j.PiercingShot(target);
    }

    void UseAbility2(Entity user, Entity target) {
        if (user is Warrior w) w.Brutalize(target);
        else if (user is Mage m) m.Blast(target);
        else if (user is Thief t) t.Trick(target);
        else if (user is BlinkerBell b) b.Blaze(target);
        else if (user is Blandroid d) d.Brutalize(target);
        else if (user is Jachariah j) j.Multishot(target);
    }

    void UpdateUI() {
        // Update hero status
        if (heroStatusText != null) {
            heroStatusText.text = "Heroes:\n";
            foreach (Hero h in party.heroes) {
                if (h.IsAlive())
                    heroStatusText.text += h.GetType().Name + ": HP:" + h.GetHealth() + " EP:" + h.GetEnergy()
                        + " Atk:" + h.GetAttack() + " Def:" + h.GetDefense() + "\n";
                else
                    heroStatusText.text += h.GetType().Name + ": DEAD\n";
            }
            heroStatusText.text += "Potions: " + party.potions;
        }

        // Update enemy status
        if (enemyStatusText != null) {
            enemyStatusText.text = "Enemies:\n";
            foreach (Enemy e in enemyGroup.enemies) {
                if (e.IsAlive())
                    enemyStatusText.text += e.GetName() + ": HP:" + e.GetHealth() + " Atk:" + e.GetAttack()
                        + " Def:" + e.GetDefense() + "\n";
            }
        }
    }
    
    // Helper class for action results
    class ActionResult {
        public Action action;
        public int damageDealt;
    }
}
