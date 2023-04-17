using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private Coordinates gameplayCoordinates;
    [SerializeField] private Locale currentLocale;
    [SerializeField] private SubLocale currentSubLocale;
    [SerializeField] private Chunk currentChunk;
    [SerializeField] private Transform currentTransform;

    [Min(1)]
    [SerializeField] private int cellViewDistance;
    [Tooltip("Chunks per SubLocale along one edge.")]
    [SerializeField] private int chunksPerSubLocale;
    [Tooltip("Cells per Chunk along one edge.")]
    [SerializeField] private int cellsPerChunk;
    [Tooltip("Tiles per Cell along one edge.")]
    [SerializeField] private int tilesPerCell;
    public int tileEdgeLength;
    public int cellEdgeLength;
    public int chunkEdgeLength;

    [Tooltip("Number of Chunks to render.")]
    [SerializeField] private int chunkStartCount;
    [Tooltip("Number of Cells to render.")]
    [SerializeField] private int cellStartCount;


    private readonly float degreeSize = 40000000 / 360.0f;
    [SerializeField] private Gradient elevationGradient;
    private bool abort = false;
    private readonly bool generated;
    [SerializeField] private GameObject cellObjectTemplate;
    [SerializeField] private Transform landRoot;
    private List<GameObject> visibleCells = new();
    private List<GameObject> sleepingCells = new();
    //[SerializeField] private float zoomLacunarity;
    bool isNewGame;

    void Awake()
    {
        if (GameRam.currentLocale != null)
        {
            currentLocale = GameRam.currentLocale;
            //currentSubLocale = currentLocale.GetSubLocale(Vector2Int.zero);
            //currentChunk = currentSubLocale.GetChunk(Vector2Int.zero);
        }
        else
        {
            //GameRam.FromSaveData(FileIO.LoadFile());
            //GameRam.planet = new Planet();
            //Generate.fnl = new FastNoiseLite(GameRam.NoiseSettings.mSeed);
            abort = true;
        }
    }

    void Start()
    {
        if (abort)
            return;
        cellEdgeLength = tileEdgeLength * tilesPerCell;
        chunkEdgeLength = cellEdgeLength * cellsPerChunk;
        if (!generated)
            GenerateLand();
        if (isNewGame)
        {

        }
    }

    private void Update()
    {
        //Vector2Int currentChunkIndex = currentChunk.index;

        //for (int y = Mathf.RoundToInt(currentTransform.position.y) - cellViewDistance; y < cellViewDistance; y++)
        //{
        //    if (y < 0)
        //    {
        //        currentChunkIndex.y--;
        //        y = cellsPerChunk - 1;
        //    }
        //    for (int x = Mathf.RoundToInt(currentTransform.position.x) - cellViewDistance; x < cellViewDistance; x++)
        //    {
        //        if (x < 0)
        //        {
        //            currentChunkIndex.x--;
        //            x = cellsPerChunk - 1;
        //        }
        //        bool isExisting = false;
        //        foreach (GameObject cellObject in visibleCells)
        //        {
        //            Cell cell = cellObject.GetComponent<Cell>();
        //            if (cell.index == new Vector2Int(x, y))
        //            {
        //                isExisting = true;
        //            }
        //        }
        //        if (!isExisting)
        //        {
        //            GenerateCell(currentChunk, new Vector2Int(x, y));
        //        }
        //    }
        //}
    }

    public int GetTileY(Vector2Int chunkIndex, Vector2Int cellIndex, Vector2Int tileIndex)
    {
        Chunk chunk = currentSubLocale.GetChunk(chunkIndex);
        if (chunk == null)
        {
            Debug.LogErrorFormat("Chunk {0} returned as null.", chunkIndex);
            return 0;
        }
        Cell cell = chunk.GetCell(cellIndex);
        if (cell == null)
        {
            Debug.LogErrorFormat("Cell {0} returned as null.", cellIndex);
            return 0;
        }
        Tile tile = cell.GetTile(tileIndex);
        if (tile == null)
        {
            Debug.LogErrorFormat("Tile {0} returned as null.", tileIndex);
            return 0;
        }
        float percentOfHeight = Mathf.InverseLerp(GameRam.planet.minHeight, GameRam.planet.maxHeight, tile.elevation);
        float heightToElevation = Mathf.Lerp(GameRam.planet.lowestElevation, GameRam.planet.highestElevation, percentOfHeight);
        int result = (int)System.Math.Round(heightToElevation - currentSubLocale.avgElevation);
        Debug.LogFormat("Elevation {0}, Y {1},\nroot height {2}, interpolated height {3}, MeterHeight {4}.", tile.elevation, result, currentSubLocale.avgElevation, percentOfHeight, heightToElevation);
        return result;
    }

    void GenerateLand()
    {
        //progress = 0;
        //progressMax = chunkStartCount * chunkStartCount * cellStartCount * cellStartCount * tilesPerCell * tilesPerCell;
        //Debug.LogFormat("Location {0} at {1}.", currentSubLocale.placeName, currentSubLocale.longLatCoord);
        //Debug.LogFormat("Seed is {0}, size is {1}², res is {2}².", GameRam.NoiseSettings.mSeed, "!!!", chunksPerSubLocale * cellsPerChunk * tilesPerCell);
        //GameRam.NoiseSettings.mLacunarity = zoomLacunarity;

        // Initialize Chunks
        currentLocale.subLocales = new List<SubLocale>
        {
            new SubLocale()
        };
        currentSubLocale = currentLocale.subLocales[0];
        currentSubLocale.coordinates = currentLocale.coordinates;

        currentSubLocale.chunks = new List<Chunk>();
        //List<Task> tasks = new List<Task>();
        for (int y = 0; y < chunkStartCount; y++)
        {
            for (int x = 0; x < chunkStartCount; x++)
            {
                Vector2Int index = new(x, y);
                Chunk newChunk = new(index, currentSubLocale);
                GenerateChunk(newChunk);
            }
        }
        //Destroy(cellObjectTemplate);
        //Debug.LogFormat("Height MinMax: {0}–{1}", GameRam.planet.minHeight, GameRam.planet.maxHeight);
    }

    void GenerateChunk(Chunk chunk)
    {
        chunk.coordinates = new Coordinates(currentSubLocale.coordinates.longitude + chunk.index.x * chunkEdgeLength / degreeSize, currentSubLocale.coordinates.latitude + chunk.index.y * chunkEdgeLength / degreeSize);
        chunk.parentSubLocale = currentSubLocale;

        // Initialize Cells
        //List<Task> cellTasks = new List<Task>();
        for (int y = 0; y < cellStartCount; y++)
        {
            for (int x = 0; x < cellStartCount; x++)
            {
                Vector2Int index = new(x, y);
                GenerateCell(chunk, index);

            }
        }

        currentSubLocale.chunks.Add(chunk);
        //Debug.LogFormat("Finished Generation of Chunk {0}.", chunk.tileIndex.ToString());
    }

    async void GenerateCell(Chunk parent, Vector2Int index)
    {
        GameObject newCell = GameObject.Instantiate(
            cellObjectTemplate, new Vector3(
                parent.index.x * cellStartCount * 100 + index.x * cellEdgeLength, 0, parent.index.y * cellStartCount * 100 + index.y * cellEdgeLength), Quaternion.identity, landRoot);
        Cell cell = newCell.GetComponent<Cell>();
        cell.coordinates = new Coordinates(parent.coordinates.longitude + index.x * cellEdgeLength / degreeSize, parent.coordinates.latitude + index.y * cellEdgeLength / degreeSize);
        cell.parentChunk = parent;
        cell.index = index;

        //cellObject.cellPlane.GetComponent<Mesh>().vertices = new Vector3[0];

        Texture2D newTex = new(tilesPerCell, tilesPerCell);
        Color32[] newcolors = new Color32[tilesPerCell * tilesPerCell];

        // Initialize Tiles
        //List<Task> tileTasks = new List<Task>();
        for (int y = 0; y < tilesPerCell; y++)
        {
            for (int x = 0; x < tilesPerCell; x++)
            {
                Vector2Int tileIndex = new(x, y);
                Tile newTile = new(tileIndex, cell);
                int colorIndex = (y * tilesPerCell + x);
                newcolors[colorIndex] = await GenerateTile(newTile);
            }
        }

        //Debug.LogFormat("Applying Tex on Cell {0}.", cellObject.tileIndex.ToString());
        newTex.SetPixels32(newcolors);
        newTex.wrapMode = TextureWrapMode.Clamp;
        newTex.filterMode = FilterMode.Point;
        newTex.Apply();
        cell.cellPlane.GetComponentInChildren<Renderer>().material.mainTexture = newTex;

        cell.parentChunk.AddCell(cell);
        //Debug.LogFormat("Finished Generation of Cell {0}.", cellObject.tileIndex.ToString());
    }

    async Task<Color> GenerateTile(Tile tile)
    {
        tile.coordinates = new Coordinates(tile.parentCell.coordinates.longitude + tile.index.x * tileEdgeLength / degreeSize, tile.parentCell.coordinates.latitude + tile.index.y * tileEdgeLength / degreeSize);

        double ele = await Generate.GetNoise(tile.coordinates, GameRam.worldMapCenter);
        tile.elevation = (float)ele;

        float inv = Mathf.InverseLerp(GameRam.planet.minHeight, GameRam.planet.maxHeight, tile.elevation);
        //if (tile.tileIndex == Vector2Int.zero)
        //Debug.Log(percentOfHeight);
        Color col = elevationGradient.Evaluate(inv);
        if ((tile.index.x % 2 == 0 && tile.index.y % 2 == 0) || (tile.index.x % 2 == 1 && tile.index.y % 2 == 1))
        {
            col.r -= 0.025f;
            col.g -= 0.025f;
            col.b -= 0.025f;
        }

        tile.parentCell.AddTile(tile);
        //progress += 1 / progressMax;
        return col;
        //Debug.LogFormat("Finished Generation of Tile {0}.", tile.tileIndex.ToString());
    }
}
