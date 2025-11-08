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


