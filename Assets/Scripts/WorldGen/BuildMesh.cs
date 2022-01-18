using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class BuildMesh : MonoBehaviour
{
    public int mapSize = 8;
    public int quadSize = 6;
    public Vector3[] verts;
    public Vector2[] uvs;
    public int[] tris;
    public Mesh mesh;
    public MeshFilter filter;
    public MeshCollider coll;

    void Start()
    {
        mesh = new Mesh();
        filter = GetComponent<MeshFilter>();
        coll = GetComponent<MeshCollider>();
        Generate();
        UpdateMesh();
    }

    void Generate()
    {
        verts = new Vector3[(mapSize + 1) * (mapSize + 1) * (quadSize)];
        int i = 0;
        for (int q = 0; q < 4; q++)
        {
            for (int y = -mapSize/2; y <= mapSize/2; y++)
            {
                for (int x = -mapSize/2; x <= mapSize/2; x++)
                {
                    switch (q)
                    {
                        case 0:
                            verts[i] = new Vector3(mapSize/2,y,x).normalized;
                        break;
                        case 1:
                            verts[i] = new Vector3(x,y,-mapSize/2).normalized;
                        break;
                        case 2:
                            verts[i] = new Vector3(-mapSize/2,y,-x).normalized;
                        break;
                        case 3:
                            verts[i] = new Vector3(-x,y,mapSize/2).normalized;
                        break;
                    }
                    i ++;
                }
            }
        }
        for (int q = 0; q < 2; q++)
        {
            for (int y = -mapSize/2; y <= mapSize/2; y++)
            {
                for (int x = -mapSize/2; x <= mapSize/2; x++)
                {
                    switch (q)
                    {
                        case 0:
                            verts[i] = new Vector3(y,-mapSize/2,x).normalized;
                        break;
                        case 1:
                            verts[i] = new Vector3(-y,mapSize/2,x).normalized;
                        break;
                    }
                    i ++;
                }
            }
        }

        tris = new int[mapSize * mapSize * 6 * 6];
        int v = 0; int t = 0;
        for (int q = 0; q < 6; q++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    tris[t + 0] = v + 0;
                    tris[t + 1] = v + mapSize + 1;
                    tris[t + 2] = v + 1;
                    tris[t + 3] = v + 1;
                    tris[t + 4] = v + mapSize + 1;
                    tris[t + 5] = v + mapSize + 2;
                    v ++;
                    t += 6;
                    if (t >= tris.Length) break;
                }
                v ++;
                if (t >= tris.Length) break;
            }
            v += 9;
            if (t >= tris.Length) break;
        }

        uvs = new Vector2[verts.Length];
        for (int j = 0, q = 0; q < 6; q++)
        {
            for (int y = 0; y <= mapSize; y++)
            {
                for (int x = 0; x <= mapSize; x++)
                {
                    uvs[j] = new Vector2((float)x/(mapSize), (float)y/(mapSize));
                    j ++;
                }
            }
        }
    }

    void UpdateMesh()
    {
        Debug.LogFormat("{0} verts, {1} triIndices.", verts.Length, tris.Length);
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.subMeshCount = 6;
        mesh.uv = uvs;
        for (int i = 0; i < 6; i++)
        {
            mesh.SetSubMesh(i, new UnityEngine.Rendering.SubMeshDescriptor(tris.Length/6 * i, tris.Length/6));
        }
        mesh.RecalculateNormals();
        filter.mesh = mesh;
        coll.sharedMesh = mesh;
    }
}
