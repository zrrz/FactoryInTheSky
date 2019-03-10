using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemDatabaseScriptableObject))]
public class ItemDatabaseCustomInspector : Editor {

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open Item Database"))
        {
            ItemDatabaseEditorWindow.OpenWindow();
        }
        //DrawDefaultInspector();
    }
}
