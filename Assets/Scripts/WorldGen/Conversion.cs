using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public static class Conversion
{
    public static class Coordinate
    {

        ////////////////////////////
        // COORDINATE CONVERSIONS //
        ////////////////////////////

        // public static async Task<Vector3Int> LLtoUVQ (float lng, float lat)
        // {
        //     Vector3Int result = Vector3Int.zero;
        //     await new WaitForEndOfFrame();
        //     return result;
        // }

        public static async Task<Vector3> LLtoXYZ (float lng, float lat)
        {
            // Rotate to proper origin
            lng -= 180;

            // Convert to radians
            lat *= Mathf.Deg2Rad;
            lng *= -Mathf.Deg2Rad;

            // Find XYZ from radians
            float z = Mathf.Cos(lat) * Mathf.Cos(lng);
            float x = Mathf.Cos(lat) * Mathf.Sin(lng);
            float y = Mathf.Sin(lat);

            Vector3 result = new Vector3(x, y, z);
            await new WaitForEndOfFrame();
            return result;
        }

        public static async Task<Vector3> UVQtoXYZ (Vector3Int uvqCoord, Vector2Int quadMapSize)
        {
            // Force center of quadrant to be 0,0 by default and normalize between -1 and 1.
            float px = (uvqCoord.x - quadMapSize.x / 2f) / (quadMapSize.x / 2f);
            float py = (uvqCoord.y - quadMapSize.y / 2f) / (quadMapSize.y / 2f);

            // Initialize offset for quadrant.
            Vector3 newCoord = Vector3.zero;

            switch (uvqCoord.z)
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

                default:
                    Debug.LogError("Conversion.Coordinate.UVQtoXYZ failed. Quadrant " + uvqCoord.z + " not recognized.");
                break;
            }

            Vector3 result = newCoord.normalized;
            await new WaitForEndOfFrame();
            return result;
        }

        // public static async Task<Vector2> UVQtoLL (Vector3Int uvqCoord, Vector2Int quadMapSize)
        // {
        //     Vector2 result = Vector2.zero;
        //     await new WaitForEndOfFrame();
        //     return result;
        // }

        public static async Task<Vector2> XYZtoLL (Vector3 coordinates)
        {
            float lat = -Mathf.Acos(coordinates.y) * Mathf.Rad2Deg + 90;
            float lng = -Mathf.Atan2(coordinates.x, coordinates.z) * Mathf.Rad2Deg + 180;

            if (lat > 90)
            {
                lat = 90-(lat-90);
            }
            else if (lat < -90)
            {
                lat = -90-(lat+90);
            }

            if (lng > 180)
            {
                lng = -(180-(lng-180));
            }
            else if (lng < -180)
            {
                lng = -(-180-(lng+180));
            }

            Vector2 result = new Vector2(lng, lat);
            await new WaitForEndOfFrame();
            return result;
        }

        // public static async Task<Vector3Int> XYZtoUVQ (Vector3 coordinates)
        // {
        //     Vector3Int result = Vector3Int.zero;;
        //     await new WaitForEndOfFrame();
        //     return result;
        // }
    }
}
