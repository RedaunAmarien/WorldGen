using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public static class Generate
{
    public static FastNoiseLite fnl;

    public static async Task<float> GetNoise(Coordinates coordinates)
    {
        Vector3 point = coordinates.localPosition;
        await new WaitForEndOfFrame();
        return fnl.GetNoise(point.x, point.y, point.z);
    }

    public static async Task<float> GetNoise(Coordinates coordinates, Vector2 newCenter)
    {
        Vector3 point = coordinates.localPosition;

        point = Quaternion.AngleAxis(-newCenter.y, Vector3.right) * point;
        point = Quaternion.AngleAxis(180 - newCenter.x, Vector3.up) * point;

        await new WaitForEndOfFrame();
        return fnl.GetNoise(point.x, point.y, point.z);
    }
    
    public static async Task<float[,]> GetNoise(Coordinates[,] coordinates)
    {
        int xLength = coordinates.GetLength(0);
        int yLength = coordinates.GetLength(1);

        float[,] noises = new float[xLength, yLength];

        for (int y = 0; y < yLength; y++)
        {
            for (int x = 0; x < xLength; x++)
            {
                Vector3 point = coordinates[x,y].localPosition;
                noises[x,y] = fnl.GetNoise(point.x, point.y, point.z);
            }
        }

        await new WaitForEndOfFrame();
        return noises;
    }
}
