using System.Collections.Generic;
using UnityEngine;

public class Level3Generator : MonoBehaviour
{
    [Header("Maze")]
    public int mazeWidth = 7;
    public int mazeHeight = 7;
    public float cellSize = 11f;
    public float wallHeight = 5f;
    public float wallThickness = 1.2f;

    [Header("Boss")]
    public GameObject bossPrefab;
    public int bossHealth = 600;
    public float bossScale = 1.9f;
    public float bossLaunchForce = 48f;
    public float bossFireInterval = 0.9f;

    [Header("Patrol Enemies")]
    public GameObject enemyPrefab;
    public int enemyCount = 4;

    [Header("Materials (optional)")]
    public Material wallMaterial;
    public Material groundMaterial;
    public Material bossMaterial;

    private bool[,] _vWalls;
    private bool[,] _hWalls;
    private bool[,] _visited;
    private Transform _mazeRoot;

    private void Start()
    {
        EnsureLevelManager();

        _mazeRoot = new GameObject("Labyrinth").transform;
        GenerateMaze();
        BuildGround();
        BuildWalls();
        CarveBossArena();
        SpawnPlayerSpawnMarker();

        SpawnBoss();
        SpawnEnemies();

        EnsureVictoryPanel();

        Debug.Log("[Level3Generator] Labyrinth + boss generated.");
    }

    private void EnsureLevelManager()
    {
        if (FindAnyObjectByType<LevelManager>() != null) return;
        var go = new GameObject("LevelManager");
        go.AddComponent<LevelManager>();
    }

    private void EnsureVictoryPanel()
    {
        if (FindAnyObjectByType<VictoryPanelUI>() != null) return;
        var go = new GameObject("VictoryPanelBuilder");
        go.AddComponent<VictoryPanelUI>();
    }

    private void GenerateMaze()
    {
        int W = mazeWidth, H = mazeHeight;
        _vWalls = new bool[W + 1, H];
        _hWalls = new bool[W, H + 1];
        _visited = new bool[W, H];

        for (int x = 0; x <= W; x++)
            for (int z = 0; z < H; z++) _vWalls[x, z] = true;
        for (int x = 0; x < W; x++)
            for (int z = 0; z <= H; z++) _hWalls[x, z] = true;

        var stack = new Stack<Vector2Int>();
        var startCell = new Vector2Int(0, 0);
        _visited[startCell.x, startCell.y] = true;
        stack.Push(startCell);

        while (stack.Count > 0)
        {
            Vector2Int cur = stack.Peek();
            var neighbors = new List<Vector2Int>();

            if (cur.x > 0 && !_visited[cur.x - 1, cur.y]) neighbors.Add(new Vector2Int(cur.x - 1, cur.y));
            if (cur.x < W - 1 && !_visited[cur.x + 1, cur.y]) neighbors.Add(new Vector2Int(cur.x + 1, cur.y));
            if (cur.y > 0 && !_visited[cur.x, cur.y - 1]) neighbors.Add(new Vector2Int(cur.x, cur.y - 1));
            if (cur.y < H - 1 && !_visited[cur.x, cur.y + 1]) neighbors.Add(new Vector2Int(cur.x, cur.y + 1));

            if (neighbors.Count == 0) { stack.Pop(); continue; }

            Vector2Int next = neighbors[Random.Range(0, neighbors.Count)];

            if (next.x == cur.x + 1) _vWalls[cur.x + 1, cur.y] = false;
            else if (next.x == cur.x - 1) _vWalls[cur.x, cur.y] = false;
            else if (next.y == cur.y + 1) _hWalls[cur.x, cur.y + 1] = false;
            else if (next.y == cur.y - 1) _hWalls[cur.x, cur.y] = false;

            _visited[next.x, next.y] = true;
            stack.Push(next);
        }
    }

    private Vector3 CellCenter(int col, int row, float y = 1f)
    {
        return new Vector3(col * cellSize + cellSize * 0.5f, y, row * cellSize + cellSize * 0.5f);
    }

    private void BuildGround()
    {
        float w = mazeWidth * cellSize;
        float h = mazeHeight * cellSize;

        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.SetParent(_mazeRoot, false);
        ground.transform.position = new Vector3(w * 0.5f, -0.5f, h * 0.5f);
        ground.transform.localScale = new Vector3(w, 1f, h);
        ApplyMaterial(ground, groundMaterial, new Color(0.25f, 0.27f, 0.3f));
    }

    private void BuildWalls()
    {
        int W = mazeWidth, H = mazeHeight;

        for (int x = 0; x <= W; x++)
        {
            for (int z = 0; z < H; z++)
            {
                if (!_vWalls[x, z]) continue;
                float cx = x * cellSize;
                float cz = z * cellSize + cellSize * 0.5f;
                SpawnWall(cx, cz, wallThickness, cellSize);
            }
        }

        for (int x = 0; x < W; x++)
        {
            for (int z = 0; z <= H; z++)
            {
                if (!_hWalls[x, z]) continue;
                float cx = x * cellSize + cellSize * 0.5f;
                float cz = z * cellSize;
                SpawnWall(cx, cz, cellSize, wallThickness);
            }
        }
    }

