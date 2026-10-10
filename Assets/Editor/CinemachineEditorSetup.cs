using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public class CinemachineEditorSetup : Editor
{
    const string VcamName = "CM_PlayerFollowCamera";

    [MenuItem("TankBattleground/Setup Cinemachine Camera (Open Scene Required)")]
    public static void SetupCinemachine()
    {

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("[Cinemachine Setup] No Main Camera found in scene. Open a gameplay scene first.");
            return;
        }

        if (mainCam.GetComponent<CinemachineBrain>() == null)
            Undo.AddComponent(mainCam.gameObject, typeof(CinemachineBrain));

        TankController player = FindAnyObjectByType<TankController>();
        if (player == null)
            Debug.LogWarning("[Cinemachine Setup] No TankController found. Camera tracking target will be left unset.");

        CinemachineCamera vcam = FindExistingVcam();
        GameObject vcamGO;
        bool created;
        if (vcam == null)
        {
            vcamGO = new GameObject(VcamName);
            Undo.RegisterCreatedObjectUndo(vcamGO, "Create Cinemachine Camera");
            vcam = Undo.AddComponent<CinemachineCamera>(vcamGO);
            created = true;
        }
        else
        {
            vcamGO = vcam.gameObject;
            created = false;
        }

        if (player != null)
        {
            Undo.RecordObject(vcam, "Set Cinemachine Camera Target");
            vcam.Follow = player.transform;
            vcam.LookAt = player.transform;
            EditorUtility.SetDirty(vcam);
        }

        var follow = vcamGO.GetComponent<CinemachineFollow>();
        if (follow == null)
            follow = Undo.AddComponent<CinemachineFollow>(vcamGO);
        Undo.RecordObject(follow, "Configure Cinemachine Follow");
        follow.TrackerSettings.BindingMode = Unity.Cinemachine.TargetTracking.BindingMode.WorldSpace;
        follow.FollowOffset = new Vector3(0f, 25f, -5f);

        if (vcamGO.GetComponent<CinemachineHardLookAt>() == null)
            Undo.AddComponent<CinemachineHardLookAt>(vcamGO);

        if (created)
        {
            vcamGO.transform.position = new Vector3(0f, 25f, -5f);
            vcamGO.transform.rotation = Quaternion.Euler(70f, 0f, 0f);
        }

        if (vcamGO.GetComponent<CinemachineImpulseListener>() == null)
            Undo.AddComponent(vcamGO, typeof(CinemachineImpulseListener));

        var thrower = FindAnyObjectByType<ThrowProjectile>();
        if (thrower != null)
        {
            var source = thrower.GetComponent<CinemachineImpulseSource>();
            if (source == null)
                source = Undo.AddComponent<CinemachineImpulseSource>(thrower.gameObject);

            var so = new SerializedObject(thrower);
            var prop = so.FindProperty("cinemachineImpulseSource");
            if (prop != null)
            {
                prop.objectReferenceValue = source;
                so.ApplyModifiedProperties();
            }
        }
        else
        {
            Debug.Log("[Cinemachine Setup] No ThrowProjectile in scene — skipping Impulse Source. " +
                "Add a CinemachineImpulseSource manually if you want camera shake on fire.");
        }

        Selection.activeGameObject = vcamGO;

        Debug.Log("[Cinemachine Setup] Done — Cinemachine 3 (Unity.Cinemachine) top-down follow configured on '"
            + vcamGO.name + "'.\n" +
            "MANUAL: disable any legacy 'TopDownCameraFollow' on the Main Camera so it doesn't fight Cinemachine.\n" +
            "MANUAL: tune FollowOffset / BindingMode and the Impulse Listener Gain to taste, and optionally add a " +
            "CinemachineConfiner bound to the arena collider to keep the camera inside the level.");
    }

    static CinemachineCamera FindExistingVcam()
    {
        var all = FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include);
        foreach (var c in all)
            if (c.name == VcamName)
                return c;
        return all.Length > 0 ? all[0] : null;
    }
}

