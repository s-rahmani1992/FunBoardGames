using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSetting : ScriptableObject
{
    [SerializeField] SceneAsset awakeScene;

    const string settingPath = "Assets/SceneSetting/Editor/SceneSetting.asset";
    const string settingInitializeKey = "scene_setting_initialized";

    [InitializeOnLoadMethod]
    static void SetMasterScene()
    {
        if (SessionState.GetBool(settingInitializeKey, false))
            return;

        SceneSetting setting = GetSetting();
        EditorSceneManager.playModeStartScene = setting.awakeScene;
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
        SessionState.SetBool(settingInitializeKey, true);
    }

    static void PlayModeStateChanged(PlayModeStateChange playModeStateChange)
    {
        if (playModeStateChange != PlayModeStateChange.ExitingEditMode) return;
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.isDirty == false) return;
        EditorSceneManager.SaveScene(activeScene, activeScene.path);
    }

    private void OnValidate()
    {
        EditorSceneManager.playModeStartScene = awakeScene;
    }

    static SceneSetting GetSetting()
    {
        SceneSetting setting = AssetDatabase.LoadAssetAtPath<SceneSetting>(settingPath);

        if (setting == null)
        {
            setting = CreateInstance<SceneSetting>();
            AssetDatabase.CreateAsset(setting, settingPath);
        }

        return setting;
    }

    [MenuItem("Tools/Edit Scene Setting")]
    static void SelectSceneSetting()
    {
        var setting = GetSetting();
        Selection.activeObject = setting;
    }
}
