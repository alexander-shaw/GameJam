# Time Collapse - Unity 2D Game

A top-down 2D survival game built with Unity. Survive waves of enemies, collect time crystals to restore health, and achieve the highest score possible.

## Project Structure

```
Assets/_Project/
├── Scripts/
│   ├── Editor/          # Editor-only scripts for scene setup
│   ├── Gameplay/        # Core gameplay scripts
│   ├── Systems/         # Game systems (GameManager, AudioManager)
│   └── UI/              # User interface controllers
├── Prefabs/            # Reusable game object templates
├── Scenes/             # Game scenes (MainMenu, Game, Credits)
├── Fonts/              # Custom fonts
├── Art/                # Art assets (Characters, Tilesets, Props, FX)
├── Audio/              # Audio assets (SFX, Music)
└── Materials/          # Material assets
```

---

## Scripts

### Editor Scripts

#### `Assets/_Project/Scripts/Editor/GameSetup.cs`
**Purpose**: Unity Editor tool for automated scene and prefab setup. Provides menu items under `Tools → Time Collapse`.

**Key Features**:
- **Setup All Scenes**: Creates MainMenu, Credits, and Game scenes with all necessary components
- **Create All Prefabs**: Generates Player, Enemy, Projectile, and TimeCrystal prefabs
- **Automatic Prefab Assignment**: Assigns prefab references to prevent them from resetting after scene regeneration
- **Font Integration**: Automatically assigns custom font to all TextMeshPro UI elements
- **EventSystem Creation**: Ensures UI buttons work by creating EventSystem in all scenes
- **Fix Prefab Sprites**: Tool to fix missing sprites on prefabs

**Menu Items**:
- `Tools/Time Collapse/Setup All Scenes` - Creates all game scenes
- `Tools/Time Collapse/Setup Main Menu Scene` - Creates main menu
- `Tools/Time Collapse/Setup Credits Scene` - Creates credits screen
- `Tools/Time Collapse/Setup Game Scene` - Creates gameplay scene
- `Tools/Time Collapse/Create All Prefabs` - Creates all prefabs
- `Tools/Time Collapse/Fix Prefab Sprites` - Fixes missing sprites
- `Tools/Time Collapse/Configure Physics2D Collision Matrix` - Instructions for collision setup
- `Tools/Time Collapse/Convert Font to TMP Font Asset` - Guides font conversion

---

### Systems Scripts

#### `Assets/_Project/Scripts/Systems/GameManager.cs`
**Purpose**: Core game logic and state management. Singleton pattern ensures one instance persists across scenes.

**Key Features**:
- **Stability (Health) System**: Manages player health (called "stability")
- **Scoring System**: Tracks score based on enemies killed (10 points per kill)
- **Game State Management**: Tracks game over, pause states
- **Scene Management**: Handles transitions between MainMenu, Game, and Credits
- **Event System**: Publishes events for UI updates (OnStabilityChanged, OnScoreChanged, OnGameOver)

**Public Properties**:
- `EnemiesKilled` - Number of enemies killed
- `CurrentStability` - Current health value
- `maxStability` - Maximum health (default: 100)
- `isGameOver` - Whether game is over
- `isPaused` - Whether game is paused

**Key Methods**:
- `StartNewGame()` - Initializes and starts a new game
- `TakeDamage(float amount)` - Reduces stability
- `RestoreStability(float amount)` - Increases stability
- `OnEnemyKilled()` - Called when enemy dies, updates score
- `GameOver()` - Ends game and returns to main menu
- `Play()` - Loads Game scene
- `ToMenu()` - Loads MainMenu scene

**Game Over Condition**: Game ends when health equals exactly 0 (not less than 0).

#### `Assets/_Project/Scripts/Systems/AudioManager.cs`
**Purpose**: Manages audio playback for music and sound effects. Singleton pattern.

**Key Features**:
- **Music Source**: Plays background music (looped)
- **SFX Source**: Plays one-shot sound effects
- **Persistent**: Survives scene changes (DontDestroyOnLoad)

