using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FPSDebugPanel : MonoBehaviour
{
    public static FPSDebugPanel Instance { get; private set; }

    [Header("Display Settings")]
    [Tooltip("How often (in seconds) the metrics text updates.")]
    public float updateInterval = 0.1f;

    [Header("Player Reference (for God Mode)")]
    public TankHealth playerTankHealth;

    private const int SampleCount = 128;
    private float[] _frameTimes = new float[SampleCount];
    private int _sampleIndex = 0;
    private int _samplesFilled = 0;

    private bool _panelVisible = false;
    private bool _godMode = false;
    private float _timeSinceUpdate = 0f;

    private bool _paused = false;
    private float _prevTimeScale = 1f;
    private bool _collisionDiag = false;

    private Rigidbody _diagRb;
    private Collider _diagCol;
    private string _diagLine1 = "";
    private string _diagLine2 = "";

    private string _fpsLine = "";
    private string _smoothFpsLine = "";
    private string _lowFpsLine = "";
    private string _frameTimeLine = "";
    private string _memoryLine = "";
    private string _enemyLine = "";
    private string _projLine = "";
    private string _godModeLine = "";

    private float _smoothFPS = 0f;
    private const float SmoothFactor = 0.1f;

    private GUIStyle _boxStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _buttonStyle;
    private bool _stylesInitialized = false;

    private void Awake()
    {

        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {

        float dt = Time.unscaledDeltaTime;
        _frameTimes[_sampleIndex % SampleCount] = dt;
        _sampleIndex++;
        _samplesFilled = Mathf.Min(_samplesFilled + 1, SampleCount);

        float currentFPS = dt > 0f ? 1f / dt : 0f;
        _smoothFPS = Mathf.Lerp(_smoothFPS, currentFPS, SmoothFactor);

        bool debugToggle = false;
        try { debugToggle = Input.GetButtonDown("DebugToggle"); } catch { }
        if (debugToggle || Input.GetKeyDown(KeyCode.F3) || Input.GetKeyDown(KeyCode.BackQuote))
        {
            _panelVisible = !_panelVisible;
        }

        _timeSinceUpdate += dt;
        if (_timeSinceUpdate >= updateInterval)
        {
            _timeSinceUpdate = 0f;
            RefreshMetrics(currentFPS);
        }
    }

    private void RefreshMetrics(float currentFPS)
    {
        float frameMs = Time.unscaledDeltaTime * 1000f;
        float low1Pct = Calculate1PercentLow();
        long memBytes = System.GC.GetTotalMemory(false);
        float memMB = memBytes / (1024f * 1024f);
        int enemies = LevelManager.EnemyCount;
        int projs = LevelManager.ActiveProjectileCount;

        _fpsLine      = string.Format("FPS: {0:F0}", currentFPS);
        _smoothFpsLine = string.Format("Avg FPS: {0:F0}", _smoothFPS);
        _lowFpsLine   = string.Format("1% Low: {0:F0}", low1Pct);
        _frameTimeLine = string.Format("Frame: {0:F2} ms", frameMs);
        _memoryLine   = string.Format("Heap: {0:F1} MB", memMB);
        _enemyLine    = string.Format("Enemies: {0}", enemies);
        _projLine     = string.Format("Projectiles: {0}", projs);
        _godModeLine  = _godMode ? "GOD MODE: ON" : "GOD MODE: OFF";
    }

    private float Calculate1PercentLow()
    {
        if (_samplesFilled == 0) return 0f;

        int count = _samplesFilled;
        float[] temp = new float[count];
        for (int i = 0; i < count; i++)
            temp[i] = _frameTimes[i];

        System.Array.Sort(temp);

        int worstCount = Mathf.Max(1, Mathf.CeilToInt(count * 0.01f));
        float sum = 0f;
        for (int i = count - 1; i >= count - worstCount; i--)
            sum += temp[i];
        float avgWorstDelta = sum / worstCount;
        return avgWorstDelta > 0f ? 1f / avgWorstDelta : 0f;
    }

    private void InitStyles()
    {
        if (_stylesInitialized) return;
        _stylesInitialized = true;

        _boxStyle = new GUIStyle(GUI.skin.box);
        _boxStyle.normal.background = MakeSolidTexture(new Color(0f, 0f, 0f, 0.75f));

        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            normal = { textColor = Color.white }
        };

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13
        };
    }

    private static Texture2D MakeSolidTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    private void OnGUI()
    {
        if (!_panelVisible) return;

        InitStyles();

        float panelX = 10f;
        float panelY = 10f;
        float panelW = 260f;
        float panelH = 520f;

        GUI.Box(new Rect(panelX, panelY, panelW, panelH), "", _boxStyle);

        float x = panelX + 8f;
        float y = panelY + 8f;
        float lineH = 20f;
        float btnH = 26f;
        float btnW = (panelW - 24f) / 3f;

        GUI.Label(new Rect(x, y, panelW, lineH), _fpsLine, _labelStyle); y += lineH;
        GUI.Label(new Rect(x, y, panelW, lineH), _smoothFpsLine, _labelStyle); y += lineH;
        GUI.Label(new Rect(x, y, panelW, lineH), _lowFpsLine, _labelStyle); y += lineH;
        GUI.Label(new Rect(x, y, panelW, lineH), _frameTimeLine, _labelStyle); y += lineH;
        GUI.Label(new Rect(x, y, panelW, lineH), _memoryLine, _labelStyle); y += lineH;
        GUI.Label(new Rect(x, y, panelW, lineH), _enemyLine, _labelStyle); y += lineH;
        GUI.Label(new Rect(x, y, panelW, lineH), _projLine, _labelStyle); y += lineH;

        GUI.Label(new Rect(x, y, panelW, lineH), _godModeLine,
            _godMode ? new GUIStyle(_labelStyle) { normal = { textColor = Color.green } } : _labelStyle);
        y += lineH + 4f;

        if (GUI.Button(new Rect(x, y, panelW - 16f, btnH), "Toggle God Mode", _buttonStyle))
        {
            ToggleGodMode();
        }
        y += btnH + 4f;

        if (GUI.Button(new Rect(x, y, panelW - 16f, btnH), "Spawn Enemy", _buttonStyle))
        {
            LevelManager.Instance?.DebugSpawnEnemy();
        }
        y += btnH + 4f;

        if (GUI.Button(new Rect(x, y, panelW - 16f, btnH), "Win/Skip Level", _buttonStyle))
        {
            LevelManager.Instance?.DebugSkipLevel();
        }
        y += btnH + 4f;

        GUI.Label(new Rect(x, y, 60f, lineH), "Speed:", _labelStyle);
        if (GUI.Button(new Rect(x + 60f, y, btnW, btnH), "0.5x", _buttonStyle))
            Time.timeScale = 0.5f;
        if (GUI.Button(new Rect(x + 60f + btnW + 2f, y, btnW, btnH), "1x", _buttonStyle))
            Time.timeScale = 1f;
        if (GUI.Button(new Rect(x + 60f + (btnW + 2f) * 2f, y, btnW, btnH), "1.5x", _buttonStyle))
            Time.timeScale = 1.5f;
        y += btnH + 4f;

        float halfW = (panelW - 16f - 4f) / 2f;
        if (GUI.Button(new Rect(x, y, halfW, btnH), _paused ? "Resume" : "Pause", _buttonStyle))
            TogglePause();
        if (GUI.Button(new Rect(x + halfW + 4f, y, halfW, btnH), "Reset Speed", _buttonStyle))
        { if (_paused) TogglePause(); Time.timeScale = 1f; }
        y += btnH + 4f;

        if (GUI.Button(new Rect(x, y, halfW, btnH), "Reload Level", _buttonStyle))
        { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
        if (GUI.Button(new Rect(x + halfW + 4f, y, halfW, btnH), "Main Menu", _buttonStyle))
        { Time.timeScale = 1f; if (LevelManager.Instance != null) LevelManager.Instance.OnMainMenuButton(); else SceneManager.LoadScene(0); }
        y += btnH + 4f;

        if (GUI.Button(new Rect(x, y, halfW, btnH), "Toggle Enemy AI", _buttonStyle))
            ToggleEnemyAI();
        if (GUI.Button(new Rect(x + halfW + 4f, y, halfW, btnH),
            _collisionDiag ? "Collision Diag: ON" : "Collision Diag: OFF", _buttonStyle))
            _collisionDiag = !_collisionDiag;
        y += btnH + 4f;

        if (GUI.Button(new Rect(x, y, panelW - 16f, btnH), "Close (F3)", _buttonStyle))
            _panelVisible = false;
        y += btnH + 6f;

        if (_collisionDiag)
        {
            UpdateCollisionDiagnostics();
            GUI.Label(new Rect(x, y, panelW, lineH), _diagLine1, _labelStyle); y += lineH;
            GUI.Label(new Rect(x, y, panelW, lineH), _diagLine2, _labelStyle); y += lineH;
        }
    }

    private void ToggleGodMode()
    {
        _godMode = !_godMode;

        if (playerTankHealth == null)
            playerTankHealth = FindAnyObjectByType<TankHealth>();

        if (playerTankHealth != null)
            playerTankHealth.godMode = _godMode;
    }

    private void TogglePause()
    {
        _paused = !_paused;
        if (_paused)
        {
            _prevTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = _prevTimeScale;
        }
    }

    private void ToggleEnemyAI()
    {

        var ais = FindObjectsByType<EnemyTankAI>(FindObjectsInactive.Include);
        if (ais == null || ais.Length == 0) return;

        bool anyEnabled = false;
        foreach (var ai in ais) if (ai != null && ai.enabled) { anyEnabled = true; break; }

        foreach (var ai in ais)
            if (ai != null) ai.enabled = !anyEnabled;
    }

    private void UpdateCollisionDiagnostics()
    {

        if (_diagRb == null || _diagCol == null)
        {
            var tc = FindAnyObjectByType<TankController>();
            if (tc == null)
            {
                _diagLine1 = "Collider: (no TankController found)";
                _diagLine2 = "";
                return;
            }
            _diagRb = tc.GetComponent<Rigidbody>();
            _diagCol = tc.GetComponent<Collider>();
        }

        if (_diagCol == null)
            _diagLine1 = "Collider: MISSING on player tank";
        else
            _diagLine1 = string.Format("Col {0} enabled={1} trigger={2}",
                _diagCol.GetType().Name, _diagCol.enabled, _diagCol.isTrigger);

        if (_diagRb == null)
            _diagLine2 = "Rigidbody: MISSING";
        else
            _diagLine2 = string.Format("RB mass={0} speed={1:0.0} m/s detect={2}",
                _diagRb.mass, _diagRb.linearVelocity.magnitude, _diagRb.collisionDetectionMode);
    }
}

