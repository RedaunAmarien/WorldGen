using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

[System.Serializable]
public class Coordinates
{
    public float latitude;
    public float longitude;
    public Vector3 localPosition;
    public readonly int u = -1;
    public readonly int v = -1;
    public readonly int quadrant = -1;

    public Coordinates(float longitude, float latitude)
    {
        this.longitude = longitude;
        this.latitude = latitude;

        // Convert to radians
        float lati = this.latitude * Mathf.Deg2Rad;
        float longi = this.longitude * -Mathf.Deg2Rad;

        // Find XYZ from radians
        float z = Mathf.Cos(lati) * Mathf.Cos(longi);
        float x = Mathf.Cos(lati) * Mathf.Sin(longi);
        float y = Mathf.Sin(lati);

        localPosition = new Vector3(x, y, z);
    }

    public Coordinates(Vector3 localPosition)
    {
        this.localPosition = localPosition;

        float lati = -Mathf.Acos(this.localPosition.y) * Mathf.Rad2Deg + 90;
        float longi = -Mathf.Atan2(this.localPosition.x, this.localPosition.z) * Mathf.Rad2Deg + 180;

        if (lati > 90)
        {
            lati = 90 - (lati - 90);
        }
        else if (lati < -90)
        {
            lati = -90 - (lati + 90);
        }

        if (longi > 180)
        {
            longi = -(180 - (longi - 180));
        }
        else if (longi < -180)
        {
            longi = -(-180 - (longi + 180));
        }

        latitude = lati;
        longitude = longi;
    }

    public Coordinates(Vector3Int UVQPosition)
    {
        u = UVQPosition.x;
        v = UVQPosition.y;
        quadrant = UVQPosition.z;

        Vector2Int quadMapSize = GameRam.planet.worldManager.quadMapSize;

        // Force center of quadrant to be 0,0 by default and normalize between -1 and 1.
        float px = ((float)u - quadMapSize.x / 2f) / (quadMapSize.x / 2f);
        float py = ((float)v - quadMapSize.y / 2f) / (quadMapSize.y / 2f);

        // Initialize offset for quadrant.
        Vector3 newCoord = Vector3.zero;

        switch (quadrant)
        {
            case 0: //Origin Face
                newCoord.x = 1;
                newCoord.y = py;
                newCoord.z = px;
                break;
            case 1: //Western Face
                newCoord.x = px;
                newCoord.y = py;
                newCoord.z = -1;
                break;
            case 2: //Dateline Face
                newCoord.x = -1;
                newCoord.y = py;
                newCoord.z = -px;
                break;
            case 3: //Eastern Face
                newCoord.x = -px;
                newCoord.y = py;
                newCoord.z = 1;
                break;
            case 4: //Southern Face
                newCoord.x = py;
                newCoord.y = -1;
                newCoord.z = px;
                break;
            case 5: //Northern Face
                newCoord.x = -py;
                newCoord.y = 1;
                newCoord.z = px;
                break;

                //default:
                //    Debug.LogError("Conversion.Coordinates.UVQtoXYZ failed. Quadrant " + quadrant + " not recognized.");
                //    localPosition = Vector3.zero;
        }
        newCoord.Normalize();

        localPosition = newCoord;

        float lati = -Mathf.Acos(localPosition.y) * Mathf.Rad2Deg + 90;
        float longi = -Mathf.Atan2(localPosition.x, localPosition.z) * Mathf.Rad2Deg + 180;

        if (lati > 90)
        {
            lati = 90 - (lati - 90);
        }
        else if (lati < -90)
        {
            lati = -90 - (lati + 90);
        }

        if (longi > 180)
        {
            longi = -(180 - (longi - 180));
        }
        else if (longi < -180)
        {
            longi = -(-180 - (longi + 180));
        }

        latitude = lati;
        longitude = longi;
    }

    public override string ToString()
    {
        return string.Format("LL: {0}, {1}\nXYZ: {2}\nUVQ: {3}, {4}, {5}", latitude, longitude, localPosition.ToString(), u, v, quadrant);
    }
}
