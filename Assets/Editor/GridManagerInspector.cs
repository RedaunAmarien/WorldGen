using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridManagerInspector : Editor
{
    GridManager manager;
    public int x, z;

    public override void OnInspectorGUI()
    {
        manager = (GridManager)target;
        GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate All"))
            {
                manager.GenerateNew(true, true, true);
            }

            if (GUILayout.Button("Cleanup All"))
            {
                manager.Cleanup();
            }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

            if (GUILayout.Button("Gen Terrain"))
            {
                manager.GenerateNew(true, false, false);
            }

            if (GUILayout.Button("Gen Tiles"))
            {
                manager.GenerateNew(false, true, false);
            }

            if (GUILayout.Button("Gen Players"))
            {
                manager.GenerateNew(false, false, true);
            }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

            x = EditorGUILayout.IntField("x", x);
            z = EditorGUILayout.IntField("z", z);

        GUILayout.EndHorizontal();

            if (GUILayout.Button("Print Tile Info"))
            {
                if (manager.tileProps[x, z] != null)
                    Debug.LogFormat("Tile x{0} z{1} is at height {2}.\nWalk?: {3} - Build?: {4} - Occupied?: {5}", manager.tileProps[x, z].location.x, manager.tileProps[x,z].location.y, manager.tileProps[x, z].height, manager.tileProps[x,z].walkable, manager.tileProps[x,z].buildable, manager.tileProps[x,z].occupied);
                else
                {
                    Debug.LogWarningFormat("Tile {0}, {1} does not appear to exist yet.", x, z);
                }
            }


        base.OnInspectorGUI();
    }
}
