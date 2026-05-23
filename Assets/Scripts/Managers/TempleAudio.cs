using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TempleAudio : MonoBehaviour
{
    public static TempleAudio Instance { get; private set; }

    const string MainMenuMusicPath = "TempleAudio/Music/Forbidden_Temple_Loop A";
    const string LevelOneMusicPath = "TempleAudio/Music/Call_of_the_Depths_Loop_A";
    const string GameOverMusicPath = "TempleAudio/Music/An_Unwelcome_Presence_Loop_A";
    const string ButtonClickPath = "TempleAudio/SFX/DM-CGS-50";

    AudioSource musicSource;
    AudioSource uiSource;
    readonly HashSet<Button> registeredButtons = new HashSet<Button>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        if (Instance != null) return;

        GameObject obj = new GameObject("TempleAudio");
        DontDestroyOnLoad(obj);
        obj.AddComponent<TempleAudio>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f) * 0.45f;

        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.playOnAwake = false;
        uiSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f) * 0.8f;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
        StartCoroutine(RegisterButtonsAfterSceneBuild());
    }

    IEnumerator RegisterButtonsAfterSceneBuild()
    {
        yield return null;
        yield return null;
        RegisterSceneButtons();
    }

    void PlayMusicForScene(string sceneName)
    {
        string path = null;

        if (sceneName == "MainMenu")
            path = MainMenuMusicPath;
        else if (sceneName == "GameOver")
            path = GameOverMusicPath;
        else if (sceneName == "Level01_Entrance")
            path = LevelOneMusicPath;

        if (string.IsNullOrEmpty(path))
        {
            if (musicSource.isPlaying)
                musicSource.Stop();
            return;
        }

        AudioClip clip = LoadClip(path);
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public static AudioClip LoadClip(string path)
    {
        return Resources.Load<AudioClip>(path);
    }

    public static AudioClip[] LoadClips(params string[] paths)
    {
        List<AudioClip> clips = new List<AudioClip>();
        foreach (string path in paths)
        {
            AudioClip clip = LoadClip(path);
            if (clip != null)
                clips.Add(clip);
        }
        return clips.ToArray();
    }

    public static void RegisterButton(Button button)
    {
        if (Instance == null || button == null) return;
        Instance.RegisterButtonInternal(button);
    }

    public static void PlayButtonClick()
    {
        AudioClip clip = LoadClip(ButtonClickPath);
        PlaySfx(clip, 1f);
    }

    public static void PlaySfx(AudioClip clip, float volume = 1f)
    {
        EnsureInstance();
        if (Instance == null || clip == null) return;
        if (Instance.uiSource != null)
            Instance.uiSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public static void SetMusicVolume(float volume)
    {
        EnsureInstance();
        if (Instance != null && Instance.musicSource != null)
            Instance.musicSource.volume = Mathf.Clamp01(volume) * 0.45f;
    }

    public static void SetSfxVolume(float volume)
    {
        EnsureInstance();
        if (Instance != null && Instance.uiSource != null)
            Instance.uiSource.volume = Mathf.Clamp01(volume) * 0.8f;
    }

    static void EnsureInstance()
    {
        if (Instance != null) return;

        GameObject obj = new GameObject("TempleAudio");
        DontDestroyOnLoad(obj);
        obj.AddComponent<TempleAudio>();
    }

    void RegisterSceneButtons()
    {
        foreach (Button button in FindObjectsOfType<Button>(true))
            RegisterButtonInternal(button);
    }

    void RegisterButtonInternal(Button button)
    {
        if (registeredButtons.Contains(button)) return;

        registeredButtons.Add(button);
        button.onClick.AddListener(PlayButtonClick);
    }
}
