using UnityEditor;
using UnityEditor.ShortcutManagement;
using System.Linq;

[InitializeOnLoad]
public class SwitchShortcutsProfileOnPlay
{
    private const string PlayingProfileId = "Playing";
    private static string _activeProfileId = "Default";

    static SwitchShortcutsProfileOnPlay()
    {
        EditorApplication.playModeStateChanged += DetectPlayModeState;
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

    private static void OnExitingPlayMode()
    {
        if(ShortcutManager.instance.activeProfileId != PlayingProfileId)
            return;

        ShortcutManager.instance.activeProfileId = _activeProfileId;
    }

    private static void OnEnteredPlayMode()
    {
        _activeProfileId = ShortcutManager.instance.activeProfileId;
        if (_activeProfileId.Equals(PlayingProfileId))
            return; 

        if (!ShortcutManager.instance.GetAvailableProfileIds().Any(p => p == PlayingProfileId))
            ShortcutManager.instance.CreateProfile(PlayingProfileId);

        ShortcutManager.instance.activeProfileId = PlayingProfileId;
        ShortcutManager.instance.RebindShortcut("Main Menu/Edit/Undo", ShortcutBinding.empty);
        ShortcutManager.instance.RebindShortcut("Main Menu/Edit/Redo", ShortcutBinding.empty);
    }
}