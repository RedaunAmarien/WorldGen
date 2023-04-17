using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    [SerializeField]
    private Vector3Int coordinates;

    public HexCoordinates(int x, int z)
    {
        coordinates = new Vector3Int(x, -x-z, z);
    }

    public int X
    {
        get
        {
            return coordinates.x;
        }
    }

    public int Z
    {
        get
        {
            return coordinates.z;
        }
    }

    public int Y
    {
        get
        {
            return -coordinates.x - coordinates.z;
        }
    }

    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        return new HexCoordinates(x - z / 2, z);
    }

    public override string ToString()
    {
        return "(" + coordinates.x.ToString() + ", " + Y.ToString() + ", " + coordinates.z.ToString() + ")";
    }

    public string ToStringOnSeparateLines()
    {
        return coordinates.x.ToString() + "\n" + Y.ToString() + "\n" + coordinates.z.ToString();
    }
}
