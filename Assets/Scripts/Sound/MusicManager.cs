using System;
using DG.Tweening;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private const int MUSIC_VOLUME_MAX = 10;

    public static MusicManager Instance { get; private set; }

    private static float musicTime;
    private static int musicVolume = 4;

    private event EventHandler OnMusicVolumeChange;

    private AudioSource musicAudioSource;
    private Tween volumeTween;

    private void Awake()
    {
        Instance = this;
        musicAudioSource = GetComponent<AudioSource>();
        musicAudioSource.time = musicTime;
    }
    private void Start()
    {
        musicAudioSource.volume = GetMusicVolumeNormalized();
    }
    private void Update()
    {
        musicTime = musicAudioSource.time;
    }
    public void ChangeMusicVolume()
    {
        musicVolume = (musicVolume + 1) % MUSIC_VOLUME_MAX;
        volumeTween?.Kill();
        volumeTween = musicAudioSource
            .DOFade(GetMusicVolumeNormalized(), 0.15f)
            .SetLink(gameObject)
            .SetUpdate(true);
        OnMusicVolumeChange?.Invoke(this, EventArgs.Empty);
    }

    public int GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetMusicVolumeNormalized()
    {
        return ((float)musicVolume) / MUSIC_VOLUME_MAX;
    }

    private void OnDisable()
    {
        volumeTween?.Kill();
    }
}
