using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileProperties : ScriptableObject
{

    public bool buildable, walkable, occupied, sailable, wetBuildable;
    public Vector2Int location;
    public int height;

    public void OnGeneration(Vector2Int newLocation, int newHeight)
    {
        SetType(newHeight);
        location = newLocation;
        // Debug.LogFormat("Tile started at {0}, {1} with height of {2}.", manager.x, manager.y, newType);
    }

    public void SetType(int newHeight) {
        height = newHeight;
        switch (height)
        {
            case 0:
                walkable = false;
                buildable = false;
                sailable = true;
                wetBuildable = true;
            break;
            case 1:
                walkable = false;
                buildable = false;
                sailable = true;
                wetBuildable = true;
            break;
            case 2:
                walkable = true;
                buildable = false;
                sailable = true;
                wetBuildable = true;
            break;
            case 3:
                walkable = true;
                buildable = true;
                sailable = false;
                wetBuildable = false;
            break;
            case 4:
                walkable = true;
                buildable = true;
                sailable = false;
                wetBuildable = false;
            break;
            case 5:
                walkable = true;
                buildable = true;
                sailable = false;
                wetBuildable = false;
            break;
            case 6:
                walkable = true;
                buildable = true;
                sailable = false;
                wetBuildable = false;
            break;
            case 7:
                walkable = true;
                buildable = true;
                sailable = false;
                wetBuildable = false;
            break;
            case 8:
                walkable = true;
                buildable = true;
                sailable = false;
                wetBuildable = false;
            break;
            default:
                Debug.LogErrorFormat("Tile at x{0}, z{1} attempted to set height to \"{2},\" which is out of range.", location.x, location.y, newHeight);
            break;
        }
    }
}