**Key Methods**:
- `PlaySFX(AudioClip clip)` - Plays a sound effect
- `PlayMusic(AudioClip clip, bool loop)` - Plays background music

---

### Gameplay Scripts

#### `Assets/_Project/Scripts/Gameplay/PlayerController.cs`
**Purpose**: Controls player movement, shooting, and damage handling.

**Key Features**:
- **Top-Down Movement**: WASD/Arrow keys control movement
- **Mouse Shooting**: Left-click shoots projectiles toward mouse cursor
- **Invincibility Frames**: Brief invincibility after taking damage
- **Continuous Health Drain**: Health drains continuously while enemies are touching (20% of max health per second)
- **Enemy Contact Tracking**: Tracks which enemies are currently touching the player

**Key Properties**:
- `speed` - Movement speed (default: 6)
- `projectilePrefab` - Prefab for projectiles
- `shootCooldown` - Time between shots (default: 0.3s)
- `projectileSpeed` - Speed of projectiles (default: 10)
- `invincibilityDuration` - How long invincibility lasts (default: 1s)
- `healthDrainRate` - Health drain rate while touching enemies (default: 0.2 = 20% per second)
- `isInvincible` - Whether player is currently invincible

**Key Methods**:
- `TakeDamage()` - Triggers invincibility frames
- `RegisterTouchingEnemy(EnemyAI enemy)` - Registers enemy contact
- `UnregisterTouchingEnemy(EnemyAI enemy)` - Unregisters enemy contact

**Movement Fix**: Uses Rigidbody2D with `linearDamping = 0`, `collisionDetectionMode = Continuous`, and `interpolation = Interpolate` to prevent jittering.

#### `Assets/_Project/Scripts/Gameplay/EnemyAI.cs`
**Purpose**: Controls enemy behavior - chasing player, dealing damage, dropping items.

**Key Features**:
- **AI Movement**: Moves toward player using Rigidbody2D
- **Damage on Contact**: Deals damage when colliding with player (15 damage)
- **Continuous Contact Tracking**: Registers/unregisters contact with player for health drain
- **Death Behavior**: Drops TimeCrystal when killed, notifies GameManager

**Key Properties**:
- `speed` - Movement speed (default: 3)
- `health` - Enemy health (default: 2)
- `damage` - Damage dealt to player (default: 15)
- `timeCrystalPrefab` - Prefab for dropped collectible

**Key Methods**:
- `TakeDamage(float amount)` - Reduces enemy health
- `Die()` - Handles enemy death (drops crystal, notifies GameManager)

**Collision Handlers**:
- `OnCollisionEnter2D` - Initial contact, applies damage if player not invincible
- `OnCollisionStay2D` - Maintains contact registration
- `OnCollisionExit2D` - Unregisters contact when enemy leaves
- `OnDestroy` - Cleanup: unregisters contact if destroyed while touching

#### `Assets/_Project/Scripts/Gameplay/EnemySpawner.cs`
**Purpose**: Spawns enemies at regular intervals around the player.

**Key Features**:
- **Automatic Spawning**: Spawns enemies at edges of camera view
- **Difficulty Scaling**: Spawn rate increases (spawns faster) as more enemies are killed
- **Max Enemy Limit**: Prevents too many enemies from spawning at once
- **Edge Spawning**: Spawns enemies at random angles around camera center

**Key Properties**:
- `enemyPrefab` - Prefab for enemies (must be assigned)
- `spawnRate` - Base time between spawns (default: 2s)
- `maxEnemies` - Maximum enemies at once (default: 20)
- `spawnDistance` - Distance from camera to spawn (default: 8)
- `difficultyIncreaseRate` - How much spawn rate decreases per kill (default: 0.05s)
- `minSpawnRate` - Minimum spawn rate (default: 0.5s)

**Difficulty**: Spawn rate = `spawnRate - (kills * difficultyIncreaseRate)`, clamped to `minSpawnRate`.

