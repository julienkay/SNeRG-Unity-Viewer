using System.IO;

public class ImportContext {

    /// <summary>
    /// True if we are we currently importing a custom scene,
    /// false if it is one of the demo scenes.
    /// </summary>
    public bool CustomScene;

    /// <summary>
    /// The demo scene being imported.
    /// </summary>
    public SNeRGScene Scene;

    /// <summary>
    /// The path to the source files for custom scene imports.
    /// </summary>
    public string CustomScenePath;

    public string SceneName {
        get {
            if (CustomScene) {
                return new DirectoryInfo(CustomScenePath).Name.ToLower();
            } else {
                return Scene.LowerCaseName();
            }

        }
    }

    public string SceneNameUpperCase {
        get {
            if (CustomScene) {
                return new DirectoryInfo(CustomScenePath).Name;
            } else {
                return Scene.Name();
            }

        }
    }
}