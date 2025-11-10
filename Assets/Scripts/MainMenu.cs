using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour {
    public ScreenFade fader;
    public string sceneName = "WorldScene";

    public void PlayGame() {
        StartCoroutine(TransitionToScene());
    }

    private IEnumerator TransitionToScene() {
        if (fader == null) {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        var async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
            yield return null;

        // fade out current UI
        yield return fader.StartCoroutine(fader.FadeOut());

        // Create a persistent fade overlay that will survive scene change
        // This ensures the new scene is covered when it loads
        GameObject persistentFadeObject = new GameObject("PersistentScreenFade");
        DontDestroyOnLoad(persistentFadeObject);
        
        Canvas canvas = persistentFadeObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Make sure it's on top
        
        CanvasGroup canvasGroup = persistentFadeObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f; // Start opaque to cover the new scene
        canvasGroup.blocksRaycasts = true;
        
        // Add a full-screen image to block the view
        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(persistentFadeObject.transform, false);
        UnityEngine.UI.Image image = imageObject.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.black;
        
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        ScreenFade persistentFader = persistentFadeObject.AddComponent<ScreenFade>();
        persistentFader.canvasGroup = canvasGroup;
        persistentFader.fadeDuration = fader.fadeDuration; // Match the fade duration
        
        // Now activate the new scene (it will be hidden behind the black fade)
        async.allowSceneActivation = true;
        
        // Wait for the new scene to fully load and initialize
        yield return null;
        yield return null;
        
        // Fade in the new scene
        yield return persistentFader.StartCoroutine(persistentFader.FadeIn());
        
        // Clean up the persistent fade object after fading in
        Destroy(persistentFadeObject);
    }
}