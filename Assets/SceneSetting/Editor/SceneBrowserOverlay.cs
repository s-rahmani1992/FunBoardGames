using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;

[EditorToolbarElement(id, typeof(SceneView))]
class SceneDropdown : EditorToolbarDropdown
{
    public const string id = "Toolbar/SceneDropdown";

    SceneDropdown()
    {
        text = "Scenes";
        tooltip = "Displays scenes included in build settings.";
        clicked += () => ShowSceneMenu(this);
    }

    void ShowSceneMenu(VisualElement parent)
    {
        var menu = new GenericDropdownMenu();
        
        foreach(var s in EditorBuildSettings.scenes)
        {
            Button button = new(() => EditorSceneManager.OpenScene(s.path));
            button.text = AssetDatabase.LoadAssetAtPath<SceneAsset>(s.path).name;
            button.tooltip = s.path;
            button.style.marginTop = button.style.marginRight = button.style.marginBottom = button.style.marginLeft = 3;
            menu.contentContainer.Add(button);
        }

        menu.DropDown(parent.worldBound, parent);
    }
}

[Overlay(typeof(SceneView), "Scene Browser")]
public class SceneBrowserOverlay : ToolbarOverlay
{ 
    SceneBrowserOverlay() : base(SceneDropdown.id){ }
}
