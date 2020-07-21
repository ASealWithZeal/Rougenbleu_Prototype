using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Grid))]
public class GridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Grid instance = (Grid)target;

        instance.gridSpace = (GameObject)EditorGUILayout.ObjectField("Grid Space", instance.gridSpace, typeof(GameObject), true);
        instance.levelImage = (Texture2D)EditorGUILayout.ObjectField("Level Image", instance.levelImage, typeof(Texture2D), true);

        if (GUILayout.Button("Generate Grid From Image"))
            instance.GenerateGrid();
    }
}
