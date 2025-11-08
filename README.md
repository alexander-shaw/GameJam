# Unity 2D WebGL — Game Jam Starter (12‑Hour Sprint)

This repo layout is optimized for **speed** and **clarity** when shipping a **web‑based (WebGL)** 2D game in a game jam/hackathon.

> Target: Unity 2021+ or 2022/2023 LTS, URP optional, build to **WebGL**.

## Folder Structure

```
Unity2D_WebGL_GameJam_Starter/
├─ Assets/
│  ├─ _Project/
│  │  ├─ Art/
│  │  │  ├─ Characters/
│  │  │  ├─ Tilesets/
│  │  │  ├─ Props/
│  │  │  └─ FX/
│  │  ├─ Audio/
│  │  │  ├─ SFX/
│  │  │  └─ Music/
│  │  ├─ Fonts/
│  │  ├─ Materials/
│  │  ├─ Prefabs/
│  │  ├─ Scenes/  ← Put `Boot.unity`, `MainMenu.unity`, `Game.unity` here
│  │  ├─ Scripts/
│  │  │  ├─ Gameplay/  ← PlayerController.cs, EnemyAI.cs, Collectible.cs
│  │  │  ├─ Systems/  ← GameManager.cs, AudioManager.cs, SaveSystem.cs
│  │  │  └─ UI/  ← UIManager.cs, MenuController.cs
│  │  ├─ Shaders/
│  │  ├─ Sprites/
│  │  ├─ UI/
│  │  ├─ Animation/
│  │  ├─ ScriptableObjects/  ← GameConfig.asset, EnemyStats.asset, etc.
│  │  └─ StreamingAssets/
│  ├─ Settings/
│  └─ ThirdParty/
├─ Builds/
│  └─ WebGL/  ← Export your WebGL build here (contains index.html & Build/)
├─ Docs/  ← Screenshots, GIFs, notes for your jam page
├─ Packages/  ← Managed by Unity
├─ ProjectSettings/  ← Managed by Unity
└─ .gitignore
```

## Quick Start (do this now)

1. **Open in Unity Hub** → *Add project from disk* → select this folder.
2. In **Scenes**, create:
   - `Boot.unity` (loads managers and then MainMenu)
   - `MainMenu.unity` (Play/Quit buttons)
   - `Game.unity` (actual gameplay scene)
3. Create these **core scripts** (place in `Assets/_Project/Scripts/Systems` and `Gameplay`):
   - `GameManager.cs` (singleton; handles state, scene loads)
   - `AudioManager.cs` (one‑shot SFX + looped music)
   - `PlayerController.cs` (2D movement)
4. Set **Project Settings → Player → Resolution and Presentation** for WebGL:
   - Default canvas width/height (e.g., 960×540 or 1280×720)
   - Run In Background: **On**
   - WebGL Compression: **Gzip** or **Brotli**
5. Set **File → Build Settings**:
   - Platform: **WebGL** → *Switch Platform*
   - Scenes in Build: add `Boot`, `MainMenu`, `Game` (in this order)
   - Click **Player Settings…** and ensure **WebGL Memory** is reasonable (e.g., 256–512 MB).
6. **Build** to `Builds/WebGL/`. You'll get `index.html` and a `Build/` folder.

## Minimal Script Stubs (drop‑in)

**GameManager.cs**
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap() => new GameObject("GameManager").AddComponent<GameManager>();

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
    }

    public void Play() => SceneManager.LoadScene("Game");
    public void ToMenu() => SceneManager.LoadScene("MainMenu");
}
```

**PlayerController.cs** (top‑down or side‑scroll; simple 2D movement)
```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float speed = 6f;
    Rigidbody2D rb;
    Vector2 input;

    void Awake() => rb = GetComponent<Rigidbody2D>();
    void Update() => input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    void FixedUpdate() => rb.velocity = input * speed;
}
```

**AudioManager.cs**
```csharp
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }
    public AudioSource musicSource;
    public AudioSource sfxSource;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
    }

    public void PlaySFX(AudioClip clip) { if (clip) sfxSource.PlayOneShot(clip); }
    public void PlayMusic(AudioClip clip, bool loop=true)
    {
        if (!clip) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }
}
```

## WebGL Deploy (fast)

- **itch.io**: Drag the **contents** of `Builds/WebGL/` into the "Upload" area → mark as "This file is a HTML game."  
- **GitHub Pages**: Create a branch (e.g., `gh-pages`) and push the **contents** of `Builds/WebGL/` to the root of that branch. Enable Pages → deploy from `gh-pages`.

## Jam Checklist (12‑hour scope)

- [ ] Core loop: move, avoid/collect, win/lose condition
- [ ] One enemy type, one collectible
- [ ] Main menu, pause, game over
- [ ] Music + 3–5 SFX
- [ ] One tileset, one player sprite, basic UI
- [ ] WebGL build tested on desktop & mobile
- [ ] README + screenshots in `Docs/`

## Tips to Ship on Time

- Keep art simple (flat colors, 16×16 or 32×32 tiles/sprites).
- Use prefabs aggressively (player, enemy, pickups).
- Avoid heavy plugins; keep *ThirdParty* empty unless vital.
- Profile WebGL early; disable vsync if needed, cap to 60 FPS.
- If physics heavy, reduce Fixed Timestep (0.02→0.033) to save CPU.

---
