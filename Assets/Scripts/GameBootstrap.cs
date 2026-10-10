using UnityEngine;

public static class GameBootstrap
{
    static bool _booted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (_booted) return;
        _booted = true;

        if (SettingsManager.Instance == null)
        {
            var go = new GameObject("[SettingsManager]");
            go.AddComponent<SettingsManager>();
        }

        if (AudioManager.Instance == null)
        {
            var go = new GameObject("[AudioManager]");
            go.AddComponent<AudioManager>();
        }

        if (OptionsMenuUI.Instance == null)
        {
            var go = new GameObject("[OptionsMenu]");
            go.AddComponent<OptionsMenuUI>();
        }

        if (FPSDebugPanel.Instance == null)
        {
            var go = new GameObject("[FPSDebugPanel]");
            go.AddComponent<FPSDebugPanel>();
        }
    }
}
