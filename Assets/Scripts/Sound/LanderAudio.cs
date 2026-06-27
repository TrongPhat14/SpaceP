using UnityEngine;

public class LanderAudio : MonoBehaviour
{
    [SerializeField] private AudioSource thrusterAudioSource;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        if (playerMovement == null)
        {
            return;
        }

        playerMovement.onBeforeForce += HandleBeforeForce;
        playerMovement.onUpForce += HandleThrusterForce;
        playerMovement.onLeftForce += HandleThrusterForce;
        playerMovement.onRightForce += HandleThrusterForce;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.OnSoundVolumeChange += HandleSoundVolumeChanged;
        }

        RefreshVolume();
        StopThrusterLoop();
    }

    private void OnDisable()
    {
        if (playerMovement != null)
        {
            playerMovement.onBeforeForce -= HandleBeforeForce;
            playerMovement.onUpForce -= HandleThrusterForce;
            playerMovement.onLeftForce -= HandleThrusterForce;
            playerMovement.onRightForce -= HandleThrusterForce;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.OnSoundVolumeChange -= HandleSoundVolumeChanged;
        }

        StopThrusterLoop();
    }

    private void HandleSoundVolumeChanged(object sender, System.EventArgs e)
    {
        RefreshVolume();
    }

    private void HandleBeforeForce(object sender, System.EventArgs e)
    {
        StopThrusterLoop();
    }

    private void HandleThrusterForce(object sender, System.EventArgs e)
    {
        if (thrusterAudioSource == null || thrusterAudioSource.isPlaying)
        {
            return;
        }

        thrusterAudioSource.Play();
    }

    private void RefreshVolume()
    {
        if (thrusterAudioSource == null || SoundManager.Instance == null)
        {
            return;
        }

        thrusterAudioSource.volume = SoundManager.Instance.GetSoundVolumeNormalized();
    }

    private void StopThrusterLoop()
    {
        if (thrusterAudioSource == null)
        {
            return;
        }

        thrusterAudioSource.Pause();
    }
}
