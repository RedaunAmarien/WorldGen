using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavAreaControl : MonoBehaviour
{
    [SerializeField] private int numOfFinders;
    [SerializeField] private int numOfWaypoints;
    [SerializeField] private Vector2Int minSpawnRange;
    [SerializeField] private Vector2Int maxSpawnRange;
    [SerializeField] GameObject finderPrefab;
    [SerializeField] GameObject waypointPrefab;
    [SerializeField] GameObject groundBlockPrefab;
    
    GameObject terrainRoot;
    GameObject[] waypoints;
    GameObject[] finders;
    GroundBlock[,] groundBlocks;

    Vector3[] positions;

    void Start()
    {
        terrainRoot = GameObject.Find("TerrainRoot");
        groundBlocks = new GroundBlock[maxSpawnRange.x - minSpawnRange.x + 2, maxSpawnRange.y - minSpawnRange.y + 2];

        for (int y = minSpawnRange.y - 1; y < maxSpawnRange.y + 1; y++)
        {
            for (int x = minSpawnRange.x - 1; x < maxSpawnRange.x + 1; x++)
            {
                groundBlocks[x, y] = GameObject.Instantiate(groundBlockPrefab, new Vector3(x, 0, y), Quaternion.identity, terrainRoot.transform).GetComponent<GroundBlock>();
            }
        }

        waypoints = new GameObject[numOfWaypoints];
        finders = new GameObject[numOfFinders];
        positions = new Vector3[finders.Length];

        for (int i = 0; i < numOfWaypoints; i++)
        {
            Vector3Int location = new Vector3Int(Random.Range(minSpawnRange.x, maxSpawnRange.x), 0, Random.Range(minSpawnRange.y, maxSpawnRange.y));
            waypoints[i] = GameObject.Instantiate(waypointPrefab, location, Quaternion.identity);
        }
        for (int i = 0; i < numOfFinders; i++)
        {
            Vector3Int location = new Vector3Int(Random.Range(minSpawnRange.x, maxSpawnRange.x), 0, Random.Range(minSpawnRange.y, maxSpawnRange.y));
            finders[i] = GameObject.Instantiate(finderPrefab, location, Quaternion.identity);
        }
    }

    public Vector2Int PositionToNode(Vector3 position)
    {
        Vector2Int node = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.z));
        return node;
    }
}
