using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Global Sound Effects")]
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;
    public AudioClip missionSuccessSound;
    public AudioClip missionFailSound;

    [Header("Procedural Retro Sounds")]
    public AudioClip bootChimeSound;
    public AudioClip errorSound;
    public AudioClip explosionSound;
    public AudioClip fanfareSound;
    public AudioClip eatSound;
    
    [Header("UI Sounds")]
    public AudioClip windowOpenSound;
    public AudioClip windowCloseSound;
    public AudioClip uiClickSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            
            // Auto-setup AudioSources if missing
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
            }

            // Generate procedural retro sounds
            bootChimeSound = CreateSynthChime();
            errorSound = CreateSynthError();
            explosionSound = CreateSynthExplosion();
            fanfareSound = CreateSynthFanfare();
            eatSound = CreateSynthEat();
            windowOpenSound = CreateSynthWindowOpen();
            windowCloseSound = CreateSynthWindowClose();
            uiClickSound = CreateSynthClick();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private AudioClip CreateSynthChime()
    {
        int samplerate = 44100;
        float duration = 2.5f;
        int sampleCount = Mathf.RoundToInt(samplerate * duration);
        float[] samples = new float[sampleCount];

        // A major chord build-up (classic retro feel)
        float[] frequencies = new float[] { 261.63f, 329.63f, 392.00f, 523.25f, 659.25f };
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / samplerate;
            float val = 0f;
            for (int f = 0; f < frequencies.Length; f++)
            {
                float freq = frequencies[f];
                float delay = f * 0.15f;
                if (t > delay)
                {
                    float noteTime = t - delay;
                    float env = Mathf.Exp(-noteTime * 1.5f) * Mathf.Min(noteTime * 10f, 1f);
                    val += Mathf.Sin(2f * Mathf.PI * freq * noteTime) * env * 0.15f;
                }
            }
            float masterEnv = Mathf.Min(1f, (duration - t) * 2f);
            samples[i] = Mathf.Clamp(val * masterEnv, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SynthChime", sampleCount, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateSynthError()
    {
        int samplerate = 44100;
        float duration = 0.4f;
        int sampleCount = Mathf.RoundToInt(samplerate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / samplerate;
            float env = Mathf.Exp(-t * 8f);
            float val = (Mathf.Sin(2f * Mathf.PI * 150f * t) + Mathf.Sin(2f * Mathf.PI * 155f * t)) * 0.4f * env;
            samples[i] = Mathf.Clamp(val, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SynthError", sampleCount, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateSynthExplosion()
    {
        int samplerate = 44100;
        float duration = 0.8f;
        int sampleCount = Mathf.RoundToInt(samplerate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / samplerate;
            float env = Mathf.Exp(-t * 5f);
            // Low-frequency rumble (50Hz base)
            float rumble = Mathf.Sin(2f * Mathf.PI * 50f * t) * 0.5f;
            // White noise component
            float noise = (Random.value * 2f - 1f) * 0.4f;
            // Combine with decaying envelope
            samples[i] = Mathf.Clamp((rumble + noise) * env, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SynthExplosion", sampleCount, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateSynthFanfare()
    {
        int samplerate = 44100;
        float duration = 1.2f;
        int sampleCount = Mathf.RoundToInt(samplerate * duration);
        float[] samples = new float[sampleCount];

        // C major arpeggio: C4 → E4 → G4 → C5
        float[] frequencies = new float[] { 261.63f, 329.63f, 392.00f, 523.25f };
        float noteGap = 0.15f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / samplerate;
            float val = 0f;
            for (int n = 0; n < frequencies.Length; n++)
            {
                float delay = n * noteGap;
                if (t > delay)
                {
                    float noteT = t - delay;
                    float env = Mathf.Exp(-noteT * 2.5f) * Mathf.Min(noteT * 20f, 1f);
                    val += Mathf.Sin(2f * Mathf.PI * frequencies[n] * noteT) * env * 0.2f;
                    // Add harmonic richness
                    val += Mathf.Sin(2f * Mathf.PI * frequencies[n] * 2f * noteT) * env * 0.05f;
                }
            }
            float masterEnv = Mathf.Min(1f, (duration - t) * 3f);
            samples[i] = Mathf.Clamp(val * masterEnv, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SynthFanfare", sampleCount, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlaySFXWithDuration(AudioClip clip, float duration, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            StartCoroutine(PlaySFXRoutine(clip, duration, volume));
        }
    }

    private System.Collections.IEnumerator PlaySFXRoutine(AudioClip clip, float duration, float volume)
    {
        GameObject tempAudio = new GameObject("TempSFX_" + clip.name);
        tempAudio.transform.SetParent(transform);
        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = sfxSource.volume * volume;
        tempSource.Play();
        yield return new UnityEngine.WaitForSeconds(duration);
        if (tempSource != null)
        {
            tempSource.Stop();
            Destroy(tempAudio);
        }
    }

    private AudioClip CreateSynthEat()
    {
        int samplerate = 44100;
        float duration = 0.1f;
        float[] samples = new float[(int)(samplerate * duration)];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / samplerate;
            samples[i] = Mathf.Sin(t * 2f * Mathf.PI * 800f) * Mathf.Exp(-t * 30f) * 0.4f;
        }
        AudioClip clip = AudioClip.Create("Eat", samples.Length, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateSynthWindowOpen()
    {
        int samplerate = 44100;
        float duration = 0.15f;
        float[] samples = new float[(int)(samplerate * duration)];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / samplerate;
            float freq = Mathf.Lerp(400f, 800f, t / duration);
            samples[i] = Mathf.Sin(t * 2f * Mathf.PI * freq) * Mathf.Exp(-t * 15f) * 0.3f;
        }
        AudioClip clip = AudioClip.Create("WinOpen", samples.Length, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateSynthWindowClose()
    {
        int samplerate = 44100;
        float duration = 0.15f;
        float[] samples = new float[(int)(samplerate * duration)];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / samplerate;
            float freq = Mathf.Lerp(800f, 400f, t / duration);
            samples[i] = Mathf.Sin(t * 2f * Mathf.PI * freq) * Mathf.Exp(-t * 15f) * 0.3f;
        }
        AudioClip clip = AudioClip.Create("WinClose", samples.Length, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateSynthClick()
    {
        int samplerate = 44100;
        float duration = 0.05f;
        float[] samples = new float[(int)(samplerate * duration)];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / samplerate;
            samples[i] = (Random.value * 2f - 1f) * Mathf.Exp(-t * 60f) * 0.2f;
        }
        AudioClip clip = AudioClip.Create("Click", samples.Length, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }
    
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip != null && musicSource != null)
        {
            musicSource.clip = clip;
            musicSource.volume = volume;
            musicSource.Play();
        }
    }
    
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
}
