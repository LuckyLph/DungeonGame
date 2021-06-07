using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Generate"))
        {
            var generator = GameObject.FindObjectOfType<DungeonGenerator>();
            generator.Generate();
        }
        DrawDefaultInspector();
    }
}