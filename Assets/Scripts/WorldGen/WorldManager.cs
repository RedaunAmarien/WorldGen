using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using System;

[System.Serializable]
public class WorldManager : MonoBehaviour
{
    [Header("Map Settings")]
    public Vector2Int mapImageSize;
    [Min(1)]
    public float areaMapZoom = 1;
    [Tooltip("Longitude, Latitude?")]
    public float areaMapLatitude;
    public float areaMapLongitude;
    public bool imperialUnits = true;
    Texture2D worldMapTex;
    public int randomLocationCount;
    public GameObject locationPrefab;
    public int locationNameMinCharCount;
    public int locationNameMaxCharCount;

    [Header("Generator Settings")]
    public bool randomizeSeed;
    public FastNoiseLite fNLSettings;
    public int seed;
    public float frequency;
    public FastNoiseLite.NoiseType noiseType;
    public FastNoiseLite.FractalType fractalType;
    public int octaves;
    public float lacunarity;
    public float gain;

    // public Generate generator;

    Vector2 mousePosition;

    //Internal Settings
    //private readonly int quadrantCount = 6;
    Vector2Int regionCount = new(8, 8);
    Vector2Int localeCount = new(16, 16);
    //Vector2Int chunksPerSubLocale = new(16, 16);
    //Vector2Int blockCount = new(16, 16);
    //Vector2Int tilesPerCell = new(16, 16);

    [Header("References")]
    // public Slider progressBar;
    // [System.NonSerialized]
    public int progMax;
    public int progCurrent;
    public int mapProgMax, mapProgCurrent;
    // public Label mapSizeLabel;
    public Button[] menuButtons;
    // public Image map2d;
    Color32[] mapColors, worldMapColors;
    public Gradient elevationGradient;
    // public Gradient heightGrad;
    // public AnimationCurve plainsFlattener;
    // public bool useFlattener;
    public Renderer mapSphere;
    public GameObject currentPin;
    WorldUI uI;

    //Internal
    [System.NonSerialized]
    public List<GameObject> pins;
    //Vector2 mousePosition;
    public bool isLocationAllowed;
    public GlobeView globeView;
    [System.NonSerialized]
    public Vector2Int quadMapSize;

    [Header("Location Hierarchy")]
    public Planet planet;

    void Start()
    {
        planet.worldManager = this;
        GameRam.planet = planet;
        GameRam.worldMapCenter = new Vector2(areaMapLongitude, areaMapLatitude);
        pins = new List<GameObject>();
        uI = GetComponent<WorldUI>();
        fNLSettings = new FastNoiseLite();
        Generate.fnl = fNLSettings;

        GenerateMapButton();
    }

    public async void GenerateMapButton()
    {
        areaMapZoom = 1;
        areaMapLongitude = 0;
        areaMapLatitude = 0;
        if (pins != null)
        {
            foreach (GameObject pin in pins)
            {
                Destroy(pin);
            }
            pins.Clear();
        }

        if (menuButtons != null)
        {
            foreach (Button button in menuButtons)
            {
                button.SetEnabled(false);
            }
            isLocationAllowed = false;
        }

        await GenerateWorld();

        if (menuButtons != null)
        {
            foreach (Button button in menuButtons)
            {
                button.SetEnabled(true);
            }
            isLocationAllowed = true;
        }

        GameRam.planet = planet;

        GenerateLocationButton();
    }

    public void UpdateRam()
    {
        GameRam.NoiseSettings.mSeed = seed;
        GameRam.NoiseSettings.mFrequency = frequency;
        GameRam.NoiseSettings.mFractalType = fractalType;
        GameRam.NoiseSettings.mNoiseType = noiseType;
        GameRam.NoiseSettings.mLacunarity = lacunarity;
        GameRam.NoiseSettings.mOctaves = octaves;
        GameRam.NoiseSettings.mGain = gain;
        GameRam.planet = planet;
    }