#### `Assets/_Project/Scripts/Gameplay/Projectile.cs`
**Purpose**: Player's projectile that damages enemies.

**Key Features**:
- **Lifetime**: Destroys itself after 5 seconds
- **Damage**: Deals 1 damage to enemies on contact
- **Trigger Collision**: Uses trigger collider to detect hits

**Key Properties**:
- `damage` - Damage dealt (default: 1)
- `lifetime` - Time before auto-destroy (default: 5s)

#### `Assets/_Project/Scripts/Gameplay/TimeCrystal.cs`
**Purpose**: Collectible item that restores player health.

**Key Features**:
- **Health Restoration**: Restores 20 stability when collected
- **Rotation Animation**: Rotates continuously
- **Audio**: Plays pickup sound when collected

**Key Properties**:
- `stabilityRestore` - Health restored (default: 20)
- `rotationSpeed` - Rotation speed (default: 90°/s)
- `pickupSound` - Audio clip to play

#### `Assets/_Project/Scripts/Gameplay/BackgroundGenerator.cs`
**Purpose**: Generates background for the game. Currently scaffolded for future procedural generation.

**Key Features**:
- **Default Background**: Creates a large black square background
- **Procedural Generation Scaffold**: Structure in place for future procedural tile generation
- **Configurable**: Supports custom background prefabs and grid-based generation

**Key Properties**:
- `backgroundPrefab` - Optional prefab for background tiles
- `backgroundSize` - Size of background (default: 50)
- `gridSize` - Grid dimensions for procedural generation (default: 5x5)
- `useProceduralGeneration` - Flag to enable procedural generation (not yet implemented)
- `seed` - Random seed for generation

**Current Status**: Creates a simple black square background. Procedural generation methods (`GenerateProceduralTiles()`, `ApplySeed()`) are scaffolded but not implemented.

#### `Assets/_Project/Scripts/Gameplay/CameraFollow.cs`
**Purpose**: Makes camera smoothly follow the player in top-down view.

**Key Features**:
- **Smooth Following**: Uses Lerp for smooth camera movement
- **Auto-Find Player**: Automatically finds player by tag if target not assigned
- **Orthographic**: Sets camera to orthographic mode for 2D

**Key Properties**:
- `target` - Transform to follow (Player)
- `smoothSpeed` - Smoothing factor (default: 0.125)
- `offset` - Camera offset from target (default: (0, 0, -10))

---

### Entity System Scripts (C++ to C# Conversion)

These scripts implement an entity-component system converted from C++ code. They are currently separate from the main gameplay but can be integrated in the future.

#### `Assets/_Project/Scripts/Gameplay/Entity.cs`
**Purpose**: Base class for all game entities with health, energy, defense, and damage stats.

**Key Features**:
- **Base Stats**: Defines base health, energy, defense, damage
- **Current Stats**: Tracks current values that can change during gameplay
- **Stat Modifiers**: Methods to modify stats (ModHealth, ModEnergy, ModAttack, ModDefense)
- **Damage System**: TakeDamage method accounts for defense

**Key Methods**:
- `Reset()` - Resets to base stats
- `TakeDamage(int amount)` - Takes damage (reduced by defense)
- `ModHealth(int amount)` - Modifies health (healing or damage)
- `ModEnergy(int amount)` - Modifies energy
- `ModAttack(int amount)` - Modifies attack damage
- `ModDefense(int amount)` - Modifies defense
- `IsAlive()` - Returns true if health > 0

#### `Assets/_Project/Scripts/Gameplay/Hero.cs`
**Purpose**: Base class for hero entities, extends Entity.

**Key Features**:
- **Hero Name**: Stores hero's name
- **Basic Attack**: Generic attack method that costs energy

**Key Methods**:
- `Attack(Entity target)` - Basic attack on target (costs 1 energy)

#### `Assets/_Project/Scripts/Gameplay/Warrior.cs`
**Purpose**: Warrior hero subclass - high health, medium damage, good defense.

