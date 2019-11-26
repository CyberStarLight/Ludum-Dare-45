// Creates a prefab at the given path.
// If a prefab already exists it asks if you want to replace it

using UnityEngine;
using UnityEditor;

public class SelectionToPrefabs : EditorWindow
{
    [MenuItem("GameObject/Create Prefabs", priority = 0)]
    static void CreatePrefab()
    {
        GameObject[] objs = Selection.gameObjects;
        int createdCount = 0;
        int skippedCount = 0;

        foreach (GameObject go in objs)
        {
            if(!AssetDatabase.IsValidFolder("Assets/Prefabs (Generated)"))
                AssetDatabase.CreateFolder("Assets", "Prefabs (Generated)");

            string localPath = "Assets/Prefabs (Generated)/" + go.name + ".prefab";
            if (AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject)))
            {
                if (EditorUtility.DisplayDialog("Are you sure?",
                        "\"" + go.name + "\" already exists. Do you want to overwrite it?",
                        "Yes",
                        "No"))
                {
                    CreateNew(go, localPath);
                    createdCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
            else
            {
                CreateNew(go, localPath);
                createdCount++;
            }
        }

        EditorUtility.DisplayDialog("Done", "Done.\nCreated: " + createdCount + "\nSkipped: " + skippedCount + "\n\nPrefabs created at: \"Assets/Prefabs (Generated)/\"", "Ok");
    }

    // Disable the menu item if no selection is in place
    [MenuItem("Examples/Create Empty Prefab", true)]
    static bool ValidateCreatePrefab()
    {
        return Selection.activeGameObject != null;
    }

    static void CreateNew(GameObject obj, string localPath)
    {
        Object prefab = PrefabUtility.CreateEmptyPrefab(localPath);
        PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
    }
}