    public async Task<bool> GenerateWorld()
    {
        planet.quadrants = new List<Quadrant>();

        // Initialize seed
        if (randomizeSeed)
        {
            seed = (int)System.DateTime.Now.Ticks;
        }

        UnityEngine.Random.InitState(seed);
        fNLSettings.SetSeed(seed);
        fNLSettings.SetFrequency(frequency);
        fNLSettings.SetNoiseType(noiseType);
        fNLSettings.SetFractalType(fractalType);
        fNLSettings.SetFractalOctaves(octaves);
        fNLSettings.SetFractalLacunarity(lacunarity);
        fNLSettings.SetFractalGain(gain);

        UpdateRam();

        // GameRam.NoiseSettings = fNLSettings;

        planet.minHeight = float.MaxValue;
        planet.maxHeight = float.MinValue;

        quadMapSize.x = regionCount.x * localeCount.x;
        quadMapSize.y = regionCount.y * localeCount.y;

        // Initialize progress bar
        progCurrent = 0;
        progMax = (quadMapSize.x * quadMapSize.y * 6);

        // Generate Quadrants
        List<Task> quadrantTasks = new();
        for (int i = 0; i < 6; i++)
        {
            quadrantTasks.Add(GenerateQuadrant(i));
        }

        // Generate each region
        while (quadrantTasks.Count > 0)
        {
            Task finishedTask = await Task.WhenAny(quadrantTasks);
            quadrantTasks.Remove(finishedTask);
        }

        foreach (Quadrant quadrant in planet.quadrants)
        {
            planet.minHeight = Mathf.Min(quadrant.minHeight, planet.minHeight);
            planet.maxHeight = Mathf.Max(quadrant.maxHeight, planet.maxHeight);
        }

        await ApplyQuadrants();
        return true;
    }

    public async Task<Quadrant> GenerateQuadrant(int index)
    {
        Quadrant quadrant = new(index)
        {
            regions = new List<Region>(),
            parentPlanet = planet,

            // Initialize full map texture
            quadrantMapTex = new Texture2D(quadMapSize.x, quadMapSize.y)
        };
        mapColors = new Color32[quadMapSize.x * quadMapSize.y];

        // Initialize Regions
        List<Task> regionTasks = new();
        for (int y = 0; y < regionCount.y; y++)
        {
            for (int x = 0; x < regionCount.x; x++)
            {
                regionTasks.Add(GenerateRegion(quadrant, new Vector2Int(x, y)));
            }
        }

        // Generate each region
        while (regionTasks.Count > 0)
        {
            Task finishedTask = await Task.WhenAny(regionTasks);
            regionTasks.Remove(finishedTask);
        }

        planet.minHeight = Math.Min(quadrant.minHeight, planet.minHeight);
        planet.maxHeight = Math.Max(quadrant.maxHeight, planet.maxHeight);

        planet.quadrants.Add(quadrant);
        return quadrant;
    }

    public async Task<Region> GenerateRegion(Quadrant quadrant, Vector2Int regionIndex)
    {
        // Create region and generate noise
        Region region = new(regionIndex, quadrant)
        {
            regionHeights = new float[localeCount.x, localeCount.y]
        };
        Coordinates[,] coordinates = new Coordinates[localeCount.x, localeCount.y];

        for (int y = 0; y < localeCount.y; y++)
        {
            for (int x = 0; x < localeCount.x; x++)
            {
                Vector3Int uvqCoordinates = new(regionIndex.x * localeCount.x + x, regionIndex.y * localeCount.y + y, quadrant.index);
                coordinates[x, y] = new Coordinates(uvqCoordinates);
                progCurrent++;
            }
        }
        // Debug.Log(coordinates[5,5].ToString());
        region.regionHeights = await Generate.GetNoise(coordinates);
        // Debug.Log(region.regionHeights[5,5].ToString());

        foreach (float height in region.regionHeights)
        {
            // Update map lowest and highest points.
            quadrant.minHeight = Math.Min(height, quadrant.minHeight);
            quadrant.maxHeight = Math.Max(height, quadrant.maxHeight);
            region.minHeight = Math.Min(height, region.minHeight);
            region.maxHeight = Math.Max(height, region.maxHeight);
        }

        quadrant.regions.Add(region);
        return region;
    }

