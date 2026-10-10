using UnityEngine;
using Unity.Cinemachine;

public class CinemachineArenaConfiner : MonoBehaviour
{
    [Header("Fallback bounds (used if no Ground object is found)")]
    [Tooltip("Half-size of the arena on X and Z.")]
    public float defaultHalfSize = 60f;
    [Tooltip("Height of the bounding box (tall enough to contain the camera).")]
    public float boundsHeight = 60f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoSpawn()
    {
        if (FindAnyObjectByType<CinemachineArenaConfiner>() != null) return;
        if (FindAnyObjectByType<CinemachineCamera>() == null) return;
        new GameObject("[CinemachineArenaConfiner]").AddComponent<CinemachineArenaConfiner>();
    }

    private void Start()
    {
        Configure();
    }

    private void Configure()
    {
        CinemachineCamera vcam = FindAnyObjectByType<CinemachineCamera>();
        if (vcam == null)
        {
            Debug.LogWarning("[CinemachineArenaConfiner] No CinemachineCamera found; cannot confine.");
            return;
        }

        Collider bounds = GetOrCreateBounds(vcam);

        var confiner = vcam.GetComponent<CinemachineConfiner3D>();
        if (confiner == null) confiner = vcam.gameObject.AddComponent<CinemachineConfiner3D>();
        confiner.BoundingVolume = bounds;
        confiner.SlowingDistance = 0f;

        Debug.Log($"[CinemachineArenaConfiner] Confiner added to '{vcam.name}' bounds size={bounds.bounds.size}.");
    }

    private Collider GetOrCreateBounds(CinemachineCamera vcam)
    {
        GameObject existing = GameObject.Find("[CameraBounds]");
        if (existing != null)
        {
            var c = existing.GetComponent<BoxCollider>();
            if (c != null) return c;
        }

        Vector3 center = vcam.transform.position;
        float halfX = defaultHalfSize, halfZ = defaultHalfSize;

        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            foreach (var col in FindObjectsByType<Collider>(FindObjectsInactive.Include))
            {
                string n = col.gameObject.name.ToLower();
                if (n.Contains("ground") || n.Contains("floor") || n.Contains("arena"))
                {
                    ground = col.gameObject;
                    break;
                }
            }
        }

        if (ground != null)
        {
            var gc = ground.GetComponent<Collider>();
            if (gc != null)
            {
                center = gc.bounds.center;
                halfX = gc.bounds.extents.x;
                halfZ = gc.bounds.extents.z;
            }
            else
            {
                center = ground.transform.position;
                halfX = Mathf.Max(ground.transform.localScale.x * 0.5f, defaultHalfSize);
                halfZ = Mathf.Max(ground.transform.localScale.z * 0.5f, defaultHalfSize);
            }
        }

        GameObject boundsGo = new GameObject("[CameraBounds]");
        var box = boundsGo.AddComponent<BoxCollider>();
        center.y = vcam.transform.position.y;
        box.center = Vector3.zero;
        box.size = new Vector3(halfX * 2f, boundsHeight, halfZ * 2f);
        box.isTrigger = true;
        box.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        boundsGo.transform.position = center;
        return box;
    }
}
