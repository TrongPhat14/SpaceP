using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    private const int MUSIC_VOLUME_MAX = 10;
    private const float FADE_DURATION = 0.15f;

    public static MusicManager Instance { get; private set; }

    private static float musicTime;
    private static int musicVolume = 4;

    public event EventHandler OnMusicVolumeChange;

    private AudioSource musicAudioSource;
    private Tween volumeTween;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        musicAudioSource = GetComponent<AudioSource>();
        RestorePlaybackPosition();
    }

    private void Start()
    {
        ApplyVolume(false);
    }

    private void Update()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicTime = musicAudioSource.time;
        }
    }

    public void ChangeMusicVolume()
    {
        musicVolume = (musicVolume + 1) % MUSIC_VOLUME_MAX;
        ApplyVolume(true);
        OnMusicVolumeChange?.Invoke(this, EventArgs.Empty);
    }

    public int GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetMusicVolumeNormalized()
    {
        return Mathf.Clamp01((float)musicVolume / MUSIC_VOLUME_MAX);
    }

    private void OnDisable()
    {
        volumeTween?.Kill();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void ApplyVolume(bool fade)
    {
        if (musicAudioSource == null)
        {
            return;
        }

        float targetVolume = GetMusicVolumeNormalized();
        volumeTween?.Kill();

        if (!fade)
        {
            musicAudioSource.volume = targetVolume;
            return;
        }

        volumeTween = musicAudioSource
            .DOFade(targetVolume, FADE_DURATION)
            .SetLink(gameObject)
            .SetUpdate(true);
    }

    private void RestorePlaybackPosition()
    {
        if (musicAudioSource == null || musicAudioSource.clip == null || musicTime <= 0f)
        {
            return;
        }

        musicAudioSource.time = Mathf.Repeat(musicTime, musicAudioSource.clip.length);
    }
}
