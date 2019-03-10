using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BlockDatabaseScriptableObject))]
public class BlockDatabaseCustomInspector : Editor {

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open Block Database"))
        {
            BlockDatabaseEditorWindow.OpenWindow();
        }
		//DrawDefaultInspector();
    }
}
