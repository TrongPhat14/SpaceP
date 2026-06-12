using UnityEngine;

public static class UISoundPlayer
{
    private const int SampleRate = 44100;
    private const float Duration = 0.055f;
    private const float Frequency = 880f;
    private const float DefaultVolume = 0.45f;

    private static AudioSource audioSource;
    private static AudioClip navigationClip;

    public static void PlayNavigation()
    {
        EnsureAudioSource();
        EnsureNavigationClip();

        if (audioSource == null || navigationClip == null)
        {
            return;
        }

        float volume = SoundManager.Instance != null
            ? SoundManager.Instance.GetSoundVolumeNormalized()
            : DefaultVolume;

        audioSource.PlayOneShot(navigationClip, volume);
    }

    public static void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        EnsureAudioSource();

        if (audioSource == null)
        {
            return;
        }

        float volume = SoundManager.Instance != null
            ? SoundManager.Instance.GetSoundVolumeNormalized()
            : DefaultVolume;

        audioSource.PlayOneShot(clip, volume);
    }

    private static void EnsureAudioSource()
    {
        if (audioSource != null)
        {
            return;
        }

        GameObject audioObject = new GameObject("UI Sound Player");
        Object.DontDestroyOnLoad(audioObject);

        audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private static void EnsureNavigationClip()
    {
        if (navigationClip != null)
        {
            return;
        }

        int sampleCount = Mathf.CeilToInt(SampleRate * Duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / SampleRate;
            float progress = (float)i / sampleCount;
            float envelope = Mathf.Sin(progress * Mathf.PI);
            float tone = Mathf.Sin(2f * Mathf.PI * Frequency * time);
            float overtone = Mathf.Sin(2f * Mathf.PI * Frequency * 2f * time) * 0.25f;

            samples[i] = (tone + overtone) * envelope * 0.35f;
        }

        navigationClip = AudioClip.Create("UI_Navigation_Blips", sampleCount, 1, SampleRate, false);
        navigationClip.SetData(samples, 0);
    }
}
