using System.Threading.Tasks;
using UnityEngine;

public static class Generate
{
    public static FastNoiseLite fnl;

    public static async Task<float> GetNoise(Coordinates coordinates)
    {
        Vector3 point = coordinates.cartesianPosition;
        await new WaitForEndOfFrame();
        return fnl.GetNoise(point.x, point.y, point.z);
    }

    public static async Task<float> GetNoise(Coordinates coordinates, Vector2 newCenter)
    {
        Vector3 point = coordinates.cartesianPosition;

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
                Vector3 point = coordinates[x,y].cartesianPosition;
                noises[x,y] = fnl.GetNoise(point.x, point.y, point.z);
            }
        }

        await new WaitForEndOfFrame();
        return noises;
    }

    public static string Name(int length)
    {
        string newName = string.Empty;

        char[] con = new char[]
            {
                'b','c','d','f','g','h','j','k','l','m','n','p','q','r','s','t','v','w','x','y','z',// 'ɴ','\'','ŋ','β','ʃ','χ'
            };
        char[] vow = new char[]
            {
                'a','e','i','o','u','y',// 'á','é','í','ó',// 'ú','ü','æ','ɪ','œ','ʏ'
            };
        for (int i = 0; i < length; i++)
        {
            if (i == 0)
                newName += (char)UnityEngine.Random.Range('A', 'Z' + 1);
            else if (i % 2 == 0)
                newName += con[UnityEngine.Random.Range(0, con.Length)];
            else
                newName += vow[UnityEngine.Random.Range(0, vow.Length)];
        }

        return newName;
    }
}
