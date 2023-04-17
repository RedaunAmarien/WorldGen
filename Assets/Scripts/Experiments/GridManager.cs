using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor.AI;

public class GridManager : MonoBehaviour
{
    [Range(1,8)]
    public int numberOfPlayers = 3;
    public GameObject[] bases;

    Grid gameGrid;
    public enum MapSize{
        ExtraSmall = 128,
        Small = 160,
        Medium = 192,
        Large = 224,
        ExtraLarge = 256,
        Massive = 384
    }
    public MapSize mapSize;

    // [Range(0,1)]
    // public float beachThresh, waterThresh, deepWaterThresh;
    public float genScale = 20f, genHeight = 4, heightSteps = 8;
    public float genOffsetX, genOffsetZ;
    public int seed;
    public bool customSeed;

    // public GameObject emptyTile;
    public GameObject playerBase;
    public GameObject playerVillager;
    public GameObject waterSizer;

    public int startingPopulation = 5;

    // public GameObject[,] baseTiles;
    public TileProperties[,] tileProps;
    public GameObject map;
    Mesh mapMesh;
    Vector3[] mapVerts;
    int[] mapTris;

    // bool timedOut;

    void Start()
    {
        Cleanup();
        GenerateNew(true, true, true);
    }

    public void GenerateNew(bool mapGen, bool tileGen, bool playerGen)
    {
        if (customSeed)
        {
            Random.InitState(seed);
        }
        else
        {
            seed = (int)System.DateTime.Now.Ticks;
            Random.InitState((int)System.DateTime.Now.Ticks);
        }
        genOffsetX = Random.Range(0f, 99999f);
        genOffsetZ = Random.Range(0f, 99999f);
        gameGrid = gameObject.GetComponent<Grid>();

        if (mapGen) GenerateMap();
        if (tileGen) GenerateTileProps();
        if (playerGen) GeneratePlayers();
    }

    public void GenerateMap() 
    {
        mapMesh = new Mesh();
        map.GetComponent<MeshFilter>().mesh = mapMesh;

        mapVerts = new Vector3[((int)mapSize+1)*((int)mapSize+1)];
        for (int i = 0, z = 0; z <= (int)mapSize; z++)
        {
            for (int x = 0; x <= (int)mapSize; x++)
            {
                float y = CalculateHeight(x, z) * genHeight;
                mapVerts[i] = new Vector3(x, y, z);
                i++;   
            }
        }

        mapTris = new int[(int)mapSize*(int)mapSize*6];
        int vert = 0;
        int tri = 0;
        for (int z = 0; z < (int)mapSize; z++)
        {
            for (int x = 0; x < (int)mapSize; x++)
            {
                mapTris[tri + 0] = vert + 0;
                mapTris[tri + 1] = vert + (int)mapSize + 1;
                mapTris[tri + 2] = vert + 1;
                mapTris[tri + 3] = vert + 1;
                mapTris[tri + 4] = vert + (int)mapSize + 1;
                mapTris[tri + 5] = vert + (int)mapSize + 2;

                vert ++;
                tri += 6;
            }
            vert ++;
        }

        Vector2[] mapUVs = new Vector2[mapVerts.Length];

        
        for (int i = 0, z = 0; z <= (int)mapSize; z++)
        {
            for (int x = 0; x <= (int)mapSize; x++)
            {
                mapUVs[i] = new Vector2(x, z);
                i++;   
            }
        }

        mapMesh.Clear();

        mapMesh.vertices = mapVerts;
        mapMesh.triangles = mapTris;
        mapMesh.uv = mapUVs;

        mapMesh.RecalculateNormals();
        mapMesh.RecalculateBounds();

        MeshCollider meshCollider = map.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mapMesh;

        waterSizer.transform.localScale = new Vector3((float)mapSize/10f, 1, (float)mapSize/10f);
        waterSizer.GetComponentInChildren<Renderer>().sharedMaterial.mainTextureScale = new Vector2((float)mapSize, (float)mapSize);
        //NavMeshBuilder.BuildNavMesh();
    }

    public void GenerateTileProps()
    {
        tileProps = new TileProperties[(int)mapSize, (int)mapSize];

        for (int x = 0; x < (int)mapSize; x++)
        {
            for (int z = 0; z < (int)mapSize; z++)
            {
                tileProps[x, z] = ScriptableObject.CreateInstance<TileProperties>();
                tileProps[x, z].location = new Vector2Int(x, z);
                int assignHeight = Mathf.RoundToInt(CalculateHeight(x, z) * 8);
                tileProps[x, z].OnGeneration(new Vector2Int(x, z), assignHeight);
            }
        }
        Debug.LogFormat("{0} baseTiles initialized.", tileProps.Length.ToString());
    }
        