**Stats**: Health: 10, Damage: 5, Defense: 3, Energy: 3

**Special Abilities**:
- `Battlecry()` - Increases attack by 1, costs 1 energy
- `Brutalize(Entity enemy)` - Attacks enemy and reduces their defense by 1, costs 1 energy

#### `Assets/_Project/Scripts/Gameplay/Mage.cs`
**Purpose**: Mage hero subclass - low health, medium damage, low defense, high energy.

**Stats**: Health: 6, Damage: 3, Defense: 1, Energy: 5

**Special Abilities**:
- `Shield()` - Increases defense by 2, costs 1 energy
- `Blast(Entity target)` - Deals double damage to target, costs 1 energy

#### `Assets/_Project/Scripts/Gameplay/Thief.cs`
**Purpose**: Thief hero subclass - medium health, medium damage, medium defense, high energy.

**Stats**: Health: 8, Damage: 4, Defense: 2, Energy: 5

**Special Abilities**:
- `Pierce(Entity target)` - Deals damage directly to health (ignores defense), costs 1 energy
- `Trick(Entity target)` - Reduces target's defense by 1 and increases own defense by 1, costs 1 energy

#### `Assets/_Project/Scripts/Gameplay/EnemyEntity.cs`
**Purpose**: Enemy entity class, extends Entity.

**Key Features**:
- **Enemy Name**: Stores enemy's name
- **Factory Method**: `CreateBasicEnemy()` creates a basic enemy with default stats

**Default Stats**: Health: 5, Damage: 3, Defense: 1, Energy: 2

---

### UI Scripts

#### `Assets/_Project/Scripts/UI/GameHUD.cs`
**Purpose**: Manages in-game UI display (score and health).

**Key Features**:
- **Status Display**: Shows "score: NUM | health: NUM" in top-left corner
- **Color Coding**: Health color changes based on percentage (green > 60%, yellow > 30%, red otherwise)
- **Game Over Panel**: Displays game over screen with restart button
- **Event Subscription**: Subscribes to GameManager events for updates

**Key Properties**:
- `statusText` - TextMeshProUGUI for score/health display
- `gameOverPanel` - Panel shown on game over
- `restartButton` - Button to return to main menu

#### `Assets/_Project/Scripts/UI/MainMenuController.cs`
**Purpose**: Controls main menu buttons and navigation.

**Key Features**:
- **Play Button**: Starts new game
- **Credits Button**: Navigates to credits screen
- **No Quit Button**: Quit button removed (only available on pause screen)

**Key Properties**:
- `playButton` - Button to start game
- `creditsButton` - Button to view credits

#### `Assets/_Project/Scripts/UI/PauseController.cs`
**Purpose**: Handles game pausing and pause menu.

**Key Features**:
- **Pause Toggle**: Pause button or Escape key toggles pause
- **Resume Button**: Resumes game
- **Quit to Menu Button**: Returns to main menu (only quit option in game)
- **Time Scale Control**: Sets Time.timeScale to 0 when paused

**Key Properties**:
- `pausePanel` - Panel shown when paused
- `pauseButton` - Visible pause button in game (top-right)
- `resumeButton` - Button to resume
- `quitButton` - Button to quit to main menu

#### `Assets/_Project/Scripts/UI/CreditsController.cs`
**Purpose**: Controls credits screen.

**Key Features**:
- **Team Name Display**: Shows "Garbage" as team name
- **Back Button**: Returns to main menu

**Key Properties**:
- `backButton` - Button to return to main menu
- `teamNameText` - TextMeshProUGUI for team name

---

## Prefabs

### `Assets/_Project/Prefabs/Player.prefab`
**Purpose**: Player character template.

**Components**:
- SpriteRenderer (white square, 32x32)
- Rigidbody2D (no gravity, continuous collision detection, interpolated)
- CircleCollider2D (radius: 0.4)
- PlayerController script

**Tag**: Player  
**Layer**: Player