    private void SpawnWall(float cx, float cz, float sizeX, float sizeZ)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.SetParent(_mazeRoot, false);
        wall.transform.position = new Vector3(cx, wallHeight * 0.5f, cz);
        wall.transform.localScale = new Vector3(sizeX, wallHeight, sizeZ);
        ApplyMaterial(wall, wallMaterial, new Color(0.55f, 0.57f, 0.6f));
    }

    private void CarveBossArena()
    {
        int W = mazeWidth, H = mazeHeight;
        int cx = W / 2;
        int cz = H / 2;
        int radius = 1;

        for (int x = cx - radius; x <= cx + radius; x++)
        {
            for (int z = cz - radius; z <= cz + radius; z++)
            {
                if (x < 0 || x >= W || z < 0 || z >= H) continue;

                if (x + 1 <= W) _vWalls[x + 1, z] = false;
                if (x >= 0) _vWalls[x, z] = false;

                if (z + 1 <= H) _hWalls[x, z + 1] = false;
                if (z >= 0) _hWalls[x, z] = false;
            }
        }
    }

    private void SpawnPlayerSpawnMarker()
    {
        var marker = new GameObject("PLAYER_SPAWN");
        marker.transform.position = CellCenter(0, mazeHeight - 1, 1f);

        var guide = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        guide.name = "SpawnGuide";
        guide.transform.SetParent(marker.transform, false);
        guide.transform.localPosition = new Vector3(0, 1f, 0);
        guide.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
        ApplyMaterial(guide, null, new Color(0.2f, 0.8f, 1f));
        Object.Destroy(guide.GetComponent<Collider>());
    }

    private void ApplyMaterial(GameObject go, Material mat, Color fallbackColor)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;

        if (mat != null)
        {
            rend.sharedMaterial = mat;
        }
        else
        {

            var m = new Material(Shader.Find("Standard"));
            m.color = fallbackColor;
            rend.sharedMaterial = m;
        }
    }

    private void SpawnBoss()
    {
        int cx = mazeWidth / 2;
        int cz = mazeHeight / 2;
        Vector3 pos = CellCenter(cx, cz, 1.5f);

        GameObject boss;
        if (bossPrefab != null)
        {
            boss = Instantiate(bossPrefab, pos, Quaternion.identity);
        }
        else
        {
            boss = BuildFallbackTank("BossTank", pos, new Color(0.7f, 0.1f, 0.1f));
        }

        boss.name = "BossTank";
        boss.tag = "EnemyTank";
        boss.transform.localScale = Vector3.one * bossScale;

        var ai = boss.GetComponent<EnemyTankAI>();
        if (ai == null) ai = boss.AddComponent<EnemyTankAI>();
        ai.launchForce = bossLaunchForce;
        ai.timeBetweenShots = bossFireInterval;
        ai.tankSpeed = 7f;
        ai.followRange = 1000f;
        ai.minDistance = 14f;

        var health = boss.GetComponent<EnemyTankHealth>();
        if (health == null) health = boss.AddComponent<EnemyTankHealth>();
        health.maxHealth = bossHealth;

        EnsureTankPhysics(boss);

        if (bossMaterial != null)
        {
            foreach (var r in boss.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = bossMaterial;
        }

    }

    private void SpawnEnemies()
    {
        int spawned = 0;
        int guard = 0;
        int W = mazeWidth, H = mazeHeight;

        while (spawned < enemyCount && guard < 500)
        {
            guard++;
            int x = Random.Range(0, W);
            int z = Random.Range(0, H);

            bool nearCentre = Mathf.Abs(x - W / 2) <= 1 && Mathf.Abs(z - H / 2) <= 1;
            bool nearSpawn = (x == 0 && z == H - 1);
            if (nearCentre || nearSpawn) continue;

            Vector3 pos = CellCenter(x, z, 1f);

            GameObject enemy;
            if (enemyPrefab != null)
                enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            else
                enemy = BuildFallbackTank("EnemyTank_Patrol", pos, new Color(0.6f, 0.35f, 0.1f));

            enemy.tag = "EnemyTank";
            EnsureTankPhysics(enemy);

            if (enemy.GetComponent<EnemyTankAI>() == null)
                enemy.AddComponent<EnemyTankAI>();
            if (enemy.GetComponent<EnemyTankHealth>() == null)
                enemy.AddComponent<EnemyTankHealth>();

            spawned++;
        }
    }

    private void EnsureTankPhysics(GameObject tank)
    {
        if (tank.GetComponent<Collider>() == null)
        {
            var col = tank.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.2f, 0);
            col.size = new Vector3(6.4f, 2.5f, 9.4f);
        }

        var rb = tank.GetComponent<Rigidbody>();
        if (rb == null) rb = tank.AddComponent<Rigidbody>();
        rb.mass = 1000f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private GameObject BuildFallbackTank(string name, Vector3 pos, Color color)
    {
        var tank = new GameObject(name);
        tank.transform.position = pos;

        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(tank.transform, false);
        body.transform.localPosition = new Vector3(0, 1.2f, 0);
        body.transform.localScale = new Vector3(6.4f, 2.5f, 9.4f);
        ApplyMaterial(body, null, color);

        var turret = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        turret.name = "Turret";
        turret.transform.SetParent(tank.transform, false);
        turret.transform.localPosition = new Vector3(0, 3f, 0);
        turret.transform.localScale = new Vector3(2.5f, 0.6f, 2.5f);
        ApplyMaterial(turret, null, color * 0.8f);

        return tank;
    }
}
