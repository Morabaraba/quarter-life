using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DunGenMap))]
public class DunGenMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DunGenMap script = (DunGenMap)target;
        if (GUILayout.Button("Reload Map"))
        {
            script.StartLoad();
        }
    }
}
