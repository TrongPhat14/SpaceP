using SpaceP.Scoring;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

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

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        PlayerMovement.Instance.onCoinPickUp += Player_onCoinPickUp;
        PlayerMovement.Instance.onFuelPickUp += Player_onFuelPickUp;
        PlayerMovement.Instance.onWindForce += Player_onWindForce;
        PlayerMovement.Instance.onLanded += Player_onLanded;
    }



    private void Player_onLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        switch (e.Result.Type)
        {
            case LandingType.Success:
                AudioSource.PlayClipAtPoint(landedSuccessfullAudioClip, Camera.main.transform.position, GetSoundVolumeNormalized());
                break;
            default:
                AudioSource.PlayClipAtPoint(crashAudioClip, Camera.main.transform.position, GetSoundVolumeNormalized());
                break;
        }
    }

    private void Player_onFuelPickUp(object sender, System.EventArgs e)
    {
        AudioSource.PlayClipAtPoint(fuelPickUpAudioClip, Camera.main.transform.position, GetSoundVolumeNormalized());
    }

    private void Player_onCoinPickUp(object sender, System.EventArgs e)
    {
        AudioSource.PlayClipAtPoint(coinPickUpAudioClip, Camera.main.transform.position, GetSoundVolumeNormalized());

    }
    private void Player_onWindForce(object sender, EventArgs e)
    {
        if (Time.time < lastWindSoundTime + WIND_SOUND_COOLDOWN)
        {
            return;
        }

        lastWindSoundTime = Time.time;
        AudioSource.PlayClipAtPoint(WindAudioClip, Camera.main.transform.position, GetSoundVolumeNormalized());
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
        return  ((float)soundVolume) / SOUND_VOLUME_MAX;
    }

}