    public async Task<bool> ApplyQuadrants()
    {
        foreach (Quadrant quadrant in planet.quadrants)
        {
            //Evaluate altitudes for colors
            foreach (var region in quadrant.regions)
            {
                for (int x = 0; x < localeCount.x; x++)
                {
                    for (int y = 0; y < localeCount.y; y++)
                    {
                        float altitude = Mathf.InverseLerp(((float)planet.minHeight), ((float)planet.maxHeight), ((float)region.regionHeights[x, y]));
                        int pixIndex = (region.coordinates.y * regionCount.y * localeCount.x * localeCount.y) + (y * regionCount.y * localeCount.y) + (region.coordinates.x * localeCount.x) + x;
                        // if (!isHeightMap) mapColors[pixIndex] = elevationGradient.Evaluate(altitude);
                        // else mapColors[pixIndex] = heightGrad.Evaluate(altitude);
                        mapColors[pixIndex] = elevationGradient.Evaluate(altitude);
                    }
                }
            }

            // Apply color array
            //Debug.Log("Applying globe textures.");
            quadrant.quadrantMapTex.SetPixels32(mapColors);
            quadrant.quadrantMapTex.Apply();

            quadrant.quadrantMapTex.filterMode = FilterMode.Point;
            quadrant.quadrantMapTex.wrapMode = TextureWrapMode.Clamp;
        }
        await ApplyMap(true);
        foreach (Quadrant quadrant in planet.quadrants)
        {
            mapSphere.materials[quadrant.index].mainTexture = quadrant.quadrantMapTex;
        }
        await new WaitForEndOfFrame();
        return true;
    }

    public async Task<bool> ApplyMap(bool isWorld)
    {
        worldMapColors = new Color32[mapImageSize.x * mapImageSize.y];
        List<Task> mapTasks = new();
        int mapRegionsX = 32;
        int mapRegionsY = 16;

        for (int y = 0; y < mapRegionsY; y++)
        {
            for (int x = 0; x < mapRegionsX; x++)
            {
                mapTasks.Add(GenerateMapColors(new Vector2Int(x, y), new Vector2Int(mapImageSize.x / mapRegionsX, mapImageSize.y / mapRegionsY)));
            }
        }


        await Awaitable.BackgroundThreadAsync();
        while (mapTasks.Count > 0)
        {
            Task finishedTask = await Task.WhenAny(mapTasks);
            mapTasks.Remove(finishedTask);
            // Debug.LogFormat("Awaiting map tasks... {0} remain.", mapTasks.Count);
        }
        await Awaitable.MainThreadAsync();

        // Apply separate array to world map
        //Debug.LogFormat("Applying world map texture.");
        worldMapTex = new Texture2D(mapImageSize.x, mapImageSize.y)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        worldMapTex.SetPixels32(worldMapColors);
        worldMapTex.Apply();
        // Debug.LogFormat("World map is {1}x{2} or {0} pixels total.", worldMapColors.Length, worldMapTex.width, worldMapTex.height);

        if (isWorld)
        {
            uI.SetWorldMap(worldMapTex);
        }
        else
        {
            uI.SetAreaMap(worldMapTex, currentPin.GetComponent<LocalePin>().linkedLocale);
        }
        await new WaitForEndOfFrame();
        return true;
    }

