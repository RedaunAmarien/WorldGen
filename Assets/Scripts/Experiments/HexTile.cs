using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    public const float outerRadius = 1f;
    public const float innerRadius = outerRadius * 0.866025404f;
    public static Vector3[] corners =
    {
        new Vector3( 0, 0, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius)
    };

    public enum Modules
    {
        None, A, B, C, D, E, F, G
    }
    public List<Modules> septantObjects;

    public TextMeshPro coordinateDisplay;
    //public Vector3Int gridCoordinates;
    [SerializeField] public HexCoordinates coordinates;

    private void Start()
    {
        coordinateDisplay.text = coordinates.ToString();
    }
}