### `Assets/_Project/Prefabs/Enemy.prefab`
**Purpose**: Enemy character template.

**Components**:
- SpriteRenderer (red square, 32x32)
- Rigidbody2D (no gravity)
- CircleCollider2D (radius: 0.4)
- EnemyAI script

**Tag**: Enemy

### `Assets/_Project/Prefabs/Projectile.prefab`
**Purpose**: Player's projectile template.

**Components**:
- SpriteRenderer (yellow square, 16x16)
- Rigidbody2D (no gravity)
- CircleCollider2D (radius: 0.2, trigger)
- Projectile script

**Tag**: Projectile  
**Layer**: Projectile

### `Assets/_Project/Prefabs/TimeCrystal.prefab`
**Purpose**: Collectible item template.

**Components**:
- SpriteRenderer (cyan square, 24x24)
- CircleCollider2D (radius: 0.3, trigger)
- TimeCrystal script

**Tag**: Collectible

---

## Scenes

### `Assets/_Project/Scenes/MainMenu.unity`
**Purpose**: Main menu screen.

**Contents**:
- Canvas with title "TIME COLLAPSE"
- Play button
- Credits button
- EventSystem (required for button interaction)
- MainMenuController script

**Navigation**:
- Play → Game scene
- Credits → Credits scene

### `Assets/_Project/Scenes/Game.unity`
**Purpose**: Main gameplay scene.

**Contents**:
- Player (from prefab)
- Camera with CameraFollow script
- BackgroundGenerator
- EnemySpawner
- Canvas with UI:
  - StatusText (score and health)
  - PauseButton (top-right)
  - PausePanel (with Resume and Quit buttons)
  - GameOverPanel (with Restart button)
- GameHUD script
- PauseController script
- AudioManager
- EventSystem

**Game Flow**:
- Player moves and shoots
- Enemies spawn and chase player
- Health drains on contact
- Game ends when health reaches 0 → returns to MainMenu

### `Assets/_Project/Scenes/Credits.unity`
**Purpose**: Credits screen.

**Contents**:
- Canvas with team name "Garbage" (centered)
- Back button
- EventSystem (required for button interaction)
- CreditsController script

**Navigation**:
- Back → MainMenu scene

---

## Assets

### Fonts

#### `Assets/_Project/Fonts/Font.ttf`
**Purpose**: Custom font file for the game.

**Usage**: Must be converted to TextMeshPro Font Asset (use `Tools → Time Collapse → Convert Font to TMP Font Asset` for instructions). Once converted, it's automatically assigned to all UI text elements.

#### `Assets/_Project/Fonts/Font SDF.asset`
**Purpose**: TextMeshPro Font Asset created from Font.ttf.

**Usage**: Automatically assigned to all TextMeshProUGUI elements by GameSetup.cs.

---

## Game Mechanics

### Health System
- **Terminology**: Called "Stability" in code
- **Max Health**: 100 (configurable in GameManager)
- **Damage Sources**:
  - Initial collision with enemy: 15 damage
  - Continuous contact: 20% of max health per second (20 health/second if max is 100)
- **Healing**: TimeCrystal restores 20 stability
- **Game Over**: When health equals exactly 0

### Scoring System
- **Points per Kill**: 10 points per enemy killed
- **Formula**: `score = enemiesKilled * 10`
- **Display**: Shown as "score: NUM | health: NUM" in top-left corner

### Enemy Spawning
- **Base Spawn Rate**: 2 seconds between spawns
- **Difficulty Scaling**: Spawn rate decreases (spawns faster) as more enemies are killed
- **Formula**: `spawnRate = baseRate - (kills * 0.05)`, minimum 0.5 seconds
- **Max Enemies**: 20 enemies at once
- **Spawn Location**: Random edge position around camera (8 units from center)

### Player Controls
- **Movement**: WASD or Arrow Keys
- **Shooting**: Left Mouse Button (shoots toward cursor)
- **Pause**: Pause Button (top-right) or Escape key

---

## Setup Instructions

