using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Unity.VisualScripting;
using System.Net;
using System.Linq;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private Coordinates gameplayCoordinates;
    [Range(1, 8)]
    [SerializeField] private int viewDistance;
    [SerializeField] private Locale currentLocale;
    [SerializeField] private Vector2Int activeChunk;
    [SerializeField] private Transform currentTransform;
    [SerializeField] private Dictionary<Vector2Int, ChunkContainer> chunkContainers;

    [Tooltip("Cells per Chunk along one edge.")]
    [SerializeField] private int cellsPerChunk;
    [Tooltip("Tiles per Cell along one edge.")]
    [SerializeField] private int tilesPerCell;
    public int tileEdgeLength;
    public int cellEdgeLength;
    public int chunkEdgeLength;

    private const float DEGREESIZE = 40000000 / 360.0f;
    [SerializeField] private Gradient elevationGradient;
    private bool generated;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private Transform landRoot;
    public bool fullyInitialized;
    Dictionary<Coordinates, GameObject> rivers = new();
    Dictionary<Coordinates, GameObject> lakes = new();

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
        cellEdgeLength = tileEdgeLength * tilesPerCell;
        chunkEdgeLength = cellEdgeLength * cellsPerChunk;
        if (!generated)
            GenerateChunks();
        fullyInitialized = true;
    }

    private void Update()
    {
        activeChunk = Position2Indices(transform.position)[0];
    }

    void GenerateChunks()
    {
        generated = true;
        Generate.fnl ??= new FastNoiseLite();

        chunkContainers = new();
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                Chunk newChunk = new(new Vector2Int(x, y));
                newChunk.coordinates = new Coordinates(
                    gameplayCoordinates.longitude + newChunk.index.x * chunkEdgeLength / DEGREESIZE,
                    gameplayCoordinates.latitude + newChunk.index.y * chunkEdgeLength / DEGREESIZE);
                newChunk.cellsPerChunkEdge = cellsPerChunk;
                GameObject chunkObject = Instantiate(chunkPrefab, new Vector3(x * chunkEdgeLength, 0, y * chunkEdgeLength), Quaternion.identity, landRoot);
                ChunkContainer chunkContainer = chunkObject.GetComponent<ChunkContainer>();
                chunkContainer.chunk = newChunk;
                chunkContainers.Add(new Vector2Int(x, y), chunkContainer);
            }
        }

        foreach (var container in chunkContainers)
        {
            for (int x = 0; x < cellsPerChunk; x++)
            {
                for (int y = 0; y < cellsPerChunk; y++)
                {
                    GenerateCell(container.Value.chunk, new Vector2Int(x, y));
                }
            }
        }
    }

    async void GenerateCell(Chunk parent, Vector2Int index)
    {
        GameObject newCell = Instantiate(
            cellPrefab, new Vector3(
                parent.index.x * cellsPerChunk * (tilesPerCell * tileEdgeLength) + index.x * cellEdgeLength,
                0,
                parent.index.y * cellsPerChunk * (tilesPerCell * tileEdgeLength) + index.y * cellEdgeLength),
            Quaternion.identity,
            chunkContainers[parent.index].gameObject.transform);

        Cell cell = newCell.GetComponent<Cell>();
        cell.coordinates = new Coordinates(
            parent.coordinates.longitude + index.x * cellEdgeLength / DEGREESIZE,
            parent.coordinates.latitude + index.y * cellEdgeLength / DEGREESIZE);
        cell.parentChunk = parent;
        cell.index = index;

        //Set Height of Cell Object from Elevation
        float ele = await Generate.GetNoise(cell.coordinates, GameRam.worldMapCenter);
        float inv = Mathf.InverseLerp(GameRam.planet.minHeight, GameRam.planet.maxHeight, ele);
        float elevation = Mathf.Lerp(GameRam.planet.lowestElevation, GameRam.planet.highestElevation, inv);

        if (index == Vector2Int.zero)
            chunkContainers[parent.index].transform.position = new Vector3(chunkContainers[parent.index].transform.position.x, elevation, chunkContainers[parent.index].transform.position.z);

        newCell.transform.position = new Vector3(
            newCell.transform.position.x,
            elevation,
            newCell.transform.position.z);

        Texture2D newTex = new(tilesPerCell, tilesPerCell);
        Color32[] newcolors = new Color32[tilesPerCell * tilesPerCell];

        // Initialize Tiles
        //List<Awaitable<Color32>> tileTasks = new List<Awaitable<Color32>>();
        for (int y = 0; y < tilesPerCell; y++)
        {
            for (int x = 0; x < tilesPerCell; x++)
            {
                Vector2Int tileIndex = new(x, y);
                Tile newTile = new(tileIndex, cell);
                //tileTasks.Add(GenerateTile(newTile));
                int colorIndex = (y * tilesPerCell + x);
                newcolors[colorIndex] = await GenerateTile(newTile);
            }
        }

        //// Generate each tile color
        //while (tileTasks.Count > 0)
        //{
        //    Awaitable finishedTask = await tileTasks[0].;
        //    newcolors[tileTasks.IndexOf(finishedTask)] = finishedTask.Task<Color32>.Result;
        //    tileTasks.Remove(finishedTask);
        //}

        //Debug.LogFormat("Applying Tex on Cell {0}.", cellObject.tileIndex.ToString());
        newTex.SetPixels32(newcolors);
        newTex.wrapMode = TextureWrapMode.Clamp;
        newTex.filterMode = FilterMode.Point;
        newTex.Apply();
        cell.cellPlane.GetComponentInChildren<Renderer>().material.mainTexture = newTex;

        cell.parentChunk.SetCell(cell);
        //Debug.LogFormat("Finished Generation of Cell {0}.", cellObject.tileIndex.ToString());
    }

    async Awaitable<Color32> GenerateTile(Tile tile)
    {
        tile.coordinates = new Coordinates(
            tile.parentCell.coordinates.longitude + tile.index.x * tileEdgeLength / DEGREESIZE,
            tile.parentCell.coordinates.latitude + tile.index.y * tileEdgeLength / DEGREESIZE);

        //Set Height of Tile Object from Elevation
        tile.elevation = await Generate.GetNoise(tile.coordinates, GameRam.worldMapCenter);
        float inv = Mathf.InverseLerp(GameRam.planet.minHeight, GameRam.planet.maxHeight, tile.elevation);
        float elevation = Mathf.Lerp(GameRam.planet.lowestElevation, GameRam.planet.highestElevation, inv);

        if (tile.index == Vector2Int.zero)
            tile.parentCell.transform.position = new Vector3(tile.parentCell.transform.position.x, elevation, tile.parentCell.transform.position.z);

        Color col = elevationGradient.Evaluate(inv);
        if ((tile.index.x % 2 == 0 && tile.index.y % 2 == 0) || (tile.index.x % 2 == 1 && tile.index.y % 2 == 1))
        {
            col.r -= 0.025f;
            col.g -= 0.025f;
            col.b -= 0.025f;
        }

        tile.parentCell.AddTile(tile);
        return col;
        //Debug.LogFormat("Finished Generation of Tile {0}.", tile.tileIndex.ToString());
    }

    public void AddRiver(Vector2Int[] indices, int depth = 7)
    {
        indices = RebalanceIndex(indices);
        if (rivers.ContainsKey(chunkContainers[indices[0]].chunk.GetCell(indices[1]).GetTile(indices[2]).coordinates) || depth <= 0)
        {
            Debug.LogFormat("Reached end of river at {0}, {1}, {2}", indices[0].ToString(), indices[1].ToString(), indices[2].ToString());
            return;
        }

        Vector3 tilePosition = Indices2Position(indices);
        GameObject riverObject = Instantiate(tilePrefab, tilePosition, Quaternion.identity, landRoot);
        rivers.Add(chunkContainers[indices[0]].chunk.GetCell(indices[1]).GetTile(indices[2]).coordinates, riverObject);

        Vector3 lowestPosition = tilePosition;
        List<Vector3> positions = new();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                positions.Add(Indices2Position(indices[0], indices[1], new Vector2Int(indices[2].x + x, indices[2].y + y)));
            }
        }

        positions = positions.OrderBy(x => x.y).ToList();

        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int[] newdices = Position2Indices(positions[i]);
            if (!rivers.ContainsKey(chunkContainers[newdices[0]].chunk.GetCell(newdices[1]).GetTile(newdices[2]).coordinates))
            {
                AddRiver(newdices, depth - 1);
                break;
            }
        }

        //if (lowestPosition != tilePosition)
        //{
        //    Vector2Int[] newIndices = Position2Indices(lowestPosition);
        //    if (!lakes.ContainsKey(chunkContainers[newIndices[0]].chunk.GetCell(newIndices[1]).GetTile(newIndices[2]).coordinates))
        //        AddRiver(newIndices, depth - 1);
        //    //else
        //    //    SetAsLake(indices, riverObject, tilePosition, depth - 1);
        //    //return;
        //}
        ////else
        ////    SetAsLake(indices, riverObject, tilePosition, depth);
    }

    void SetAsLake(Vector2Int[] indices, GameObject riverObject, Vector3 position, int depth)
    {
        lakes.Add(chunkContainers[indices[0]].chunk.GetCell(indices[1]).GetTile(indices[2]).coordinates, riverObject);
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                AddRiver(Position2Indices(new Vector3(position.x + x * tileEdgeLength, 0, position.z + y * tileEdgeLength)), depth);
            }
        }
    }

    public Vector3 Indices2Position(Vector2Int[] indices)
    {
        return Indices2Position(indices[0], indices[1], indices[2]);
    }

    public Vector3 Indices2Position(Vector2Int chunkIndex, Vector2Int cellIndex, Vector2Int tileIndex)
    {
        if (!fullyInitialized)
            return Vector3.zero;

        Vector2Int[] newIndices = RebalanceIndex(new Vector2Int[3] { chunkIndex, cellIndex, tileIndex });
        //if (newIndices[2] != tileIndex)
        //    Debug.LogFormat("Comparison: {0}, {1}, {2} vs {3}, {4}, {5}", chunkIndex, cellIndex, tileIndex, newIndices[0], newIndices[1], newIndices[2]);

        Chunk chunkToCheck = null;
        foreach (var chunkContainer in chunkContainers)
        {
            //Debug.LogFormat("Testing chunk {0} for {1}...", loadedChunks[i].index, chunkIndex);
            if (chunkContainer.Key == newIndices[0])
            {
                //Debug.LogFormat("Chunk {0} found.", chunkIndex);
                chunkToCheck = chunkContainer.Value.chunk;
                break;
            }
        }

        if (chunkToCheck == null)
        {
            Debug.LogErrorFormat("Chunk {0} (previously {1}) returned as null.", newIndices[0], chunkIndex);
            return Vector3.zero;
        }
        Cell cell = chunkToCheck.GetCell(newIndices[1]);
        if (cell == null)
        {
            Debug.LogErrorFormat("Cell {0} (previously {1}) returned as null.", newIndices[1], cellIndex);
            return Vector3.zero;
        }
        Tile tile = cell.GetTile(newIndices[2]);
        if (tile == null)
        {
            Debug.LogErrorFormat("Tile {0} (previously {1}) returned as null.", newIndices[2], tileIndex);
            return Vector3.zero;
        }

        Vector3 location;
        float percentOfHeight = Mathf.InverseLerp(GameRam.planet.minHeight, GameRam.planet.maxHeight, tile.elevation);
        float heightToElevation = Mathf.Lerp(GameRam.planet.lowestElevation, GameRam.planet.highestElevation, percentOfHeight);
        location.y = heightToElevation;
        //Debug.LogFormat("Elevation {0}, Y {1},\nroot height {2}, interpolated height {3}.", tile.elevation, heightToElevation, currentLocale.avgElevation, percentOfHeight);

        location.x = cell.transform.position.x + tileEdgeLength * newIndices[2].x;
        location.z = cell.transform.position.z + tileEdgeLength * newIndices[2].y;

        return location;
    }

    public Vector2Int[] Position2Indices(Vector3 position)
    {
        Vector2Int[] newIndices = new Vector2Int[3];
        Vector2[] floatIndices = new Vector2[3];

        if (!fullyInitialized)
            return newIndices;

        floatIndices[0] = new Vector2(
            position.x / chunkEdgeLength,
            position.z / chunkEdgeLength);

        floatIndices[1] = new Vector2(
            position.x % chunkEdgeLength / cellEdgeLength,
            position.z % chunkEdgeLength / cellEdgeLength);
        if (floatIndices[1].x < 0)
            floatIndices[1].x += cellsPerChunk;
        if (floatIndices[1].y < 0)
            floatIndices[1].y += cellsPerChunk;

        floatIndices[2] = new Vector2(
            position.x % chunkEdgeLength % cellEdgeLength / tileEdgeLength,
            position.z % chunkEdgeLength % cellEdgeLength / tileEdgeLength);
        if (floatIndices[2].x < 0)
            floatIndices[2].x += tilesPerCell;
        if (floatIndices[2].y < 0)
            floatIndices[2].y += tilesPerCell;

        newIndices[0] = new Vector2Int(Mathf.FloorToInt(floatIndices[0].x), Mathf.FloorToInt(floatIndices[0].y));
        newIndices[1] = new Vector2Int(Mathf.FloorToInt(floatIndices[1].x), Mathf.FloorToInt(floatIndices[1].y));
        newIndices[2] = new Vector2Int(Mathf.FloorToInt(floatIndices[2].x), Mathf.FloorToInt(floatIndices[2].y));

        return newIndices;
    }

    public Vector2Int[] RebalanceIndex(Vector2Int[] indices)
    {
        Vector2Int[] newIndices = new Vector2Int[] { indices[0], indices[1], indices[2] };

        //Tile X
        if (newIndices[2].x >= tilesPerCell)
        {
            newIndices[1].x += Mathf.FloorToInt(newIndices[2].x / tilesPerCell);
            newIndices[2].x = newIndices[2].x % tilesPerCell;
            //Debug.LogFormat("Cell {0} => {1}, Tile {2} => {3}", indices[1], newIndices[1], indices[2], newIndices[2]);
        }
        else if (newIndices[2].x < 0)
        {
            newIndices[1].x += Mathf.FloorToInt(newIndices[2].x / tilesPerCell) - 1;
            newIndices[2].x = ((newIndices[2].x % tilesPerCell) + tilesPerCell) % tilesPerCell;
            //Debug.LogFormat("Cell {0} => {1}, Tile {2} => {3}", indices[1], newIndices[1], indices[2], newIndices[2]);
        }

        //Tile Y
        if (newIndices[2].y >= tilesPerCell)
        {
            newIndices[1].y += Mathf.FloorToInt(newIndices[2].y / tilesPerCell);
            newIndices[2].y = newIndices[2].y % tilesPerCell;
            //Debug.LogFormat("Cell {0} => {1}, Tile {2} => {3}", indices[1], newIndices[1], indices[2], newIndices[2]);
        }
        else if (newIndices[2].y < 0)
        {
            newIndices[1].y += Mathf.FloorToInt(newIndices[2].y / tilesPerCell) - 1;
            newIndices[2].y = ((newIndices[2].y % tilesPerCell) + tilesPerCell) % tilesPerCell;
            //Debug.LogFormat("Cell {0} => {1}, Tile {2} => {3}", indices[1], newIndices[1], indices[2], newIndices[2]);
        }

        //Cell X
        if (newIndices[1].x >= cellsPerChunk)
        {
            newIndices[0].x += Mathf.FloorToInt(newIndices[1].x / cellsPerChunk);
            newIndices[1].x = newIndices[1].x % cellsPerChunk;
            //Debug.LogFormat("Chunk {0} => {1}, Cell {2} => {3}", indices[0], newIndices[0], indices[1], newIndices[1]);
        }
        else if (newIndices[1].x < 0)
        {
            newIndices[0].x += Mathf.FloorToInt(newIndices[1].x / cellsPerChunk) - 1;
            newIndices[1].x = ((newIndices[1].x % cellsPerChunk) + cellsPerChunk) % cellsPerChunk;
            //Debug.LogFormat("Chunk {0} => {1}, Cell {2} => {3}", indices[0], newIndices[0], indices[1], newIndices[1]);
        }

        //Cell Y
        if (newIndices[1].y >= cellsPerChunk)
        {
            newIndices[0].y += Mathf.FloorToInt(newIndices[1].y / cellsPerChunk);
            newIndices[1].y = newIndices[1].y % cellsPerChunk;
            //Debug.LogFormat("Chunk {0} => {1}, Cell {2} => {3}", indices[0], newIndices[0], indices[1], newIndices[1]);
        }
        else if (newIndices[1].y < 0)
        {
            newIndices[0].y += Mathf.FloorToInt(newIndices[1].y / cellsPerChunk) - 1;
            newIndices[1].y = ((newIndices[1].y % cellsPerChunk) + cellsPerChunk) % cellsPerChunk;
            //Debug.LogFormat("Chunk {0} => {1}, Cell {2} => {3}", indices[0], newIndices[0], indices[1], newIndices[1]);
        }

        //if (indices[2] != newIndices[2])
        //    Debug.LogFormat("Rebalanced {0}, {1}, {2} to {3}, {4}, {5}.",
        //        indices[0].ToString(),
        //        indices[1].ToString(),
        //        indices[2].ToString(),
        //        newIndices[0].ToString(),
        //        newIndices[1].ToString(),
        //        newIndices[2].ToString());

        return newIndices;
    }
}
