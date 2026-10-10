using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    const string K_Master  = "set_master";
    const string K_Music   = "set_music";
    const string K_SFX     = "set_sfx";
    const string K_Mute    = "set_mute";
    const string K_AimDead = "set_aimdead";
    const string K_InvertY = "set_inverty";
    const string K_Shake   = "set_shake";
    const string K_Quality = "set_quality";
    const string K_VSync   = "set_vsync";
    const string K_Full    = "set_full";
    const string K_ResW    = "set_resw";
    const string K_ResH    = "set_resh";
    const string K_FpsCap  = "set_fpscap";

    public const float DEF_Master = 1f, DEF_Music = 0.8f, DEF_SFX = 1f, DEF_AimDead = 0.19f;

    public float MasterVolume { get; private set; }
    public float MusicVolume  { get; private set; }
    public float SfxVolume    { get; private set; }
    public bool  Muted        { get; private set; }
    public float AimDeadzone  { get; private set; }
    public bool  InvertAimY   { get; private set; }
    public static bool CameraShakeEnabled { get; private set; }
    public int   QualityLevel { get; private set; }
    public bool  VSync        { get; private set; }
    public bool  Fullscreen   { get; private set; }
    public int   ResolutionWidth  { get; private set; }
    public int   ResolutionHeight { get; private set; }
    public int   FpsCap       { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAndApply();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        ApplyAimSettings();
    }

    public void LoadAndApply()
    {
        MasterVolume = PlayerPrefs.GetFloat(K_Master, DEF_Master);
        MusicVolume  = PlayerPrefs.GetFloat(K_Music,  DEF_Music);
        SfxVolume    = PlayerPrefs.GetFloat(K_SFX,    DEF_SFX);
        Muted        = PlayerPrefs.GetInt(K_Mute, 0) == 1;
        AimDeadzone  = Mathf.Clamp(PlayerPrefs.GetFloat(K_AimDead, DEF_AimDead), 0.05f, 0.5f);
        InvertAimY   = PlayerPrefs.GetInt(K_InvertY, 0) == 1;
        CameraShakeEnabled = PlayerPrefs.GetInt(K_Shake, 1) == 1;

        int qDefault = QualitySettings.GetQualityLevel();
        QualityLevel = Mathf.Clamp(PlayerPrefs.GetInt(K_Quality, qDefault), 0, QualitySettings.names.Length - 1);
        VSync      = PlayerPrefs.GetInt(K_VSync, 1) == 1;
        Fullscreen = PlayerPrefs.GetInt(K_Full, 1) == 1;
        ResolutionWidth  = PlayerPrefs.GetInt(K_ResW, Screen.currentResolution.width);
        ResolutionHeight = PlayerPrefs.GetInt(K_ResH, Screen.currentResolution.height);
        FpsCap     = PlayerPrefs.GetInt(K_FpsCap, -1);

        ApplyAll();
    }

    public void ApplyAll()
    {

        QualitySettings.SetQualityLevel(QualityLevel, true);
        QualitySettings.vSyncCount = VSync ? 1 : 0;
        Screen.SetResolution(ResolutionWidth, ResolutionHeight, Fullscreen);
        Application.targetFrameRate = FpsCap;

        if (AudioManager.Instance != null)
            AudioManager.Instance.ApplyVolumes(MasterVolume, MusicVolume, SfxVolume, Muted);

        ApplyAimSettings();
    }

    public void SetMasterVolume(float v) { MasterVolume = v; PlayerPrefs.SetFloat(K_Master, v); if (AudioManager.Instance != null) AudioManager.Instance.SetMaster(v); }
    public void SetMusicVolume (float v) { MusicVolume  = v; PlayerPrefs.SetFloat(K_Music,  v); if (AudioManager.Instance != null) AudioManager.Instance.SetMusic(v); }
    public void SetSfxVolume   (float v) { SfxVolume    = v; PlayerPrefs.SetFloat(K_SFX,    v); if (AudioManager.Instance != null) AudioManager.Instance.SetSfx(v); }
    public void SetMuted(bool m)         { Muted = m; PlayerPrefs.SetInt(K_Mute, m ? 1 : 0); if (AudioManager.Instance != null) AudioManager.Instance.SetMuted(m); }

    public void SetAimDeadzone(float v) { AimDeadzone = Mathf.Clamp(v, 0.05f, 0.5f); PlayerPrefs.SetFloat(K_AimDead, AimDeadzone); ApplyAimSettings(); }
    public void SetInvertY(bool b)      { InvertAimY = b; PlayerPrefs.SetInt(K_InvertY, b ? 1 : 0); ApplyAimSettings(); }
    public void SetCameraShake(bool b)  { CameraShakeEnabled = b; PlayerPrefs.SetInt(K_Shake, b ? 1 : 0); }

    public void SetQuality(int i)    { QualityLevel = Mathf.Clamp(i, 0, QualitySettings.names.Length - 1); PlayerPrefs.SetInt(K_Quality, QualityLevel); QualitySettings.SetQualityLevel(QualityLevel, true); }
    public void SetVSync(bool b)     { VSync = b; PlayerPrefs.SetInt(K_VSync, b ? 1 : 0); QualitySettings.vSyncCount = b ? 1 : 0; }
    public void SetFullscreen(bool b){ Fullscreen = b; PlayerPrefs.SetInt(K_Full, b ? 1 : 0); Screen.fullScreen = b; }
    public void SetFpsCap(int fps)   { FpsCap = fps; PlayerPrefs.SetInt(K_FpsCap, fps); Application.targetFrameRate = fps; }
    public void SetResolution(int w, int h) { ResolutionWidth = w; ResolutionHeight = h; PlayerPrefs.SetInt(K_ResW, w); PlayerPrefs.SetInt(K_ResH, h); Screen.SetResolution(w, h, Fullscreen); }

    public void Save()     { PlayerPrefs.Save(); }
    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey(K_Master); PlayerPrefs.DeleteKey(K_Music); PlayerPrefs.DeleteKey(K_SFX);
        PlayerPrefs.DeleteKey(K_Mute); PlayerPrefs.DeleteKey(K_AimDead); PlayerPrefs.DeleteKey(K_InvertY);
        PlayerPrefs.DeleteKey(K_Shake); PlayerPrefs.DeleteKey(K_Quality); PlayerPrefs.DeleteKey(K_VSync);
        PlayerPrefs.DeleteKey(K_Full); PlayerPrefs.DeleteKey(K_ResW); PlayerPrefs.DeleteKey(K_ResH);
        PlayerPrefs.DeleteKey(K_FpsCap);
        PlayerPrefs.Save();
        LoadAndApply();
    }

    void ApplyAimSettings()
    {
        Tank_Inputs inp = FindAnyObjectByType<Tank_Inputs>();
        if (inp != null)
            inp.SetAimConfig(AimDeadzone, InvertAimY);
    }
}
