using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class SoundManager : MonoBehaviour
{
    private const int SOUND_VOLUME_MAX = 10;

    public static SoundManager Instance { get; private set; }
    private static int soundVolume = 6;

    public event EventHandler OnSoundVolumeChange;


    [SerializeField] private AudioClip fuelPickUpAudioClip;
    [SerializeField] private AudioClip coinPickUpAudioClip;
    [SerializeField] private AudioClip WindAudioClip;
    [SerializeField] private AudioClip landedSuccessfullAudioClip;
    [SerializeField] private AudioClip crashAudioClip;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        PlayerMovement.instance.onCoinPickUp += Player_onCoinPickUp;
        PlayerMovement.instance.onFuelPickUp += Player_onFuelPickUp;
        PlayerMovement.instance.onWindForce += Player_onWindForce;
        PlayerMovement.instance.onLanded += Player_onLanded;
    }



    private void Player_onLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        switch (e.landingType)
        {
            case PlayerMovement.LandingType.Success:
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
