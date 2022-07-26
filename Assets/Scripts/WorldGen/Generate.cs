using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public static class Generate
{
    public static FastNoiseLite fnl;

    public static async Task<float> GetNoise(Vector3 xyzPoint)
    {
        await new WaitForEndOfFrame();
        return fnl.GetNoise(xyzPoint.x, xyzPoint.y, xyzPoint.z);
    }

    public static async Task<float> GetNoise(Vector3Int uvqCoord, Vector2Int quadMapSize)
    {
        Vector3 point = await Conversion.Coordinate.UVQtoXYZ(uvqCoord, quadMapSize);
        float value = fnl.GetNoise(point.x, point.y, point.z);
        return value;
    }
    public static async Task<float> GetNoise(Vector3 subUvqCoord, Vector2Int uv)
    {
        Vector3 point = await Conversion.Coordinate.SubUVQtoXYZ(subUvqCoord, uv);
        float value = fnl.GetNoise(point.x, point.y, point.z);
        return value;
    }

    public static async Task<float> GetNoise(Vector2 longLatCoord, Vector2 newCenter)
    {
        Vector3 point = await Conversion.Coordinate.LLtoXYZ(longLatCoord.x, longLatCoord.y);

        point = Quaternion.AngleAxis(-newCenter.y, Vector3.right) * point;
        point = Quaternion.AngleAxis(180-newCenter.x, Vector3.up) * point;

        return fnl.GetNoise(point.x, point.y, point.z);
    }
    
    public static async Task<float[,]> GetNoise(Vector2[,] longlats)
    {
        int xLength = longlats.GetLength(0);
        int yLength = longlats.GetLength(1);

        float[,] noise = new float[xLength, yLength];

        for (int y = 0; y < yLength; y++)
        {
            for (int x = 0; x < xLength; x++)
            {
                Vector3 point = await Conversion.Coordinate.LLtoXYZ(longlats[x,y].x, longlats[x,y].y);
                // Debug.Log(point.ToString());
                noise[x,y] = fnl.GetNoise(point.x, point.y, point.z);
            }
        }
        
        return noise;
    }
    
    public static async Task<float[,]> GetNoise(Vector3Int[,] uvqs, Vector2Int quadMapSize)
    {
        int xLength = uvqs.GetLength(0);
        int yLength = uvqs.GetLength(1);

        float[,] noise = new float[xLength, yLength];

        for (int y = 0; y < yLength; y++)
        {
            for (int x = 0; x < xLength; x++)
            {
                Vector3 point = await Conversion.Coordinate.UVQtoXYZ(uvqs[x,y], quadMapSize);
                // Debug.Log(point.ToString());
                noise[x,y] = fnl.GetNoise(point.x, point.y, point.z);
            }
        }
        
        return noise;
    }
    
    // public static async Task<float[,]> GetNoise(Vector3[] pointsa, Vector3[] pointsb)
    // {
    //     float[,] noise = new float[longs.Length, lats.Length];

    //     for (int y = 0; y < lats.Length; y++)
    //     {
    //         for (int x = 0; x < longs.Length; x++)
    //         {
    //             Vector3 point = await Conversion.Coordinate.UVQtoXYZ(uvqs[x,y], quadMapSize);
    //             noise[x,y] = fnl.GetNoise(point.x, point.y, point.z);
    //         }
    //     }
        
    //     return noise;
    // }
}
