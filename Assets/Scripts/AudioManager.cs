using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Optional AudioMixer (recommended)")]
    [Tooltip("Assign a mixer with exposed params MasterVolume / MusicVolume / SFXVolume.")]
    public AudioMixer mixer;

    [Tooltip("Exposed parameter names (must match the mixer exactly).")]
    public string masterParam = "MasterVolume";
    public string musicParam  = "MusicVolume";
    public string sfxParam    = "SFXVolume";

    [Header("Fallback sources (used when no mixer is assigned)")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    float _master = 1f, _music = 0.8f, _sfx = 1f;
    bool  _muted = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
    }

    void Start()
    {

        if (mixer != null)
        {
            var sfxGroups = mixer.FindMatchingGroups("SFX");
            if (sfxGroups != null && sfxGroups.Length > 0 && sfxSource != null)
                sfxSource.outputAudioMixerGroup = sfxGroups[0];

            var musicGroups = mixer.FindMatchingGroups("Music");
            if (musicGroups != null && musicGroups.Length > 0 && musicSource != null)
                musicSource.outputAudioMixerGroup = musicGroups[0];
        }

        if (SettingsManager.Instance != null)
            ApplyVolumes(SettingsManager.Instance.MasterVolume,
                         SettingsManager.Instance.MusicVolume,
                         SettingsManager.Instance.SfxVolume,
                         SettingsManager.Instance.Muted);
        else
            ApplyVolumes(_master, _music, _sfx, _muted);
    }

    public void ApplyVolumes(float master, float music, float sfx, bool muted)
    {
        _master = Mathf.Clamp01(master);
        _music  = Mathf.Clamp01(music);
        _sfx    = Mathf.Clamp01(sfx);
        _muted  = muted;
        Push();
    }

    public void SetMaster(float v) { _master = Mathf.Clamp01(v); Push(); }
    public void SetMusic (float v) { _music  = Mathf.Clamp01(v); Push(); }
    public void SetSfx   (float v) { _sfx    = Mathf.Clamp01(v); Push(); }
    public void SetMuted (bool m)  { _muted  = m; Push(); }

    static float ToDb(float linear) => linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;

    void Push()
    {
        float m = _muted ? 0f : _master;

        if (mixer != null)
        {
            bool ok = true;
            ok &= mixer.SetFloat(masterParam, ToDb(m));
            ok &= mixer.SetFloat(musicParam,  ToDb(_muted ? 0f : _music));
            ok &= mixer.SetFloat(sfxParam,    ToDb(_muted ? 0f : _sfx));

            if (ok) return;
        }

        AudioListener.volume = m;
        if (musicSource != null) musicSource.volume = _muted ? 0f : _music;
        if (sfxSource   != null) sfxSource.volume   = _muted ? 0f : _sfx;
    }

    public void PlayMusic(AudioClip clip) { if (musicSource != null && clip != null) { musicSource.clip = clip; musicSource.loop = true; musicSource.Play(); } }
    public void PlaySfx(AudioClip clip)   { if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip); }

    public AudioMixerGroup SfxGroup => sfxSource != null ? sfxSource.outputAudioMixerGroup : null;
}