### Initial Setup

1. **Open Project in Unity**
   - Open Unity Hub
   - Add project from disk → select this folder

2. **Create Prefabs**
   - Go to `Tools → Time Collapse → Create All Prefabs`
   - This creates Player, Enemy, Projectile, and TimeCrystal prefabs

3. **Setup Scenes**
   - Go to `Tools → Time Collapse → Setup All Scenes`
   - This creates MainMenu, Credits, and Game scenes with all components

4. **Configure Physics2D Collision Matrix**
   - Go to `Edit → Project Settings → Physics 2D → Layer Collision Matrix`
   - Uncheck: Projectile layer should NOT collide with Player layer
   - (This prevents projectiles from hitting the player)

5. **Convert Font (Optional)**
   - Go to `Tools → Time Collapse → Convert Font to TMP Font Asset`
   - Follow the instructions to convert Font.ttf to a TMP Font Asset
   - The font will be automatically assigned to all UI text

6. **Configure Build Settings**
   - Go to `File → Build Profiles` (or `File → Build Settings` in older Unity versions)
   - Add scenes: MainMenu, Credits, Game (in that order)
   - Set platform to WebGL (optional)

### Running the Game

1. **Start from Main Menu**
   - Set MainMenu scene as the active scene
   - Press Play in Unity Editor
   - Click "Play" button to start game

2. **In-Game**
   - Move with WASD/Arrow Keys
   - Shoot with Left Mouse Button
   - Pause with Pause Button or Escape
   - Collect TimeCrystals to restore health

3. **Game Over**
   - When health reaches 0, game automatically returns to Main Menu

### Regenerating Scenes

If you need to regenerate scenes (e.g., after code changes):

1. **Run Setup Again**
   - Go to `Tools → Time Collapse → Setup All Scenes`
   - This will recreate all scenes with latest code

2. **Prefab References**
   - Prefab references are automatically assigned by `AssignPrefabReferences()`
   - No manual assignment needed

3. **Note**: Regenerating scenes will overwrite any manual changes to scene objects. Prefab references are preserved.

---

## Dependencies

### Unity Packages (in `Packages/manifest.json`)
- `com.unity.textmeshpro` (3.0.8) - Text rendering
- `com.unity.ugui` (2.0.0) - UI system
- `com.unity.2d.sprite` (1.0.0) - 2D sprites
- Standard Unity modules (Physics2D, Audio, etc.)

---

## Troubleshooting

### Enemies Not Spawning
- **Check**: EnemySpawner has Enemy prefab assigned
- **Fix**: Run `Tools → Time Collapse → Setup All Scenes` to auto-assign
- **Verify**: Enemy prefab has SpriteRenderer with sprite assigned

### UI Buttons Not Working
- **Check**: EventSystem exists in scene
- **Fix**: Run `Tools → Time Collapse → Setup All Scenes` to create EventSystem

### Player Stuck/Jittering
- **Check**: Rigidbody2D settings (should have linearDamping = 0, continuous collision detection)
- **Fix**: Run `Tools → Time Collapse → Setup All Scenes` to apply correct settings

### Prefab References Reset
- **Check**: Prefabs are assigned in Inspector
- **Fix**: Run `Tools → Time Collapse → Setup All Scenes` - it auto-assigns prefabs

### Font Not Showing
- **Check**: Font.ttf converted to TMP Font Asset
- **Fix**: Use `Tools → Time Collapse → Convert Font to TMP Font Asset` for instructions

---

## Future Enhancements

### Procedural Generation
- `BackgroundGenerator.cs` has scaffolded code for procedural tile generation
- Methods `GenerateProceduralTiles()` and `ApplySeed()` are ready for implementation
- Will use Perlin noise or similar for varied backgrounds

### Entity System Integration
- Entity/Hero/Enemy system is implemented but not yet integrated with main gameplay
- Can be used for future hero selection or enemy variety features

---

## Credits

**Team**: Garbage

---

## License

[Add your license here]
