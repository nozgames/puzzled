using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public static class AutoLoadSceneZero {
    const string autoLoadSceneZeroMenuStr = "Edit/Autoload Scene 0 &p";

    static bool autoLoadSceneZero {
        get {
            return EditorPrefs.HasKey(autoLoadSceneZeroMenuStr) && EditorPrefs.GetBool(autoLoadSceneZeroMenuStr);
        }
        set {
            EditorPrefs.SetBool(autoLoadSceneZeroMenuStr, value);
        }
    }

    [MenuItem(autoLoadSceneZeroMenuStr, false, 150)]
    static void AutoLoadSceneZeroCheckMenu() {
        autoLoadSceneZero = !autoLoadSceneZero;
        Menu.SetChecked(autoLoadSceneZeroMenuStr, autoLoadSceneZero);

        ShowNotifyOrLog(autoLoadSceneZero ? "Autoload Scene 0 enabled" : "Autoload Scene 0 disabled");
    }

    // The menu won't be gray out, we use this validate method for update check state
    [MenuItem(autoLoadSceneZeroMenuStr, true)]
    static bool PlayFromFirstSceneCheckMenuValidate() {
        Menu.SetChecked(autoLoadSceneZeroMenuStr, autoLoadSceneZero);
        return true;
    }

    // This method is called before any Awake. It's the perfect callback for this feature
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void LoadFirstScene() {
        if (!autoLoadSceneZero)
            return;

        if (EditorBuildSettings.scenes.Length == 0) {
            Debug.LogWarning("The scene build list is empty. Can't play from first scene.");
            return;
        }

        // Ensure we are not already on scene zero..
        if (SceneManager.GetActiveScene().buildIndex == 0)
            return;

        // Disable all the objects in the current scene so they dont 
        foreach (GameObject go in Object.FindObjectsOfType<GameObject>())
            go.SetActive(false);

        SceneManager.LoadScene(0);
    }

    static void ShowNotifyOrLog(string msg) {
        if (Resources.FindObjectsOfTypeAll<SceneView>().Length > 0)
            EditorWindow.GetWindow<SceneView>().ShowNotification(new GUIContent(msg));
        else
            Debug.Log(msg); // When there's no scene view opened, we just print a log
    }
}