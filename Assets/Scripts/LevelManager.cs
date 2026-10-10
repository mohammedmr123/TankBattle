using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject victoryPanel;
    public GameObject gameOverPanel;

    [Header("Scene Indices")]
    public int mainMenuSceneIndex = 0;
    public int firstLevelSceneIndex = 1;
    public int totalLevelCount = 3;

    private static readonly HashSet<EnemyTankAI> _liveEnemies = new HashSet<EnemyTankAI>();

    private bool _levelComplete = false;

    public static int ActiveProjectileCount { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _liveEnemies.Clear();
        _levelComplete = false;
        ActiveProjectileCount = 0;

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void RegisterEnemy(EnemyTankAI enemy)
    {
        _liveEnemies.Add(enemy);
    }

    public static void UnregisterEnemy(EnemyTankAI enemy)
    {
        _liveEnemies.Remove(enemy);

        if (Instance != null)
            Instance.CheckVictory();
    }

    public static int EnemyCount => _liveEnemies.Count;

    private void CheckVictory()
    {
        if (_levelComplete) return;
        if (_liveEnemies.Count > 0) return;

        _levelComplete = true;
        Time.timeScale = 1f;
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    public void TriggerDefeat()
    {
        if (_levelComplete) return;
        _levelComplete = true;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void OnRetryButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnNextLevelButton()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next <= firstLevelSceneIndex + totalLevelCount - 1)
            SceneManager.LoadScene(next);
        else
            SceneManager.LoadScene(mainMenuSceneIndex);
    }

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    public void DebugSkipLevel()
    {

        foreach (EnemyTankAI e in new List<EnemyTankAI>(_liveEnemies))
        {
            if (e != null) Destroy(e.gameObject);
        }
        _liveEnemies.Clear();
        CheckVictory();
    }

    [Header("QA Spawn")]
    public EnemyTankAI enemyPrefab;
    public Transform spawnPoint;

    public void DebugSpawnEnemy()
    {
        if (enemyPrefab == null) return;
        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero + Vector3.right * 5f;
        EnemyTankAI spawned = Instantiate(enemyPrefab, pos, Quaternion.identity);

        _ = spawned;
    }
}

