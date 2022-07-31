using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileableBase))]
public class Tileable : Editor
{
    TileableBase tileBase;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Snap to Position"))
        {
            tileBase.SnapToRoot();
        }
    }

    private void OnEnable()
    {
        tileBase = (TileableBase)target;
    }
}