    public void GeneratePlayers()
    {
        bases = new GameObject[numberOfPlayers];
        for (int i = 0; i < numberOfPlayers; i++)
        {
            int startLocX = 0;
            int startLocZ = 0;
            bool placeValid = false;
            do
            {
                startLocX = Random.Range(6,(int)mapSize-5);
                startLocZ = Random.Range(6,(int)mapSize-5);
                placeValid = TestLocation(startLocX, startLocZ, 4);
            }
            while (!placeValid);

            bases[i] = Instantiate(playerBase, gameGrid.CellToLocal(new Vector3Int(startLocX, (tileProps[startLocX, startLocZ].height), startLocZ)), Quaternion.Euler(0, 0, 0));
            bases[i].GetComponent<UnitBuildingBase>().ChangeOwner(i);
            for (int k = 0; k < 4; k++)
            {
                for (int j = 0; j < 4; j++)
                {
                    tileProps[startLocX + k, startLocZ + j].occupied = true;
                }
            }
            // Debug.LogFormat("Player {0} located at {1}, {2} at height {3}.", i, startLocX, startLocZ, tileProps[startLocX, startLocZ].height);

            //Spawn Villager(s)
            for (int j = 0; j < startingPopulation; j++)
            {
                int x = 0;
                int z = 0;
                // int flip = -1;
                bool placeValid2 = false;
                do
                {
                    x = Random.Range(-2, 7);
                    z = Random.Range(-2, 7);
                    // if (x >= -2 && x <= 1 && z >= -2 && z <= 1) 
                    // {
                    //     z = 3;
                    //     x = 3;
                    // }
                    placeValid2 = TestLocation(startLocX + x, startLocZ + z, 1);
                }
                while (!placeValid2);
                
                GameObject vill = Instantiate(playerVillager, gameGrid.CellToLocal(new Vector3Int(startLocX + x, (tileProps[(startLocX + x), startLocZ + z].height), startLocZ + z)), Quaternion.Euler(0, 0, 0));
                //vill.GetComponent<Citizen>().OnBorn(i, "Villager");
                tileProps[(startLocX + x),  startLocZ + z].occupied = true;
                // Debug.LogFormat("Villager {0}-{4} located at {1}, {2} at height {3}.", i, startLocX+x, startLocZ+z, tileProps[startLocX + x, startLocZ + z].height, j);
            }
        }
    }

    bool TestLocation (int startX, int startZ, int size)
    {
        bool valid = true;
        int height = -1;
        int prevHeight = -1;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (startX+i > (int)mapSize || startZ+j > (int)mapSize || startX+i < 0 || startZ+j < 0)
                {
                    Debug.LogErrorFormat("Tile is outside bounds of array at {0}, {1} from {2}, {3}.", startX+i, startZ+j, startX, startZ);
                    valid = false;
                }
                else
                {
                    height = tileProps[(startX+i), startZ+j].height;
                    if (!tileProps[(startX+i), startZ+j].buildable || tileProps[(startX+i), startZ+j].occupied || (prevHeight != height && prevHeight != -1))
                    {
                        valid = false;
                    }
                    prevHeight = height;
                    height = -1;
                }
            }
        }

        return valid;
    }

    float CalculateHeight (int x, int z)
    {
        float xCoord = (float)x / 64 * genScale + genOffsetX;
        float zCoord = (float)z / 64 * genScale + genOffsetZ;
        
        float newHeight = Mathf.PerlinNoise(xCoord,zCoord);
        newHeight = Mathf.Round(newHeight / 0.125f) * 0.125f;

        return newHeight;
    }

    public void PlanBuild(int locX, int locZ/*, building type*/)
    {

    }
    
    public void FinishBuild(int locX, int locZ/*, building type, plan index*/)
    {

    }

    public void Cleanup()
    {
        if (tileProps != null)
        {
            foreach (TileProperties tile in tileProps)
            {
                DestroyImmediate(tile);
            }
            tileProps = new TileProperties[0,0];
        }

        if (bases != null)
        {
            foreach (GameObject pBase in bases)
            {
                DestroyImmediate(pBase);
            }
            bases = new GameObject[0];
        }

        GameObject[] units = GameObject.FindGameObjectsWithTag("UnitVillager");

        foreach (GameObject unit in units)
        {
            DestroyImmediate(unit);
        }

        GameObject[] objects = GameObject.FindGameObjectsWithTag("DELETE");

        foreach (GameObject item in objects)
        {
            DestroyImmediate(item);
        }
    }
}
