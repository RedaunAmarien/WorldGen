using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private Coordinates gameplayCoordinates;
    [Range(1, 8)]
    [SerializeField] private int viewDistance;
    [SerializeField] private Locale currentLocale;
    //[SerializeField] private SubLocale dummySubLocale;
    [SerializeField] private Chunk activeChunk;
    [SerializeField] private Transform currentTransform;
    [SerializeField] private List<Chunk> loadedChunks;

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

    private void Awake()
    {
        if (GameRam.currentLocale == null)
        {
            currentLocale = new Locale();
            gameplayCoordinates = new Coordinates(0, 0);
            GameRam.planet = new Planet();
            return;
        }

        currentLocale = GameRam.currentLocale;
        gameplayCoordinates = currentLocale.coordinates;
    }

    void Start()
    {
        if (abort)
            return;

        cellEdgeLength = tileEdgeLength * tilesPerCell;
        chunkEdgeLength = cellEdgeLength * cellsPerChunk;
        if (!generated)
            GenerateLand();
    }

    public int GetTileY(Vector2Int chunkIndex, Vector2Int cellIndex, Vector2Int tileIndex)
    {
        Chunk chunkToCheck = null;
        for (int i = 0; i < loadedChunks.Count; i++)
        {
            //Debug.LogFormat("Testing chunk {0} for {1}...", loadedChunks[i].index, chunkIndex);
            if (loadedChunks[i].index == chunkIndex)
            {
                //Debug.LogFormat("Chunk {0} found.", chunkIndex);
                chunkToCheck = loadedChunks[i];
                break;
            }
        }

        if (chunkToCheck == null)
        {
            Debug.LogErrorFormat("Chunk {0} returned as null.", chunkIndex);
            return 0;
        }
        Cell cell = chunkToCheck.GetCell(cellIndex);
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
        int result = (int)System.Math.Round(heightToElevation - currentLocale.avgElevation);
        Debug.LogFormat("Elevation {0}, Y {1},\nroot height {2}, interpolated height {3}, MeterHeight {4}.", tile.elevation, result, currentLocale.avgElevation, percentOfHeight, heightToElevation);
        return result;
    }

    void GenerateLand()
    {
        if (Generate.fnl == null)
            Generate.fnl = new FastNoiseLite();

        //dummySubLocale = new SubLocale();
        loadedChunks.Clear();
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                Chunk newChunk = new(new Vector2Int(x, y));
                newChunk.coordinates = new Coordinates(
                    gameplayCoordinates.longitude + newChunk.index.x * chunkEdgeLength / degreeSize,
                    gameplayCoordinates.latitude + newChunk.index.y * chunkEdgeLength / degreeSize);
                loadedChunks.Add(newChunk);
            }
        }

        foreach (Chunk chunk in loadedChunks)
        {
            for (int x = 0; x < cellsPerChunk; x++)
            {
                for (int y = 0; y < cellsPerChunk; y++)
                {
                    GenerateCell(chunk, new Vector2Int(x, y));
                }
            }
        }
    }

    async void GenerateCell(Chunk parent, Vector2Int index)
    {
        GameObject newCell = Instantiate(
            cellObjectTemplate, new Vector3(
                parent.index.x * cellsPerChunk * (tilesPerCell * tileEdgeLength) + index.x * cellEdgeLength,
                0,
                parent.index.y * cellsPerChunk * (tilesPerCell * tileEdgeLength) + index.y * cellEdgeLength),
            Quaternion.identity, landRoot);

        Cell cell = newCell.GetComponent<Cell>();
        cell.coordinates = new Coordinates(
            parent.coordinates.longitude + index.x * cellEdgeLength / degreeSize,
            parent.coordinates.latitude + index.y * cellEdgeLength / degreeSize);
        cell.parentChunk = parent;
        cell.index = index;

        float ele = await Generate.GetNoise(cell.coordinates, GameRam.worldMapCenter);
        float inv = Mathf.InverseLerp(GameRam.planet.minHeight, GameRam.planet.maxHeight, ele);
        float elevation = Mathf.Lerp(GameRam.planet.lowestElevation, GameRam.planet.highestElevation, inv);

        newCell.transform.position = new Vector3(
            newCell.transform.position.x,
            elevation,
            newCell.transform.position.z);

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

        cell.parentChunk.SetCell(cell);
        //Debug.LogFormat("Finished Generation of Cell {0}.", cellObject.tileIndex.ToString());
    }

    async Task<Color> GenerateTile(Tile tile)
    {
        tile.coordinates = new Coordinates(
            tile.parentCell.coordinates.longitude + tile.index.x * tileEdgeLength / degreeSize,
            tile.parentCell.coordinates.latitude + tile.index.y * tileEdgeLength / degreeSize);

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
