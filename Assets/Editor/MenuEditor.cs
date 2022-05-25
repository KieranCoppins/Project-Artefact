using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// A custom inspector script to allow pushing the menu buttons without needing to wear the hololens for testing
/// </summary>
[CustomEditor(typeof(MenuManager))]
public class MenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MenuManager manager = (MenuManager)target;
        if (GUILayout.Button("Place Viewport Placer"))
        {
            manager.placeViewportPlacer();
        }
        if (GUILayout.Button("Place Viewport"))
        {
            manager.placeViewport();
        }
    }
}
