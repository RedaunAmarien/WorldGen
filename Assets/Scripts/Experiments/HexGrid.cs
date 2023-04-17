using Autodesk.Fbx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public int width = 6;
    public int height = 6;
    public float tileGap;
    public HexTile tilePrefab;
    HexTile[] tiles;
    HexMesh hexMesh;

    // Start is called before the first frame update
    void Awake()
    {
        //gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();
        tiles = new HexTile[width * height];
        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateTile(x, z, i++);
            }
        }
    }

    private void Start()
    {
        hexMesh.Triangulate(tiles);
    }

    void CreateTile(int x, int z, int i)
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * HexTile.innerRadius * 2f;
        position.y = 0;
        position.z = z * HexTile.outerRadius * 1.5f;

        HexTile tile = tiles[i] = Instantiate(tilePrefab);
        tile.transform.SetParent(transform, false);
        tile.transform.localPosition = position;
        //tile.gridCoordinates = new Vector3Int(x, 0, z);
        tile.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
    }
}
