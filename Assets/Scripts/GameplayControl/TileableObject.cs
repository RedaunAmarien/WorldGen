using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TileableObject : MonoBehaviour
{
    public enum RootCorner
    {
        Northeast, Southeast, Southwest, Northwest, Center
    };
    public RootCorner rootCorner;

    public Vector2Int objectSize;
    public bool hasHitbox;
    public Vector2 hitboxSize;
    public bool affectsMoveSpeed;
    public float moveSpeedMult;
}
