using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;

public class BuildPipelineScript
{
    private static readonly string[] SceneNames = {
        "MainMenu",
        "Level1_Warmup",
        "Level2_Tactical",
        "Level3_Showdown"
    };

    private static readonly string ScenesPath = "Assets/Scenes";
    private static readonly string BuildPath  = "Builds/TankBattleground";

    [MenuItem("TankBattleground/Build Windows Standalone")]
    public static void BuildWindows()
    {

        EnsureScenesExist();

        var editorScenes = new EditorBuildSettingsScene[SceneNames.Length];
        for (int i = 0; i < SceneNames.Length; i++)
        {
            string path = $"{ScenesPath}/{SceneNames[i]}.unity";
            editorScenes[i] = new EditorBuildSettingsScene(path, true);
        }
        EditorBuildSettings.scenes = editorScenes;

        Directory.CreateDirectory(BuildPath);

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes          = GetScenePaths(),
            locationPathName = $"{BuildPath}/TankBattleground.exe",
            target           = BuildTarget.StandaloneWindows64,
            options          = BuildOptions.None
        };

        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(opts);
        UnityEditor.Build.Reporting.BuildSummary summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[Build] SUCCESS — {summary.totalSize / 1024 / 1024} MB, " +
                      $"time: {summary.totalTime.TotalSeconds:F1}s, " +
                      $"path: {BuildPath}");
            EditorUtility.RevealInFinder(BuildPath);
        }
        else
        {
            Debug.LogError($"[Build] FAILED — {summary.totalErrors} errors. " +
                           $"Check the Console for details.");
        }
    }

    [MenuItem("TankBattleground/Register Scenes in Build Settings Only")]
    public static void RegisterScenesOnly()
    {
        EnsureScenesExist();

        var editorScenes = new EditorBuildSettingsScene[SceneNames.Length];
        for (int i = 0; i < SceneNames.Length; i++)
        {
            string path = $"{ScenesPath}/{SceneNames[i]}.unity";
            editorScenes[i] = new EditorBuildSettingsScene(path, true);
        }
        EditorBuildSettings.scenes = editorScenes;
        Debug.Log("[Build] Build Settings scenes updated.");
    }

    private static void EnsureScenesExist()
    {
        foreach (string name in SceneNames)
        {
            string path = $"{ScenesPath}/{name}.unity";
            if (!File.Exists(path))
            {

                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(scene, path);
                EditorSceneManager.CloseScene(scene, true);
                Debug.Log($"[Build] Created placeholder scene: {path}");
            }
        }
        AssetDatabase.Refresh();
    }

    private static string[] GetScenePaths()
    {
        string[] paths = new string[SceneNames.Length];
        for (int i = 0; i < SceneNames.Length; i++)
            paths[i] = $"{ScenesPath}/{SceneNames[i]}.unity";
        return paths;
    }

    [MenuItem("TankBattleground/Package Build as ZIP")]
    public static void PackageBuildAsZip()
    {
        string zipName = "TankBattleground_Windows.zip";
        if (!Directory.Exists(BuildPath))
        {
            Debug.LogError($"[Zip] Build directory not found: {BuildPath}. Run Build Windows first.");
            return;
        }

        if (File.Exists(zipName)) File.Delete(zipName);

        System.IO.Compression.ZipFile.CreateFromDirectory(BuildPath, zipName);
        Debug.Log($"[Zip] Created: {Path.GetFullPath(zipName)}");
        EditorUtility.RevealInFinder(zipName);
    }
}