    public async Task<Color32[]> GenerateMapColors(Vector2Int sectionRoot, Vector2Int range)
    {
        await Awaitable.BackgroundThreadAsync();
        mapProgCurrent = 0;
        mapProgMax = mapImageSize.x * mapImageSize.y;
        // Debug.LogFormat("Generating colors for region {0}, {1} of world map.", section.x, section.y);
        for (int y = 0; y < range.y; y++)
        {
            float pointY = sectionRoot.y * range.y + y;
            for (int x = 0; x < range.x; x++)
            {
                float pointX = sectionRoot.x * range.x + x;
                float longi = (pointX / mapImageSize.x) * (360 / areaMapZoom) + (-180 / areaMapZoom);
                float lati = (pointY / mapImageSize.y) * (180 / areaMapZoom) + (-90 / areaMapZoom);
                float height = await Generate.GetNoise(new Coordinates(longi, lati), new Vector2(areaMapLongitude, areaMapLatitude));

                height = Mathf.InverseLerp(planet.minHeight, planet.maxHeight, height);
                mapProgCurrent++;
                worldMapColors[Mathf.FloorToInt(pointY) * mapImageSize.x + Mathf.FloorToInt(pointX)] = elevationGradient.Evaluate(height);
            }
            // Debug.LogFormat("Analyzed part of row {0}.", y);
        }
        // Debug.LogFormat("Generated colors for region {0}, {1} of size {2} world map.", section.x, section.y, colors.Length);
        await Awaitable.MainThreadAsync();
        return null;
    }

    void OnPosition(InputValue val)
    {
        {
            mousePosition = val.Get<Vector2>();
        }
    }

    async void OnLocate()
    {
        if (!isLocationAllowed)
            return;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition), out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Planet"))
            {
                Debug.DrawLine(Camera.main.ScreenPointToRay(mousePosition).origin, hit.point, Color.green, 60);
                int u = Mathf.FloorToInt(hit.textureCoord.x * quadMapSize.x);
                int v = Mathf.FloorToInt(hit.textureCoord.y * quadMapSize.y);

                MeshFilter mf = (MeshFilter)globeView.gameObject.GetComponentInChildren(typeof(MeshFilter));
                Mesh mesh = mf.mesh;

                int totalSubMeshes = mesh.subMeshCount;
                int[] subMeshesFaceTotals = new int[totalSubMeshes];

                for (int i = 0; i < totalSubMeshes; i++)
                {
                    subMeshesFaceTotals[i] = mesh.GetTriangles(i).Length / 3;
                }

                int q = -1;
                int maxVal = 0;

                for (int i = 0; i < totalSubMeshes; i++)
                {
                    maxVal += subMeshesFaceTotals[i];

                    if (hit.triangleIndex <= maxVal - 1)
                    {
                        q = i;
                        break;
                    }

                }

                if (q < 0) Debug.LogError("Quad not detected.");
                // else Debug.LogFormat("UVQ uvqCoordinates for new manager is {0},{1} on {2}", u, v, q);

                await GenerateLocation(new Vector3Int(u, v, q), true, true);
            }
            else if (hit.collider.CompareTag("Pin"))
            {
                Debug.DrawLine(Camera.main.ScreenPointToRay(mousePosition).origin, hit.point, Color.blue, 60);
                LocalePin pin = hit.collider.gameObject.GetComponentInParent<LocalePin>();
                currentPin = pin.gameObject;
                areaMapLatitude = pin.linkedLocale.coordinates.latitude;
                areaMapLongitude = pin.linkedLocale.coordinates.longitude;
                areaMapZoom = 512;
            }

