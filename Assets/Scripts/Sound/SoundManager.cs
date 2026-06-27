using System;
using SpaceP.Scoring;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private const int SOUND_VOLUME_MAX = 10;
    private const float WIND_SOUND_COOLDOWN = 0.5f;

    public static SoundManager Instance { get; private set; }
    private static int soundVolume = 6;

    public event EventHandler OnSoundVolumeChange;


    [SerializeField] private AudioClip fuelPickUpAudioClip;
    [SerializeField] private AudioClip coinPickUpAudioClip;
    [SerializeField] private AudioClip WindAudioClip;
    [SerializeField] private AudioClip landedSuccessfullAudioClip;
    [SerializeField] private AudioClip crashAudioClip;

    private float lastWindSoundTime = -WIND_SOUND_COOLDOWN;
    private Camera cachedCamera;
    private bool isSubscribedToPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        cachedCamera = Camera.main;
    }

    private void Start()
    {
        SubscribeToPlayerEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromPlayerEvents();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void HandleLandingResult(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        AudioClip clip = e.Result.Type == LandingType.Success
            ? landedSuccessfullAudioClip
            : crashAudioClip;

        PlayClip(clip);
    }

    private void HandleFuelPickup(object sender, EventArgs e)
    {
        PlayClip(fuelPickUpAudioClip);
    }

    private void HandleCoinPickup(object sender, EventArgs e)
    {
        PlayClip(coinPickUpAudioClip);
    }

    private void HandleWindForce(object sender, EventArgs e)
    {
        if (Time.time < lastWindSoundTime + WIND_SOUND_COOLDOWN)
        {
            return;
        }

        lastWindSoundTime = Time.time;
        PlayClip(WindAudioClip);
    }

    public void ChangeSoundVolume()
    {
        soundVolume = (soundVolume + 1) % SOUND_VOLUME_MAX;
        OnSoundVolumeChange?.Invoke(this, EventArgs.Empty);
    }

    public int GetSoundVolume()
    {
        return soundVolume;
    }

    public float GetSoundVolumeNormalized()
    {
        return Mathf.Clamp01((float)soundVolume / SOUND_VOLUME_MAX);
    }

    public void PlayClip(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        Vector3 position = cachedCamera != null ? cachedCamera.transform.position : Vector3.zero;
        AudioSource.PlayClipAtPoint(clip, position, GetSoundVolumeNormalized());
    }

    private void SubscribeToPlayerEvents()
    {
        if (isSubscribedToPlayer || PlayerMovement.Instance == null)
        {
            return;
        }

        PlayerMovement.Instance.onCoinPickUp += HandleCoinPickup;
        PlayerMovement.Instance.onFuelPickUp += HandleFuelPickup;
        PlayerMovement.Instance.onWindForce += HandleWindForce;
        PlayerMovement.Instance.onLanded += HandleLandingResult;
        isSubscribedToPlayer = true;
    }

    private void UnsubscribeFromPlayerEvents()
    {
        if (!isSubscribedToPlayer || PlayerMovement.Instance == null)
        {
            isSubscribedToPlayer = false;
            return;
        }

        PlayerMovement.Instance.onCoinPickUp -= HandleCoinPickup;
        PlayerMovement.Instance.onFuelPickUp -= HandleFuelPickup;
        PlayerMovement.Instance.onWindForce -= HandleWindForce;
        PlayerMovement.Instance.onLanded -= HandleLandingResult;
        isSubscribedToPlayer = false;
    }
}
