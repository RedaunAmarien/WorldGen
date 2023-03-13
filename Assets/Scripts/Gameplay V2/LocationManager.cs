using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

public class LocationManager : MonoBehaviour
{
    [SerializeField] private Locale locale;
    [Tooltip("Chunks per Locale along one edge.")]
    [SerializeField] private int chunkCount;
    [Tooltip("Cells per Chunk along one edge.")]
    [SerializeField] private int cellCount;
    [Tooltip("Tiles per Cell along one edge.")]
    [SerializeField] private int tileCount;
    [Tooltip("Number of Chunks to render.")]
    [SerializeField] private int chunkStartCount;
    [Tooltip("Number of Cells to render.")]
    [SerializeField] private int cellStartCount;
    [Tooltip("Length of one edge of a Chunk in meters.")]
    [SerializeField] private int chunkSizeMeters;
    [Tooltip("Length of one edge of a Cell in meters.")]
    [SerializeField] private int cellSizeMeters;
    [Tooltip("Length of one edge of a Tile in meters.")]
    [SerializeField] private int tileSizeMeters;
    private readonly float degreeSize = 40000000 / 360.0f;
    [SerializeField] private Gradient elevationGradient;
    private bool abort = false;
    private readonly bool generated;
    [SerializeField] private GameObject cellObjectTemplate;
    [SerializeField] private Transform landRoot;
    float progress;
    float progressMax;
    //[SerializeField] private float zoomLacunarity;

    void Awake()
    {
        if (GameRam.currentLocale != null)
            locale = GameRam.currentLocale;
        else
        {
            Debug.LogError("GameRam not found. Return to worldgen and try again.");
            abort = true;
        }
    }

    void Start()
    {
        if (abort)
            return;
        if (!generated)
            GenerateLand();
    }

    private void Update()
    {
        if (progress % 10 == 0 && !abort)
            Debug.Log(progress * 100);
    }

    public int GetTileY(Vector2Int chunkIndex, Vector2Int cellIndex, Vector2Int tileIndex)
    {
        Chunk chunk = locale.GetChunk(chunkIndex);
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
        int result = (int)System.Math.Round(heightToElevation - locale.avgElevation);
        Debug.LogFormat("Elevation {0}, Y {1},\nroot height {2}, interpolated height {3}, MeterHeight {4}.", tile.elevation, result, locale.avgElevation, percentOfHeight, heightToElevation);
        return result;
    }

    void GenerateLand()
    {
        progress = 0;
        progressMax = chunkStartCount * chunkStartCount * cellStartCount * cellStartCount * tileCount * tileCount;
        //Debug.LogFormat("Location {0} at {1}.", locale.placeName, locale.longLatCoord);
        //Debug.LogFormat("Seed is {0}, size is {1}², res is {2}².", GameRam.NoiseSettings.mSeed, "!!!", chunkCount * cellCount * tileCount);
        //GameRam.NoiseSettings.mLacunarity = zoomLacunarity;

        // Initialize Chunks
        locale.chunks = new List<Chunk>();
        //List<Task> tasks = new List<Task>();
        for (int y = 0; y < chunkStartCount; y++)
        {
            for (int x = 0; x < chunkStartCount; x++)
            {
                Vector2Int index = new(x, y);
                Chunk newChunk = new(index, locale);
                GenerateChunk(newChunk);
            }
        }
        Destroy(cellObjectTemplate);
        //Debug.LogFormat("Height MinMax: {0}–{1}", GameRam.planet.minHeight, GameRam.planet.maxHeight);
    }

    void GenerateChunk(Chunk chunk)
    {
        chunk.coordinates = new Coordinates(locale.coordinates.longitude + chunk.index.x * chunkSizeMeters / degreeSize, locale.coordinates.latitude + chunk.index.y * chunkSizeMeters / degreeSize);
        chunk.parentLocale = locale;

        // Initialize Cells
        //List<Task> cellTasks = new List<Task>();
        for (int y = 0; y < cellStartCount; y++)
        {
            for (int x = 0; x < cellStartCount; x++)
            {
                Vector2Int index = new(x, y);
                Cell newCell = new(index, chunk);
                GenerateCell(newCell);

            }
        }

        locale.chunks.Add(chunk);
        //Debug.LogFormat("Finished Generation of Chunk {0}.", chunk.index.ToString());
    }

    async void GenerateCell(Cell cell)
    {
        cell.coordinates = new Coordinates(cell.parentChunk.coordinates.longitude + cell.index.x * cellSizeMeters / degreeSize, cell.parentChunk.coordinates.latitude + cell.index.y * cellSizeMeters / degreeSize);
        cell.cellPlane = Instantiate(cellObjectTemplate, new Vector3(
            cell.parentChunk.index.x * cellStartCount * 200 + cell.index.x * 200, 0, cell.parentChunk.index.y * cellStartCount * 200 + cell.index.y * 200),
            Quaternion.identity, landRoot);
        //cell.cellPlane.GetComponent<Mesh>().vertices = new Vector3[0];

        Texture2D newTex = new(tileCount, tileCount);
        Color32[] newcolors = new Color32[tileCount * tileCount];

        // Initialize Tiles
        //List<Task> tileTasks = new List<Task>();
        for (int y = 0; y < tileCount; y++)
        {
            for (int x = 0; x < tileCount; x++)
            {
                Vector2Int index = new(x, y);
                Tile newTile = new(index, cell);
                int colorIndex = (y * tileCount + x);
                newcolors[colorIndex] = await GenerateTile(newTile);
            }
        }

        //Debug.LogFormat("Applying Tex on Cell {0}.", cell.index.ToString());
        newTex.SetPixels32(newcolors);
        newTex.wrapMode = TextureWrapMode.Clamp;
        newTex.filterMode = FilterMode.Point;
        newTex.Apply();
        cell.cellPlane.GetComponentInChildren<Renderer>().material.mainTexture = newTex;

        cell.parentChunk.AddCell(cell);
        //Debug.LogFormat("Finished Generation of Cell {0}.", cell.index.ToString());
    }

    async Task<Color> GenerateTile(Tile tile)
    {
        tile.coordinates = new Coordinates(tile.parentCell.coordinates.longitude + tile.index.x * tileSizeMeters / degreeSize, tile.parentCell.coordinates.latitude + tile.index.y * tileSizeMeters / degreeSize);

        double ele = await Generate.GetNoise(tile.coordinates, GameRam.worldMapCenter);
        tile.elevation = (float)ele;

        float inv = Mathf.InverseLerp(GameRam.planet.minHeight, GameRam.planet.maxHeight, tile.elevation);
        //if (tile.index == Vector2Int.zero)
        //Debug.Log(percentOfHeight);
        Color col = elevationGradient.Evaluate(inv);
        if ((tile.index.x % 2 == 0 && tile.index.y % 2 == 0) || (tile.index.x % 2 == 1 && tile.index.y % 2 == 1))
        {
            col.r -= 0.025f;
            col.g -= 0.025f;
            col.b -= 0.025f;
        }

        tile.parentCell.AddTile(tile);
        progress += 1 / progressMax;
        return col;
        //Debug.LogFormat("Finished Generation of Tile {0}.", tile.index.ToString());
    }
}
