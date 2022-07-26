using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

public class LocationManager : MonoBehaviour
{
    [SerializeField]
    public Locale locale;
    public int chunkCount, chunkStartCount, cellCount, cellStartCount, tileCount;
    public int chunkMSize, cellMSize, tileMSize;
    public float degreeSize = 40000000/360.0f;
    public Gradient elevationGradient;
    bool abort = false;
    bool generated;
    public Renderer plane;
    public Texture2D groundTex;
    Color[] groundColors;
    public Color colorUsed;

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

    async void Start()
    {
        if (abort) return;
        if (!generated) await GenerateLand();
    }

    async Task<bool> GenerateLand()
    {
        Debug.LogFormat("Location {0} at {1}.", locale.placeName, locale.longLatCoord);
        Debug.LogFormat("Seed is {0}, size is {1}², res is {2}².", GameRam.NoiseSettings.mSeed, "[null]", chunkCount * cellCount * tileCount);
        groundTex = new Texture2D(tileCount, tileCount);
        groundColors = new Color[groundTex.width * groundTex.height];

        
        // Initialize Chunks
        locale.chunks = new List<Chunk>();
        List<Task> chunkTasks = new List<Task>();
        for (int i = 0; i < chunkStartCount; i++)
        {
            locale.chunks.Add(new Chunk());
            chunkTasks.Add(GenerateChunk(i));
        }
        while (chunkTasks.Count > 0)
        {
            Task finishedTask = await Task.WhenAny(chunkTasks);
            chunkTasks.Remove(finishedTask);
        }

        Debug.Log("Applying Tex");
        groundTex.SetPixels(groundColors);
        groundTex.wrapMode = TextureWrapMode.Clamp;
        groundTex.filterMode = FilterMode.Point;
        groundTex.Apply();
        plane.material.mainTexture = groundTex;

        return true;
    }

    async Task<Chunk> GenerateChunk(int chunkIndex)
    {
        Chunk newChunk = new Chunk();
        newChunk.longLatCoord = locale.longLatCoord + new Vector2(chunkMSize/degreeSize, chunkMSize/degreeSize);
        newChunk.pLocale = locale;
        locale.chunks[chunkIndex] = newChunk;

        // Initialize Cells
        locale.chunks[chunkIndex].cells = new List<Cell>();
        List<Task> cellTasks = new List<Task>();
        for (int i = 0; i < cellStartCount; i++)
        {
            locale.chunks[chunkIndex].cells.Add(new Cell());
            cellTasks.Add(GenerateCell(chunkIndex, i));
        }
        while (cellTasks.Count > 0)
        {
            Task finishedTask = await Task.WhenAny(cellTasks);
            cellTasks.Remove(finishedTask);
        }

        return newChunk;
    }

    async Task<Cell> GenerateCell(int chunkIndex, int cellIndex)
    {
        Cell newCell = new Cell();
        newCell.longLatCoord = locale.chunks.Last().longLatCoord + new Vector2(cellMSize/degreeSize, cellMSize/degreeSize);
        newCell.pChunk = locale.chunks[chunkIndex];
        locale.chunks[chunkIndex].cells[cellIndex] = newCell;

        // Initialize Tiles
        locale.chunks[chunkIndex].cells[cellIndex].tiles = new List<Tile>();
        List<Task> tileTasks = new List<Task>();
        for (int i = 0; i < tileCount * tileCount; i++)
        {
            locale.chunks[chunkIndex].cells[cellIndex].tiles.Add(new Tile());
            tileTasks.Add(GenerateTile(chunkIndex, cellIndex, i));
        }
        while (tileTasks.Count > 0)
        {
            Task finishedTask = await Task.WhenAny(tileTasks);
            tileTasks.Remove(finishedTask);
        }
        return newCell;
    }

    async Task<Tile> GenerateTile(int chunkIndex, int cellIndex, int tileIndex)
    {
        Tile newTile = new Tile();
        newTile.pCell = locale.chunks[chunkIndex].cells[cellIndex];
        float subU = 0;
        float subV = 0;
        newTile.subUvqCoord = new Vector3(subU, subV, newTile.pCell.pChunk.pLocale.uvqCoord.z);
        double ele = await Generate.GetNoise(newTile.subUvqCoord, new Vector2Int(newTile.pCell.pChunk.pLocale.uvqCoord.x, newTile.pCell.pChunk.pLocale.uvqCoord.y));
        newTile.elevation = ((float)ele);
        // Debug.LogFormat("{0}: {1}", newTile.longLatCoord.ToString(), newTile.elevation.ToString());
        locale.chunks[chunkIndex].cells[cellIndex].tiles[tileIndex] = newTile;

        int colorIndex = (chunkIndex * chunkCount + cellIndex * cellCount + tileIndex);
        float inv = Mathf.InverseLerp(GameRam.planet.minHeight, GameRam.planet.maxHeight, newTile.elevation);
        groundColors[colorIndex] = elevationGradient.Evaluate(inv);
        Debug.Log(groundColors[colorIndex].ToString());

        return newTile;
    }
}
