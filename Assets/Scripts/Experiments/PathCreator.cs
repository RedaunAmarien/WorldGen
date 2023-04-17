using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [HideInInspector]
    public Path path;

    public Color anchorCol = Color.blue;
    public Color controlCol = Color.white;
    public Color segmentCol = Color.green;
    public Color selectedCol = Color.yellow;
    [SerializeField] public UnityEditor.Handles.CapFunction anchorCap = UnityEditor.Handles.SphereHandleCap;
    [SerializeField] public UnityEditor.Handles.CapFunction controlCap = UnityEditor.Handles.CubeHandleCap;
    public float anchorDiameter = 0.25f;
    public float controlDiameter = 0.125f;
    public bool displayControls = true;


    public void CreatePath()
    {
        path = new Path(transform.position);
    }

    private void Reset()
    {
        CreatePath();
    }
}
