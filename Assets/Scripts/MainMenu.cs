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

        // activate the new scene; if that scene has a ScreenFader set to startOpaqueAndFadeIn,
        // it will run FadeIn automatically on Start
        async.allowSceneActivation = true;
    }
}