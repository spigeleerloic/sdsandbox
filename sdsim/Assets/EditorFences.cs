/*using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlaceFences))]
public class EditorFences : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlaceFences placeFencesScript = (PlaceFences)target;

        if (GUILayout.Button("Place Fences and Save Scene"))
        {
            placeFencesScript.PlaceFencesAlongRoad();
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }
    }
}
*/