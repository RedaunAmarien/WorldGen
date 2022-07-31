using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator creator;
    Path Path
    {
        get
        {
            return creator.path;
        }
    }

    const float segmentAddDist = 0.1f;
    int selectedIndex = -1;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Create Path"))
        {
            Undo.RecordObject(creator, "Create Path");
            creator.CreatePath();
        }

        bool isClosed = GUILayout.Toggle(Path.IsClosed, "Close Path");
        if (isClosed != Path.IsClosed)
        {
            Undo.RecordObject(creator, "Toggle Close Path");
            Path.IsClosed = isClosed;
        }

        bool autoSetTangents = GUILayout.Toggle(Path.AutoSetTangents, "Auto Set Tangents");
        if (autoSetTangents != Path.AutoSetTangents)
        {
            Undo.RecordObject(creator, "Toggle Auto Set Tangents");
            Path.AutoSetTangents = autoSetTangents;
        }

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        Input();
        Draw();
    }

    void Input()
    {
        Event guiEvent = Event.current;
        Vector3 mousePos = Vector3.zero;
        Vector3 normal = Vector3.up;
        if (HandleUtility.PlaceObject(guiEvent.mousePosition, out mousePos, out normal))
        {
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
            {
                if (selectedIndex != -1)
                {
                    Undo.RecordObject(creator, "Split Segment");
                    Path.SplitSegment(mousePos, selectedIndex);

                }
                else if (!Path.IsClosed)
                {
                    Undo.RecordObject(creator, "Add Segment");
                    Path.AddSegment(mousePos);
                }
            }
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDistAnchor = 1f;
            int closestIndex = -1;

            for (int i = 0; i < Path.NumPoints; i+=3)
            {
                float dist = Vector3.Distance(mousePos, Path[i]);
                if (dist < minDistAnchor)
                {
                    minDistAnchor = dist;
                    closestIndex = i;
                }
            }

            if (closestIndex != -1)
            {
                Undo.RecordObject(creator, "Delete Segment");
                Path.DeleteSegment(closestIndex);
            }
        }

        if (guiEvent.type == EventType.MouseMove)
        {
            float minSegDist = segmentAddDist;
            int newIndex = -1;
            for (int i = 0; i < Path.NumSegments; i++)
            {
                Vector3[] points = Path.GetPointsInSegment(i);
                float dst = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);
                if (dst < minSegDist)
                {
                    minSegDist = dst;
                    newIndex = i;
                }
            }

            if (newIndex != selectedIndex)
            {
                selectedIndex = newIndex;
                HandleUtility.Repaint();
            }
        }
    }

    private void Draw()
    {
        for (int i = 0; i < Path.NumSegments; i++)
        {
            Vector3[] points = Path.GetPointsInSegment(i);
            if (creator.displayControls)
            {
                Handles.color = Color.black;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
            }
            Color segmentCol = (i == selectedIndex && Event.current.shift) ? creator.selectedCol: creator.segmentCol;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentCol, null, 2);
        }

        for (int i = 0; i < Path.NumPoints; i++)
        {
            if (i % 3 != 0 && !creator.displayControls)
                continue;
            Handles.color = (i % 3 == 0) ? creator.anchorCol : creator.controlCol;
            float size = (i % 3 == 0) ? creator.anchorDiameter : creator.controlDiameter;
            Handles.CapFunction capType = (i % 3 == 0) ? creator.anchorCap : creator.controlCap ;
            Vector3 newPos = Handles.FreeMoveHandle(Path[i], Quaternion.identity, size, Vector3.zero, capType);
            if (Path[i] != newPos)
            {
                Undo.RecordObject(creator, "Move Point");
                Path.MovePoint(i, newPos);
            }
        }
    }

    private void OnEnable()
    {
        creator = (PathCreator)target;
        if (creator.path == null)
        {
            creator.CreatePath();
        }
    }
}
