using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

public class LocationManager : MonoBehaviour
{
    [SerializeField] private Locale locale;
    [SerializeField] private int chunkCount, chunkStartCount, cellCount, cellStartCount, tileCount;
    [SerializeField] private int chunkMSize, cellMSize, tileMSize;
    private float degreeSize = 40000000 / 360.0f;
    [SerializeField] private Gradient elevationGradient;
    private bool abort = false;
    private bool generated;
    [SerializeField] private GameObject cellObjectTemplate;
    [SerializeField] private Transform landRoot;
    float progress, progmax;
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
        if (progress % 10 == 0)
            Debug.Log(progress * 100);
        
    }

    public int GetTileY(Vector2Int chunkIndex, Vector2Int cellIndex, Vector2Int tileIndex)
    {
        Chunk chunky = locale.GetChunk(chunkIndex);
        if (chunky == null)
        {
            Debug.LogErrorFormat("Chunk {0} returned as null.", chunkIndex);
            return 0;
        }
        Cell celly = chunky.GetCell(cellIndex);
        if (celly == null)
        {
            Debug.LogErrorFormat("Cell {0} returned as null.", cellIndex);
            return 0;
        }
        Tile tiley = celly.GetTile(tileIndex);
        if (tiley == null)
        {
            Debug.LogErrorFormat("Tile {0} returned as null.", tileIndex);
            return 0;
        }
        float eve = tiley.elevation;
        float inv = Mathf.InverseLerp(GameRam.planet.minHeight, GameRam.planet.maxHeight, eve);
        float mtEve = Mathf.Lerp(GameRam.planet.minElevation, GameRam.planet.maxElevation, inv);
        int y = (int)System.Math.Round(mtEve - locale.avgElevation);
        Debug.LogFormat("Elevation {0}, Y {1},\nroot height {2}, interpolated height {3}, MeterHeight {4}.", eve, y, locale.avgElevation, inv, mtEve);
        return y;
    }

    void GenerateLand()
    {
        progress = 0;
        progmax = chunkStartCount * chunkStartCount * cellCount * cellCount * tileCount * tileCount;
        //Debug.LogFormat("Location {0} at {1}.", locale.placeName, locale.longLatCoord);
        //Debug.LogFormat("Seed is {0}, size is {1}², res is {2}².", GameRam.NoiseSettings.mSeed, "!!!", chunkCount * cellCount * tileCount);
        //GameRam.NoiseSettings.mLacunarity = zoomLacunarity;

        // Initialize Chunks
        locale.chunks = new List<Chunk>();
        List<Task> chunkTasks = new List<Task>();
        for (int y = 0; y < chunkStartCount; y++)
        {
            for (int x = 0; x < chunkStartCount; x++)
            {
                Vector2Int ind = new Vector2Int(x, y);
                Chunk newChunk = new Chunk(ind, locale);
                GenerateChunk(newChunk);
            }
        }
        Destroy(cellObjectTemplate);
        //Debug.LogFormat("Height MinMax: {0}–{1}", GameRam.planet.minHeight, GameRam.planet.maxHeight);
    }

    void GenerateChunk(Chunk chunk)
    {
        chunk.longLatCoord.x = locale.longLatCoord.x + chunk.index.x * chunkMSize / degreeSize;
        chunk.longLatCoord.y = locale.longLatCoord.y + chunk.index.y * chunkMSize / degreeSize;
        chunk.pLocale = locale;

        // Initialize Cells
        List<Task> cellTasks = new List<Task>();
        for (int y = 0; y < cellStartCount; y++)
        {
            for (int x = 0; x < cellStartCount; x++)
            {
                Vector2Int ind = new Vector2Int(x, y);
                Cell newCell = new Cell(ind, chunk);
                GenerateCell(newCell);

            }
        }

        locale.chunks.Add(chunk);
        //Debug.LogFormat("Finished Generation of Chunk {0}.", chunk.index.ToString());
    }

    async void GenerateCell(Cell cell)
    {
        cell.longLatCoord.x = cell.pChunk.longLatCoord.x + cell.index.x * cellMSize / degreeSize;
        cell.longLatCoord.y = cell.pChunk.longLatCoord.y + cell.index.y * cellMSize / degreeSize;
        cell.cellPlane = Instantiate(cellObjectTemplate, new Vector3(
            cell.pChunk.index.x * cellStartCount * 200 + cell.index.x * 200, 0, cell.pChunk.index.y * cellStartCount * 200 + cell.index.y * 200),
            Quaternion.identity, landRoot);
        //cell.cellPlane.GetComponent<Mesh>().vertices = new Vector3[0];

        Texture2D newTex = new Texture2D(tileCount, tileCount);
        Color[] newcolors = new Color[tileCount * tileCount];

        // Initialize Tiles
        List<Task> tileTasks = new List<Task>();
        for (int y = 0; y < tileCount; y++)
        {
            for (int x = 0; x < tileCount; x++)
            {
                Vector2Int ind = new Vector2Int(x, y);
                Tile newTile = new Tile(ind, cell);
                int colorIndex = (y * tileCount + x);
                newcolors[colorIndex] = await GenerateTile(newTile);
            }
        }

        //Debug.LogFormat("Applying Tex on Cell {0}.", cell.index.ToString());
        newTex.SetPixels(newcolors);
        newTex.wrapMode = TextureWrapMode.Clamp;
        newTex.filterMode = FilterMode.Point;
        newTex.Apply();
        cell.cellPlane.GetComponentInChildren<Renderer>().material.mainTexture = newTex;

        cell.pChunk.AddCell(cell);
        //Debug.LogFormat("Finished Generation of Cell {0}.", cell.index.ToString());
    }

    async Task<Color> GenerateTile(Tile tile)
    {
        tile.longLatCoord.x = tile.pCell.longLatCoord.x + tile.index.x * tileMSize / degreeSize;
        tile.longLatCoord.y = tile.pCell.longLatCoord.y + tile.index.y * tileMSize / degreeSize;

        double ele = await Generate.GetNoise(tile.longLatCoord, GameRam.worldMapCenter);
        tile.elevation = (float)ele;

        float inv = Mathf.InverseLerp(GameRam.planet.minHeight, GameRam.planet.maxHeight, tile.elevation);
        //if (tile.index == Vector2Int.zero)
        //Debug.Log(inv);
        Color col = elevationGradient.Evaluate(inv);
        if ((tile.index.x % 2 == 0 && tile.index.y % 2 == 0) || (tile.index.x % 2 == 1 && tile.index.y % 2 == 1))
        {
            col.r -= 0.025f;
            col.g -= 0.025f;
            col.b -= 0.025f;
        }

        tile.pCell.AddTile(tile);
        progress += 1 / progmax;
        return col;
        //Debug.LogFormat("Finished Generation of Tile {0}.", tile.index.ToString());
    }
}