            await ApplyMap(false);
        }
    }

    public async void GenerateLocationButton()
    {
        if (!isLocationAllowed)
        {
            return;
        }

        for (int i = 0; i < randomLocationCount; i++)
        {
            int u = UnityEngine.Random.Range(0, quadMapSize.x);
            int v = UnityEngine.Random.Range(0, quadMapSize.y);
            int q = UnityEngine.Random.Range(0, 6);
            while (!await GenerateLocation(new Vector3Int(u, v, q), true, false))
            {
                u = UnityEngine.Random.Range(0, quadMapSize.x);
                v = UnityEngine.Random.Range(0, quadMapSize.y);
                q = UnityEngine.Random.Range(0, 6);
            }
        }
    }

    public async Task<bool> GenerateLocation(Vector3Int uvqCoord, bool genName, bool allowOcean)
    {
        Locale locale = new()
        {
            coordinates = new Coordinates(uvqCoord)
        };

        GameObject pin = GameObject.Instantiate(locationPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("Planet").transform);
        LocalePin pinScript = pin.GetComponentInChildren<LocalePin>();

        locale.timeZone = Mathf.RoundToInt((float)locale.coordinates.longitude / 360 * 24);

        double elevation = await Generate.GetNoise(locale.coordinates);
        elevation = Mathf.InverseLerp(((float)planet.minHeight), ((float)planet.maxHeight), ((float)elevation));
        elevation = Mathf.Lerp(planet.lowestElevation, planet.highestElevation, ((float)elevation));
        if (elevation < 0 && !allowOcean)
        {
            Destroy(pin);
            //Debug.Log("Failed creation of manager due to water.");
            return false;
        }
        locale.avgElevation = elevation;

        pin.transform.localPosition = locale.coordinates.cartesianPosition;
        pin.transform.LookAt(GameObject.Find("Planet").transform, GameObject.Find("Planet").transform.up);

        if (genName)
        {
            int nameLength = UnityEngine.Random.Range(locationNameMinCharCount, locationNameMaxCharCount + 1);
            locale.placeName = Generate.Name(nameLength);
        }
        else
        {
            locale.placeName = "[Unnamed]";
        }
        locale.description = string.Format("{0}\nRoot Height: {1}", locale.coordinates.ToString(), locale.avgElevation);
        pin.name = locale.placeName;
        currentPin = pin;

        int regionIndex = Mathf.FloorToInt((float)(uvqCoord.y / localeCount.y) * regionCount.x + (float)(uvqCoord.x / localeCount.x));
        // Debug.LogFormat("Creating pin at {0},{1} (region {3}) on quadrant {2}.", uvqCoord.x, uvqCoord.y, uvqCoord.z, regionIndex);

        pinScript.linkedLocale = locale;
        pins.Add(pin);
        if (planet.quadrants[uvqCoord.z].regions[regionIndex].locales == null)
        {
            planet.quadrants[uvqCoord.z].regions[regionIndex].locales = new List<Locale>();
        }
        planet.quadrants[uvqCoord.z].regions[regionIndex].locales.Add(locale);

        areaMapLatitude = locale.coordinates.latitude;
        areaMapLongitude= locale.coordinates.longitude;
        areaMapZoom = 512;

        return true;
    }

    public async void RegenMapButton()
    {
        await ApplyMap(false);
    }

    public void SaveMap()
    {
        FileIO.SaveFile();
    }

    public void ExportMap()
    {
        FileIO.ExportMap(worldMapTex);
    }

    public void LoadLocation()
    {
        currentPin.GetComponent<LocalePin>().ChooseLocation();
    }

    public void LoadedPlanet(Planet p)
    {
        planet = p;
    }

    public void LoadMap()
    {
        // isLocationAllowed = false;
        foreach (GameObject pin in pins)
        {
            Destroy(pin);
        }
        pins.Clear();

        GameRam.FromSaveData(FileIO.LoadFile());
        LoadedPlanet(GameRam.planet);
        GenerateMapButton();
    }

    public void ToggleImperial()
    {
        imperialUnits = !imperialUnits;
    }

    // void OnValidate()
    // {
    //     float globeRadius = 6378.1f * planet.globeScale;
    //     float fullMapWidth = globeRadius * 2 * Mathf.PI;
    //     float mapWidthMiles = fullMapWidth * 0.621371f;
    //     // mapSizeLabel.text = string.Format("Map Width: {0} {2} (across center only)\nMap Height: {1} {2}", useMiles ? mapWidthMiles : fullMapWidth, useMiles ? mapWidthMiles / 2 : fullMapWidth / 2, useMiles ? "mi": "km");

    //     if (autoUpdate) {
    //         GenerateMapButton();
    //     }
    // }
}