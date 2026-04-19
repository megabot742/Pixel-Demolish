using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : BaseManager<AudioManager>
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Music Player")]
    [SerializeField] private AudioSource musicSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip[] menuMusicClips;
    [SerializeField] private AudioClip[] gameMusicClips;

    private AudioClip[] currentPlaylist;
    private int currentTrackIndex = -1;
    private string lastSceneName = "";

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Kiểm tra AudioSource
        if (musicSource != null)
        {
            musicSource.enabled = true;
            musicSource.playOnAwake = false;
            musicSource.loop = false;  
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        LoadSavedVolumes();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentScene = scene.name;

        // Chỉ phát nhạc nếu Scene thay đổi (nhạc có lập lại)
        if (currentScene != lastSceneName)
        {
            lastSceneName = currentScene;
            PlayMusicForCurrentScene();
        }
    }

    private void LoadSavedVolumes() //Load lại giá trị volume lưu trước đó từ PlayerPrefs
    {
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.8f);
        float sfxVol   = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);

        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
    }
    #region Handle set volume
    public void SetMusicVolume(float normalizedVolume)
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, normalizedVolume);
        audioMixer.SetFloat("Music", Mathf.Log10(Mathf.Max(0.0001f, normalizedVolume)) * 20f);
    }

    public void SetSFXVolume(float normalizedVolume)
    {
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, normalizedVolume);
        audioMixer.SetFloat("SFX", Mathf.Log10(Mathf.Max(0.0001f, normalizedVolume)) * 20f);
    }

    public float GetMusicVolume() => PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.8f);
    public float GetSFXVolume()   => PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);

    private void PlayMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        AudioClip[] newPlaylist = (sceneName == "Menu") ? menuMusicClips : gameMusicClips;

        // If the playlist is identical and is playing → do nothing
        if (newPlaylist == currentPlaylist && musicSource.isPlaying) return;

        currentPlaylist = newPlaylist;

        if (currentPlaylist == null || currentPlaylist.Length == 0)
        {
            //Debug.LogWarning($"[AudioManager] There is no music for the scene: {sceneName}");
            return;
        }

        PlayRandomTrack();
    }
    #endregion
    #region RandomTrack
    private void PlayRandomTrack()
    {
        if (musicSource == null || currentPlaylist == null || currentPlaylist.Length == 0)
            return;

        // Force enabled AudioSource
        musicSource.enabled = true;
        if (!musicSource.gameObject.activeInHierarchy)
        {
            musicSource.gameObject.SetActive(true);
        }

        // Play random track
        int newIndex = Random.Range(0, currentPlaylist.Length);
        while (newIndex == currentTrackIndex && currentPlaylist.Length > 1)
            newIndex = Random.Range(0, currentPlaylist.Length);

        currentTrackIndex = newIndex;
        musicSource.clip = currentPlaylist[currentTrackIndex];
        musicSource.Play();

        //Debug.Log($"[AudioManager] Play: {musicSource.clip.name}");
    }

    private void Update()
    {
        // Chỉ chuyển bài khi nhạc kết thúc
        if (musicSource != null && !musicSource.isPlaying && currentPlaylist != null && currentPlaylist.Length > 0)
        {
            PlayRandomTrack();
        }
    }
    #endregion
    //Setting Panel
    public void UpdateMusicVolumeFromSlider(float value) => SetMusicVolume(value);
    public void UpdateSFXVolumeFromSlider(float value)   => SetSFXVolume(value);
}
