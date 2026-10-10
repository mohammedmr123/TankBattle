using UnityEngine;

public class OptionsMenuUI : MonoBehaviour
{
    public static OptionsMenuUI Instance { get; private set; }
    public static bool IsOpen { get; private set; }

    bool _visible;
    bool _pausedByMenu;
    float _prevTimeScale = 1f;

    bool _awaitingConfirm;
    float _confirmTimer;
    int _pendingW, _pendingH;
    bool _pendingFull;
    int _savedW, _savedH;
    bool _savedFull;

    string[] _resStrings;
    int[] _resW, _resH;
    int _resIndex;
    string[] _qualityNames;
    int _qualityIndex;
    int _fpsCapIndex;
    readonly int[] _fpsOptions = { -1, 30, 60, 120, 144 };

    GUIStyle _win, _label, _btn, _toggle, _sliderBg;
    bool _stylesReady;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildResolutionList();
        _qualityNames = QualitySettings.names;
    }

    void Update()
    {

        bool cancel = false;
        try { cancel = Input.GetButtonDown("Cancel"); } catch { }
        if (cancel || Input.GetKeyDown(KeyCode.Escape))
            Toggle();

        if (_awaitingConfirm)
        {
            _confirmTimer -= Time.unscaledDeltaTime;
            if (_confirmTimer <= 0f) RevertDisplayChange();
        }
    }

    public static void Open()  { if (Instance != null && !IsOpen) Instance.Toggle(); }
    public static void Close() { if (Instance != null && IsOpen)  Instance.Toggle(); }

    void Toggle()
    {
        _visible = !_visible;
        IsOpen = _visible;
        if (_visible) SyncFromSettings();
        ApplyPause();
    }

    void ApplyPause()
    {
        if (_visible)
        {
            if (Time.timeScale > 0f) { _prevTimeScale = Time.timeScale; _pausedByMenu = true; }
            else _pausedByMenu = false;
            if (_pausedByMenu) Time.timeScale = 0f;
        }
        else
        {
            _awaitingConfirm = false;
            if (_pausedByMenu) { Time.timeScale = _prevTimeScale; _pausedByMenu = false; }
        }
    }

    void BuildResolutionList()
    {
        Resolution[] all = Screen.resolutions;
        _resStrings = new string[all.Length];
        _resW = new int[all.Length];
        _resH = new int[all.Length];
        for (int i = 0; i < all.Length; i++)
        {
            _resW[i] = all[i].width; _resH[i] = all[i].height;
            _resStrings[i] = all[i].width + " x " + all[i].height;
        }
    }

    void SyncFromSettings()
    {
        var s = SettingsManager.Instance;
        if (s == null) return;
        _resIndex = FindResIndex(s.ResolutionWidth, s.ResolutionHeight);
        _qualityIndex = Mathf.Clamp(s.QualityLevel, 0, _qualityNames.Length - 1);
        _fpsCapIndex = FindFpsIndex(s.FpsCap);
        _savedW = s.ResolutionWidth; _savedH = s.ResolutionHeight; _savedFull = s.Fullscreen;
    }

    int FindResIndex(int w, int h)
    {
        for (int i = 0; i < _resW.Length; i++) if (_resW[i] == w && _resH[i] == h) return i;
        return _resW.Length - 1;
    }
    int FindFpsIndex(int fps)
    {
        for (int i = 0; i < _fpsOptions.Length; i++) if (_fpsOptions[i] == fps) return i;
        return 0;
    }

    void RequestDisplayChange(int w, int h, bool full)
    {
        _pendingW = w; _pendingH = h; _pendingFull = full;
        _awaitingConfirm = true;
        _confirmTimer = 10f;

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetResolution(w, h);
        if (full != Screen.fullScreen) Screen.fullScreen = full;
    }
    void ConfirmDisplayChange()
    {
        _awaitingConfirm = false;
        _savedW = _pendingW; _savedH = _pendingH; _savedFull = _pendingFull;
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetFullscreen(_pendingFull);
            SettingsManager.Instance.Save();
        }
    }
    void RevertDisplayChange()
    {
        _awaitingConfirm = false;
        Screen.SetResolution(_savedW, _savedH, _savedFull);
        if (SettingsManager.Instance != null) SettingsManager.Instance.SetResolution(_savedW, _savedH);
    }

    void InitStyles()
    {
        if (_stylesReady) return;
        _stylesReady = true;
        _win = new GUIStyle(GUI.skin.window) { fontSize = 16, fontStyle = FontStyle.Bold };
        _label = new GUIStyle(GUI.skin.label) { fontSize = 15, normal = { textColor = Color.white } };
        _btn = new GUIStyle(GUI.skin.button) { fontSize = 15 };
        _toggle = new GUIStyle(GUI.skin.toggle) { fontSize = 15 };
        _sliderBg = new GUIStyle(GUI.skin.box);
    }

    void OnGUI()
    {
        if (!_visible) return;
        var s = SettingsManager.Instance;
        if (s == null) return;
        InitStyles();

        float w = 460f, h = 470f;
        float x = (Screen.width - w) * 0.5f, y = (Screen.height - h) * 0.5f;
        GUILayout.BeginArea(new Rect(x, y, w, h), "OPTIONS", _win);
        GUILayout.Space(8);

        GUILayout.Label("— AUDIO —", _label);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Master", GUILayout.Width(120));
        float mv = GUILayout.HorizontalSlider(s.MasterVolume, 0f, 1f);
        if (!Mathf.Approximately(mv, s.MasterVolume)) s.SetMasterVolume(mv);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Music", GUILayout.Width(120));
        float mus = GUILayout.HorizontalSlider(s.MusicVolume, 0f, 1f);
        if (!Mathf.Approximately(mus, s.MusicVolume)) s.SetMusicVolume(mus);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("SFX", GUILayout.Width(120));
        float sfx = GUILayout.HorizontalSlider(s.SfxVolume, 0f, 1f);
        if (!Mathf.Approximately(sfx, s.SfxVolume)) s.SetSfxVolume(sfx);
        GUILayout.EndHorizontal();
        bool muted = GUILayout.Toggle(s.Muted, "Mute All", _toggle);
        if (muted != s.Muted) s.SetMuted(muted);

        GUILayout.Space(6);
        GUILayout.Label("— CONTROLS —", _label);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Aim Deadzone", GUILayout.Width(120));
        float dz = GUILayout.HorizontalSlider(s.AimDeadzone, 0.05f, 0.5f);
        if (!Mathf.Approximately(dz, s.AimDeadzone)) s.SetAimDeadzone(dz);
        GUILayout.EndHorizontal();
        bool inv = GUILayout.Toggle(s.InvertAimY, "Invert Aim Y (gamepad)", _toggle);
        if (inv != s.InvertAimY) s.SetInvertY(inv);
        bool shake = GUILayout.Toggle(SettingsManager.CameraShakeEnabled, "Camera Shake", _toggle);
        if (shake != SettingsManager.CameraShakeEnabled) s.SetCameraShake(shake);

        GUILayout.Space(6);
        GUILayout.Label("— GRAPHICS —", _label);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Resolution", GUILayout.Width(120));
        int newRes = GUILayout.SelectionGrid(_resIndex, _resStrings, 3);
        if (newRes != _resIndex) { _resIndex = newRes; RequestDisplayChange(_resW[newRes], _resH[newRes], s.Fullscreen); }
        GUILayout.EndHorizontal();
        bool full = GUILayout.Toggle(s.Fullscreen, "Fullscreen", _toggle);
        if (full != s.Fullscreen) RequestDisplayChange(s.ResolutionWidth, s.ResolutionHeight, full);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Quality", GUILayout.Width(120));
        int newQ = GUILayout.SelectionGrid(_qualityIndex, _qualityNames, 3);
        if (newQ != _qualityIndex) { _qualityIndex = newQ; s.SetQuality(newQ); }
        GUILayout.EndHorizontal();
        bool vsync = GUILayout.Toggle(s.VSync, "VSync", _toggle);
        if (vsync != s.VSync) s.SetVSync(vsync);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Frame Limit", GUILayout.Width(120));
        string[] fpsStrings = { "Unlimited", "30", "60", "120", "144" };
        int newFps = GUILayout.SelectionGrid(_fpsCapIndex, fpsStrings, 5);
        if (newFps != _fpsCapIndex) { _fpsCapIndex = newFps; s.SetFpsCap(_fpsOptions[newFps]); }
        GUILayout.EndHorizontal();

        GUILayout.Space(8);
        if (_awaitingConfirm)
        {
            GUILayout.Label("Keep these display settings? Reverting in " + Mathf.CeilToInt(_confirmTimer) + "s...", _label);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Keep Changes", _btn)) ConfirmDisplayChange();
            if (GUILayout.Button("Revert", _btn)) RevertDisplayChange();
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save", _btn)) s.Save();
            if (GUILayout.Button("Reset Defaults", _btn)) { s.ResetToDefaults(); SyncFromSettings(); }
            if (GUILayout.Button("Back (ESC)", _btn)) Toggle();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }
}
