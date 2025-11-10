using UnityEngine;
using System.Collections;

public class ScreenFade : MonoBehaviour {
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f;
    public bool startOpaqueAndFadeIn = false;

    void Awake() {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null) return;

        // If we want the scene to fade in, start opaque; otherwise start transparent
        if (startOpaqueAndFadeIn) {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        } else {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    IEnumerator Start() {
        // If requested, automatically fade in on scene start
        if (startOpaqueAndFadeIn && canvasGroup != null) {
            yield return StartCoroutine(FadeIn());
        }
    }

    public IEnumerator FadeOut() {
        if (canvasGroup == null) yield break;
        canvasGroup.blocksRaycasts = true;
        float t = 0f;

        while (t < fadeDuration) {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    public IEnumerator FadeIn() {
        if (canvasGroup == null) yield break;
        canvasGroup.blocksRaycasts = false;
        float t = 0f;

        while (t < fadeDuration) {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}