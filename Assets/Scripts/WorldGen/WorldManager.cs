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
    public Vector2Int worldMapSize;
    [Min(1)]
    public float worldMapZoom = 1;
    public Vector2 worldMapLocation;
    public bool imperialUnits = true;
    Texture2D worldMapTex;
    public int randomLocCount;
    public GameObject locationPrefab;

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

    //Internal Settings
    int quadrantCount = 6;
    Vector2Int regionCount = new Vector2Int(8,8);
    Vector2Int localeCount = new Vector2Int(16, 16);
    Vector2Int chunkCount = new Vector2Int(16, 16);
    Vector2Int blockCount = new Vector2Int(16, 16);
    Vector2Int tileCount = new Vector2Int(16, 16);
    
    [Header("References")]
    // public Slider progressBar;
    // [System.NonSerialized]
    public int progMax;
    public int progCurrent;
    public int mapProgMax, mapProgCurrent;
    // public Label mapSizeLabel;
    public Button[] menuButtons;
    // public Image map2d;
    Color[] mapColors, worldMapColors;
    public Gradient elevGrad;
    // public Gradient heightGrad;
    // public AnimationCurve plainsFlattener;
    // public bool useFlattener;
    public Renderer mapSphere;
    public GameObject currentPin;
    WorldUI uI;

    //Internal
    [System.NonSerialized]
    public List<GameObject> pins;
    Vector2 mousPos;
    public bool isLocationAllowed;
    public GlobeView globeView;
    [System.NonSerialized]
    public Vector2Int quadMapSize;

    [Header("Location Hierarchy")]
    public Planet planet;

    void Start() {
        planet.worldManager = this;
        GameRam.planet = planet;
        GameRam.worldMapCenter = worldMapLocation;
        pins = new List<GameObject>();
        uI = GetComponent<WorldUI>();
        fNLSettings = new FastNoiseLite();
        Generate.fnl = fNLSettings;
    }

    public async void GenerateMapButton()
    {
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
        List<Task> quadrantTasks = new List<Task>();
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
        Quadrant quadrant = new Quadrant(index);
        quadrant.regions = new List<Region>();
        quadrant.pPlanet = planet;

        // Initialize full map texture
        quadrant.quadrantMapTex = new Texture2D(quadMapSize.x, quadMapSize.y);
        mapColors = new Color[quadMapSize.x * quadMapSize.y];

        // Initialize Regions
        List<Task> regionTasks = new List<Task>();
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

    public async Task<Region> GenerateRegion(Quadrant quad, Vector2Int reg)
    {
        // Create region and generate noise
        Region region = new Region(reg, quad);
        region.regionHeights = new float[localeCount.x, localeCount.y];
        Vector3Int[,] locs = new Vector3Int[localeCount.x, localeCount.y];

        for (int y = 0; y < localeCount.y; y++)
        {
            for (int x = 0; x < localeCount.x; x++)
            {
                locs[x,y].x = reg.x * localeCount.x + x;
                locs[x,y].y = reg.y * localeCount.y + y;
                locs[x,y].z = quad.index;
                progCurrent ++;
                // Vector3 point = await Conversion.Coordinate.UVQtoXYZ(locs[x,y], quadMapSize);
                // region.regionHeights[x,y] = fNLSettings.GetNoise(point.x, point.y, point.z);
            }
        }
        // Debug.Log(locs[5,5].ToString());
        region.regionHeights = await Generate.GetNoise(locs, quadMapSize);
        // Debug.Log(region.regionHeights[5,5].ToString());

        foreach (float height in region.regionHeights)
        {
            // Update map lowest and highest points.
            quad.minHeight = Math.Min(height, quad.minHeight);
            quad.maxHeight = Math.Max(height, quad.maxHeight);
            region.minHeight = Math.Min(height, region.minHeight);
            region.maxHeight = Math.Max(height, region.maxHeight);
        }

        quad.regions.Add(region);
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
                        float altitude = Mathf.InverseLerp(((float)planet.minHeight), ((float)planet.maxHeight), ((float)region.regionHeights[x,y]));
                        int pixIndex = (region.coordinates.y * regionCount.y * localeCount.x * localeCount.y) + (y * regionCount.y * localeCount.y) + (region.coordinates.x * localeCount.x) + x;
                        // if (!isHeightMap) mapColors[pixIndex] = elevGrad.Evaluate(altitude);
                        // else mapColors[pixIndex] = heightGrad.Evaluate(altitude);
                        mapColors[pixIndex] = elevGrad.Evaluate(altitude);
                    }
                }
            }

            // Apply color array
            //Debug.Log("Applying globe textures.");
            quadrant.quadrantMapTex.SetPixels(mapColors);
            quadrant.quadrantMapTex.Apply();
            
            quadrant.quadrantMapTex.filterMode = FilterMode.Point;
            quadrant.quadrantMapTex.wrapMode = TextureWrapMode.Clamp;
        }
        await ApplyMap();
        foreach (Quadrant quadrant in planet.quadrants)
        {
            mapSphere.materials[quadrant.index].mainTexture = quadrant.quadrantMapTex;
        }
        await new WaitForEndOfFrame();
        return true;
    }

    public async Task<bool> ApplyMap()
    {
        worldMapColors = new Color[worldMapSize.x * worldMapSize.y];
        List<Task> mapTasks = new List<Task>();
        int mapRegionsX = 32;
        int mapRegionsY = 16;
        
        for (int y = 0; y < mapRegionsY; y++)
        {
            for (int x = 0; x < mapRegionsX; x++)
            {
                mapTasks.Add(GenerateMapColors(new Vector2Int(x, y), new Vector2Int(worldMapSize.x/mapRegionsX, worldMapSize.y/mapRegionsY)));
            }
        }

        while (mapTasks.Count > 0)
        {
            Task finishedTask = await Task.WhenAny(mapTasks);
            mapTasks.Remove(finishedTask);
            // Debug.LogFormat("Awaiting map tasks... {0} remain.", mapTasks.Count);
        }
        
        // Apply separate array to world map
        //Debug.LogFormat("Applying world map texture.");
        worldMapTex = new Texture2D(worldMapSize.x, worldMapSize.y);
        worldMapTex.filterMode = FilterMode.Point;
        worldMapTex.wrapMode = TextureWrapMode.Clamp;
        worldMapTex.SetPixels(worldMapColors);
        worldMapTex.Apply();
        // Debug.LogFormat("World map is {1}x{2} or {0} pixels total.", worldMapColors.Length, worldMapTex.width, worldMapTex.height);

        uI.SetMap(worldMapTex);
        await new WaitForEndOfFrame();
        return true;
    }

    public async Task<Color[]> GenerateMapColors(Vector2Int texSection, Vector2Int range)
    {
        mapProgCurrent = 0;
        mapProgMax = worldMapSize.x * worldMapSize.y;
        // Debug.LogFormat("Generating colors for region {0}, {1} of world map.", section.x, section.y);
        for (int y = 0; y < range.y; y++)
        {
            float py = texSection.y * range.y + y;
            for (int x = 0; x < range.x; x++)
            {
                float px = texSection.x * range.x + x;
                Vector2 lnglat;
                lnglat.x = (px / worldMapSize.x) * (360 / worldMapZoom) + (-180 / worldMapZoom);
                lnglat.y = (py / worldMapSize.y) * (180 / worldMapZoom) +  (-90 / worldMapZoom);

                double height = await Generate.GetNoise(lnglat, worldMapLocation);

                // Vector3 point = await Conversion.Coordinate.LLtoXYZ(lnglat.x, lnglat.y);

                // point = Quaternion.AngleAxis(worldMapLocation.y, Vector3.right) * point;
                // point = Quaternion.AngleAxis(-worldMapLocation.x, Vector3.up) * point;

                // float height = await Generate.GetNoise(point);
                // Debug.LogFormat("{0} => {1} & {2} => {3}: {4} at {5}", px, lng, py, lat, point.ToString(), height);
                height = Mathf.InverseLerp(((float)planet.minHeight), ((float)planet.maxHeight), ((float)height));
                mapProgCurrent ++;
                worldMapColors[Mathf.FloorToInt(py) * worldMapSize.x + Mathf.FloorToInt(px)] = elevGrad.Evaluate(((float)height));
            }
            // Debug.LogFormat("Analyzed part of row {0}.", y);
        }
        // Debug.LogFormat("Generated colors for region {0}, {1} of size {2} world map.", section.x, section.y, colors.Length);
        return null;
    }

    void OnPosition(InputValue val)
    {
        {
            mousPos = val.Get<Vector2>();
        }
    }

    async void OnLocate()
    {
        if (!isLocationAllowed)
            return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(mousPos), out hit))
        {
            Debug.DrawLine(Camera.main.ScreenPointToRay(mousPos).origin, hit.point, Color.green, 60);
            int u = Mathf.FloorToInt(hit.textureCoord.x * quadMapSize.x);
            int v = Mathf.FloorToInt(hit.textureCoord.y * quadMapSize.y);

            MeshFilter mf = (MeshFilter)globeView.gameObject.GetComponentInChildren(typeof(MeshFilter));
            Mesh mesh = mf.mesh;
     
            int totalSubMeshes = mesh.subMeshCount;
            int[] subMeshesFaceTotals= new int[totalSubMeshes];
     
            for(int i = 0; i < totalSubMeshes; i++)  
            {
                subMeshesFaceTotals[i] = mesh.GetTriangles(i).Length /3;
            }

            int q = -1;
            int maxVal = 0;
            
            for(int i = 0; i < totalSubMeshes; i++)  
            {
                maxVal += subMeshesFaceTotals[i];
            
                if (hit.triangleIndex <= maxVal - 1 )
                {      
                    q = i;
                    break;
                }
    
            }

            if (q < 0) Debug.LogError("Quad not detected.");
            // else Debug.LogFormat("UVQ coord for new location is {0},{1} on {2}", u, v, q);

            await GenerateLocation(new Vector3Int(u, v, q), false);
        }
    }

    public async void GenerateLocationButton()
    {
        if (!isLocationAllowed)
            return;

        for (int i = 0; i < randomLocCount; i++)
        {
            int q = UnityEngine.Random.Range(0,6);
            int u = UnityEngine.Random.Range(0,quadMapSize.x);
            int v = UnityEngine.Random.Range(0,quadMapSize.y);
            while (!await GenerateLocation(new Vector3Int(u, v, q), true))
            {
                q = UnityEngine.Random.Range(0,6);
                u = UnityEngine.Random.Range(0,quadMapSize.x);
                v = UnityEngine.Random.Range(0,quadMapSize.y);
            }
        }
    }

    public async Task<bool> GenerateLocation(Vector3Int uvqCoord, bool genName)
    {
        Locale loca = new Locale();
        loca.uvqCoord = uvqCoord;

        GameObject pin = GameObject.Instantiate(locationPrefab, Vector3.zero, Quaternion.identity, GameObject.Find("Planet").transform);
        LocalePin pinScript = pin.GetComponentInChildren<LocalePin>();

        loca.xyzCoord = await Conversion.Coordinate.UVQtoXYZ(uvqCoord, quadMapSize);
        loca.longLatCoord = await Conversion.Coordinate.XYZtoLL(loca.xyzCoord);
        loca.timeZone = Mathf.RoundToInt((float)loca.longLatCoord.x / 360 * 24);

        double elevation = 0;
        elevation = await Generate.GetNoise(loca.xyzCoord);
        elevation = Mathf.InverseLerp(((float)planet.minHeight), ((float)planet.maxHeight), ((float)elevation));
        elevation = Mathf.Lerp(planet.minElevation, planet.maxElevation, ((float)elevation));
        if (elevation < 0)
        {
            Destroy(pin);
            //Debug.Log("Failed creation of location due to water.");
            return false;
        }
        loca.avgElevation = elevation;

        pin.transform.localPosition = loca.xyzCoord;
        pin.transform.LookAt(GameObject.Find("Planet").transform, GameObject.Find("Planet").transform.up);

        if (genName)
        {
            int r = UnityEngine.Random.Range(3,10);
            string name = string.Empty;
            char[] con = new char[]
                {'b','c','d','f','g','h','j','k','l','m','n','p','q','r','s','t','v','w','x','y','z',/*'ɴ','\'','ŋ','β','ʃ','χ'*/};
            char[] vow = new char[]
                {'a','e','i','o','u','y',/*'á','é','í','ó','ú','ü','æ','ɪ','œ','ʏ'*/};
            for (int i = 0; i < r; i++)
            {
                if (i == 0)
                    name += (char)UnityEngine.Random.Range('A','Z'+1);
                else if (i % 2 == 0)
                    name += con[UnityEngine.Random.Range(0,con.Length)];
                else
                    name += vow[UnityEngine.Random.Range(0,vow.Length)];
            }
            loca.placeName = name;
        }
        else
        {
            loca.placeName = string.Format("{0:n2}°{1} {2:n2}°{3}", Math.Abs(loca.longLatCoord.y), loca.longLatCoord.y > 0 ? "N" : "S", Math.Abs(loca.longLatCoord.x), loca.longLatCoord.x > 0 ? "E" : "W");
        }
        // loca.placeName = string.Format("{0}\n{1}\n{2}", loca.longLatCoord.ToString(), loca.xyzCoord.ToString(), loca.uvqCoord.ToString());
        pin.gameObject.name = loca.placeName;
        currentPin = pin;

        int regIndex = Mathf.FloorToInt((float)(uvqCoord.y/localeCount.y) * regionCount.x + (float)(uvqCoord.x/localeCount.x));
        // Debug.LogFormat("Creating pin at {0},{1} (region {3}) on quadrant {2}.", uvqCoord.x, uvqCoord.y, uvqCoord.z, regIndex);

        pinScript.linkedLocale = loca;
        pins.Add(pin);
        if (planet.quadrants[uvqCoord.z].regions[regIndex].locales == null)
        {
            planet.quadrants[uvqCoord.z].regions[regIndex].locales = new List<Locale>();
        }
        planet.quadrants[uvqCoord.z].regions[regIndex].locales.Add(loca);
        return true;
    }

    public async void RegenMapButton()
    {
        await GenerateMapColors(new Vector2Int(0,0), new Vector2Int(worldMapSize.x, worldMapSize.y));
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