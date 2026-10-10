using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class WallCollisionFixer
{
    [MenuItem("Tools/Fix Wall Collisions (Remove Scenery Rigidbodies)")]
    public static void FixWallCollisions()
    {
        int removed = 0;
        int kept = 0;

        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                GameObject go = t.gameObject;

                if (go.GetComponent<TankController>() != null) { kept++; continue; }
                if (go.GetComponent<EnemyTankAI>() != null) { kept++; continue; }
                if (go.GetComponent<Projectile>() != null) { kept++; continue; }
                if (go.GetComponent<Rigidbody>() == null && go.GetComponent<Rigidbody2D>() == null)
                    continue;

                Undo.RecordObject(go, "Remove Scenery Rigidbody");

                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb != null) { Object.DestroyImmediate(rb); removed++; }

                Rigidbody2D rb2 = go.GetComponent<Rigidbody2D>();
                if (rb2 != null) { Object.DestroyImmediate(rb2); removed++; }
            }
        }

        if (removed > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[WallCollisionFixer] Removed {removed} scenery Rigidbody component(s). " +
                      $"Kept {kept} tank/projectile Rigidbody component(s). " +
                      $"Now SAVE the scene (Ctrl+S) to keep the change.");
            EditorUtility.DisplayDialog(
                "Wall Collisions Fixed",
                $"Removed {removed} Rigidbody component(s) from walls/scenery.\n\n" +
                $"Your tanks/projectiles were left untouched.\n\n" +
                $"IMPORTANT: press Ctrl+S to save the scene.",
                "OK");
        }
        else
        {
            Debug.Log("[WallCollisionFixer] No scenery Rigidbodies found — nothing to remove.");
            EditorUtility.DisplayDialog(
                "Wall Collisions",
                "No extra Rigidbodies were found on walls/scenery.\n\n" +
                "If the tank still passes through, make sure each wall has a Collider " +
                "and that 'Is Trigger' is UNCHECKED.",
                "OK");
        }
    }
}
