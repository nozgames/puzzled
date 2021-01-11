using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using System.Linq;

[InitializeOnLoad]
public class SwitchShortcutsProfileOnPlay
{
    private const string PlayingProfileId = "Playing";
    private static string _activeProfileId = "Default";

    static SwitchShortcutsProfileOnPlay()
    {
        EditorApplication.playModeStateChanged += DetectPlayModeState;
        EditorApplication.update += Update;
    }

    private static bool _gameViewFocused = false;
    private static EditorWindow _lastFocused = null;

    private static void Update()
    {
        if(EditorWindow.focusedWindow != _lastFocused)
        {
            _gameViewFocused = EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType().FullName == "UnityEditor.GameView";
            _lastFocused = EditorWindow.focusedWindow;
            UpdateProfile();


            if (EditorWindow.focusedWindow != null)
                Debug.Log($"{EditorWindow.focusedWindow.GetType()} / {_gameViewFocused}");
        }
    }

    private static void DetectPlayModeState(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredPlayMode:
                OnEnteredPlayMode();
                break;
            case PlayModeStateChange.ExitingPlayMode:
                OnExitingPlayMode();
                break;
        }
    }

    private static void OnExitingPlayMode() => UpdateProfile();

    private static void OnEnteredPlayMode() => UpdateProfile();

    private static void UpdateProfile()
    {
        if(EditorApplication.isPlaying && _gameViewFocused)
        {
            var activeProfileId = ShortcutManager.instance.activeProfileId;
            if (activeProfileId.Equals(PlayingProfileId))
                return;

            _activeProfileId = activeProfileId;

            if (!ShortcutManager.instance.GetAvailableProfileIds().Any(p => p == PlayingProfileId))
                ShortcutManager.instance.CreateProfile(PlayingProfileId);

            ShortcutManager.instance.activeProfileId = PlayingProfileId;

            if (ShortcutManager.instance.GetShortcutBinding("Animation/Play Animation").keyCombinationSequence != ShortcutBinding.empty.keyCombinationSequence)
                foreach (var shortcut in ShortcutManager.instance.GetAvailableShortcutIds())
                    ShortcutManager.instance.RebindShortcut(shortcut, ShortcutBinding.empty);
        }
        else
        {
            if (ShortcutManager.instance.activeProfileId != PlayingProfileId)
                return;

            ShortcutManager.instance.activeProfileId = _activeProfileId;
        }
    }
}