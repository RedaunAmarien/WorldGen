using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Threading.Tasks;

public class Generator : MonoBehaviour
{
    [Header("Generation Settings")]
    public FastNoiseLite noise;
    public enum NoiseType 
    { 
        OpenSimplex2,
        OpenSimplex2S,
        Cellular,
        Perlin,
        ValueCubic,
        Value 
    }
    public enum FractalType 
    {
        None, 
        FBm, 
        Ridged, 
        PingPong, 
        DomainWarpProgressive, 
        DomainWarpIndependent 
    }

    [Header("General Settings")]
    public bool autoUpdate;
    public bool useMiles;
    Texture2D worldMapTex;

    [Header("Preset Locations")]
    public int randomLocCount;
    public GameObject locationPrefab;

    [Header("Internal Settings")]
    public int quadrantCount = 6;
    public Vector2Int regionCount = new Vector2Int(8,8);
    public Vector2Int localeCount = new Vector2Int(16, 16);
    public Vector2Int chunkCount = new Vector2Int(16, 16);
    public Vector2Int blockCount = new Vector2Int(16, 16);
    public Vector2Int tileCount = new Vector2Int(16, 16);
    
    [Header("References")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    [System.NonSerialized]
    public int progMax, progCurrent;
    public TextMeshProUGUI mapSizeLabel;
    public Button[] menuButtons;
    public RawImage map2d;
    public Vector2Int worldMapSize;
    [Min(1)]
    public float worldMapZoom = 1;
    public Vector2 worldMapLocation;
    Color[] mapColors, worldMapColors;
    public Gradient elevGrad;
    public Gradient heightGrad;
    public Renderer mapSphere;
    public GameObject currentPin;

    //Internal
    [System.NonSerialized]
    public List<GameObject> pins;
    Vector2 mousPos;
    public bool isLocationAllowed;
    GlobeView globeView;
    [System.NonSerialized]
    public Vector2Int quadMapSize;

    [Header("Location Hierarchy")]
    public Planet planet;

    void Start() {
        globeView = GetComponent<GlobeView>();
        planet.generator = this;
        GameRam.planet = planet;
        pins = new List<GameObject>();
    }

    public async void GenerateMapButton()
    {
        foreach (GameObject pin in pins)
        {
            Destroy(pin);
        }
        pins.Clear();

        foreach (Button button in menuButtons)
        {
            button.interactable = false;
        }
        isLocationAllowed = false;

        await GenerateWorld();

        foreach (Button button in menuButtons)
        {
            button.interactable = true;
        }
        isLocationAllowed = true;

        GameRam.planet = planet;
    }

    public async Task<bool> GenerateWorld()
    {
        planet.quadrants = new List<Quadrant>();

        // Initialize seed
        if (!planet.useCustomSeed)
        {
            planet.seed = (int)System.DateTime.Now.Ticks;
        }

        Random.InitState(planet.seed);
        
        InitializeNoise();

        planet.minHeight = float.MaxValue;
        planet.maxHeight = float.MinValue;
        quadMapSize.x = regionCount.x * localeCount.x;
        quadMapSize.y = regionCount.y * localeCount.y;

        // Initialize progress bar
        progCurrent = 0;
        progMax = (quadMapSize.x * quadMapSize.y * 6) + (worldMapSize.x * worldMapSize.y);

        // Generate Quadrants
        // Debug.Log("Creating Quadrants...");
        List<Task> quadrantTasks = new List<Task>();
        for (int i = 0; i < 6; i++)
        {
            quadrantTasks.Add(GenerateQuadrant(i));
        }

        // Generate each region
        // Debug.LogFormat("Generating {0} regions of size {1}x{2}...", regionTasks.Count, regionSize.x, regionSize.y);
        while (quadrantTasks.Count > 0)
        {
            Task finishedTask = await Task.WhenAny(quadrantTasks);
            quadrantTasks.Remove(finishedTask);
        }
        // Debug.Log("Finished Quadrants");

        foreach (Quadrant quadrant in planet.quadrants)
        {
            if (quadrant.minHeight < planet.minHeight)
                planet.minHeight = quadrant.minHeight;
            if (quadrant.maxHeight > planet.maxHeight)
                planet.maxHeight = quadrant.maxHeight;
        }

        await ApplyQuadrants();
        return true;
    }

    public void InitializeNoise()
    {
        noise = new FastNoiseLite(planet.seed);
        noise.SetFractalOctaves(planet.totalOctaves);
        noise.SetFractalLacunarity(planet.lacunarity);
        noise.SetFractalWeightedStrength(planet.noiseStrength);
        noise.SetFractalGain(planet.persistence);
        noise.SetFrequency(planet.noiseFrequency);
        noise.SetFractalType((FastNoiseLite.FractalType)planet.fractalType);
        noise.SetRotationType3D(FastNoiseLite.RotationType3D.ImproveXYPlanes);
        noise.SetNoiseType((FastNoiseLite.NoiseType)planet.noiseType);
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
        // Debug.Log("Creating Regions...");
        List<Task> regionTasks = new List<Task>();
        for (int y = 0; y < regionCount.y; y++)
        {
            for (int x = 0; x < regionCount.x; x++)
            {
                regionTasks.Add(GenerateRegion(quadrant, new Vector2Int(x, y)));
            }
        }

        // Generate each region
        // Debug.LogFormat("Generating {0} regions of size {1}x{2}...", regionTasks.Count, regionSize.x, regionSize.y);
        while (regionTasks.Count > 0)
        {
            Task finishedTask = await Task.WhenAny(regionTasks);
            regionTasks.Remove(finishedTask);
        }
        // Debug.Log("Finished Regions");

        if (quadrant.minHeight < planet.minHeight)
            planet.minHeight = quadrant.minHeight;
        if (quadrant.maxHeight > planet.maxHeight)
            planet.maxHeight = quadrant.maxHeight;

        // Debug.Log(mapMinMax.ToString());
        planet.quadrants.Add(quadrant);
        return quadrant;
    }

    public async Task<Region> GenerateRegion(Quadrant quad, Vector2Int loc)
    {
        // Create region and generate noise
        // Debug.LogFormat("Region at {0},{1} beginning generation...", loc.x, loc.y);

        Region region = new Region(loc, quad);
        region.regionHeights = await GenerateNoise(loc, localeCount, quad.index);

        // Convert noise to elevation colors
        Color[] regionColors = new Color[localeCount.x * localeCount.y];
        for (int y = 0; y < localeCount.y; y++)
        {
            for (int x = 0; x < localeCount.x; x++)
            {
                // Update map lowest and highest points.
                if (region.regionHeights[x,y] > quad.maxHeight)
                {
                    quad.maxHeight = region.regionHeights[x,y];
                    region.maxHeight = region.regionHeights[x,y];
                }
                else if (region.regionHeights[x,y] < quad.minHeight)
                {
                    quad.minHeight = region.regionHeights[x,y];
                    region.minHeight = region.regionHeights[x,y];
                }
            }
        }

        // Debug.LogFormat("Region {0},{1} finished.\nMax at {2}, Min at {3}. Size of {4}x{5}.", loc.x, loc.y, mapMax, mapMin, size.x, size.y);
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
                        float altitude = Mathf.InverseLerp(planet.minHeight, planet.maxHeight, region.regionHeights[x,y]);
                        int pixIndex = (region.coordinates.y * regionCount.y * localeCount.x * localeCount.y) + (y * regionCount.y * localeCount.y) + (region.coordinates.x * localeCount.x) + x;
                        // if (!isHeightMap) mapColors[pixIndex] = elevGrad.Evaluate(altitude);
                        // else mapColors[pixIndex] = heightGrad.Evaluate(altitude);
                        mapColors[pixIndex] = elevGrad.Evaluate(altitude);
                    }
                }
            }

            // Apply color array
            Debug.Log("Applying globe textures.");
            quadrant.quadrantMapTex.SetPixels(mapColors);
            quadrant.quadrantMapTex.Apply();
            
            quadrant.quadrantMapTex.filterMode = FilterMode.Point;
            quadrant.quadrantMapTex.wrapMode = TextureWrapMode.Clamp;
            mapSphere.materials[quadrant.index].mainTexture = quadrant.quadrantMapTex;
        }
        await ApplyMap();
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
        Debug.LogFormat("Applying world map texture.");
        worldMapTex = new Texture2D(worldMapSize.x, worldMapSize.y);
        worldMapTex.SetPixels(worldMapColors);
        worldMapTex.filterMode = FilterMode.Point;
        worldMapTex.wrapMode = TextureWrapMode.Clamp;
        worldMapTex.Apply();
        map2d.texture = worldMapTex;
        await new WaitForEndOfFrame();
        return true;
    }

    public async Task<Color[]> GenerateMapColors(Vector2Int section, Vector2Int range)
    {
        Color[] colors = new Color[range.x * range.y];
        for (int y = 0; y < range.y; y++)
        {
            for (int x = 0; x < range.x; x++)
            {
                int px = section.x * range.x + x;
                int py = section.y * range.y + y;
                float lng = (float)(px / worldMapSize.x * 360 / worldMapZoom) - (180/worldMapZoom) + worldMapLocation.x;
                float lat = (float)(py / worldMapSize.y * 180 / worldMapZoom) - (90/worldMapZoom) + worldMapLocation.y;

                Vector3 point = await Conversion.Coordinate.LLtoXYZ(lng, lat);
                float height = noise.GetNoise(point.x, point.y, point.z);
                height = Mathf.InverseLerp(planet.minHeight, planet.maxHeight, height);
                progCurrent ++;
                worldMapColors[py * worldMapSize.x + px] = elevGrad.Evaluate(height);
            }
            // Debug.LogFormat("Analyzed part of row {0}.", y);
        }
        return colors;
    }

    public async Task<float[,]> GenerateNoise(Vector2Int loc, Vector2Int size, int quad)
    {
        // Convert each Lat/Long pixel in region into XYZ location and generate 3D noise
        float[,] noiseMatrix = new float[size.x, size.y];

        for (int y = 0; y < size.y; y++)
        {
            int yReal = size.y * loc.y + y;
            for (int x = 0; x < size.x; x++)
            {
                int xReal = size.x * loc.x + x;

                int xCoord = Mathf.FloorToInt(xReal);
                int yCoord = Mathf.FloorToInt(yReal);

                Vector3 newCoord = await Conversion.Coordinate.UVQtoXYZ(new Vector3Int(xCoord, yCoord, quad), quadMapSize);

                float height;
                if (planet.isMode2D) height = noise.GetNoise(xCoord, yCoord);
                else height = noise.GetNoise(newCoord.x, newCoord.y, newCoord.z);
                // height *= octAmp;

                progCurrent ++;
                
                noiseMatrix[x,y] = height;
            }
        }
        return noiseMatrix;
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

            MeshFilter mf = (MeshFilter)gameObject.GetComponentInChildren(typeof(MeshFilter));
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
            int q = Random.Range(0,6);
            int u = Random.Range(0,quadMapSize.x);
            int v = Random.Range(0,quadMapSize.y);
            while (!await GenerateLocation(new Vector3Int(u, v, q), true))
            {
                q = Random.Range(0,6);
                u = Random.Range(0,quadMapSize.x);
                v = Random.Range(0,quadMapSize.y);
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

        float elevation = 0;
        elevation = noise.GetNoise(loca.xyzCoord.x, loca.xyzCoord.y, loca.xyzCoord.z);
        elevation = Mathf.InverseLerp(planet.minHeight, planet.maxHeight, elevation);
        elevation = Mathf.Lerp(planet.minElevation, planet.maxElevation, elevation);
        if (elevation < 0)
        {
            Destroy(pin);
            Debug.Log("Failed creation of location due to water.");
            return false;
        }
        loca.avgElevation = elevation;

        pin.transform.localPosition = loca.xyzCoord;
        pin.transform.LookAt(GameObject.Find("Planet").transform, GameObject.Find("Planet").transform.up);

        if (genName)
        {
            int r = Random.Range(3,10);
            string name = string.Empty;
            char[] con = new char[]
                {'b','c','d','f','g','h','j','k','l','m','n','p','q','r','s','t','v','w','x','y','z',/*'ɴ','\'','ŋ','β','ʃ','χ'*/};
            char[] vow = new char[]
                {'a','e','i','o','u','y',/*'á','é','í','ó','ú','ü','æ','ɪ','œ','ʏ'*/};
            // string[] swears = new string[]
            //     {"Cum", "Penis", "Fuk", "Fuc", "Fuq", "Cuc", "Cuk", "Cuq", "Nig", "Niga", "Nigar", "Niger", "Pusy", "Pusi"};
            for (int i = 0; i < r; i++)
            {
                if (i == 0)
                    name += (char)Random.Range('A','Z'+1);
                else if (i % 2 == 0)
                    name += con[Random.Range(0,con.Length)];
                else
                    name += vow[Random.Range(0,vow.Length)];
            }
            // for (int i = 0; i < swears.Length; i++)
            // {
            //     if (name == swears[i]) name = "[REDACTED]";
            // }
            loca.placeName = name;
        }
        else
        {
            loca.placeName = string.Format("{0:n2}°{1} {2:n2}°{3}",Mathf.Abs(loca.longLatCoord.y), loca.longLatCoord.y > 0 ? "N" : "S", Mathf.Abs(loca.longLatCoord.x), loca.longLatCoord.x > 0 ? "E" : "W");
        }
        // loca.placeName = string.Format("{0}\n{1}\n{2}", loca.longLatCoord.ToString(), loca.xyzCoord.ToString(), loca.uvqCoord.ToString());
        pin.gameObject.name = loca.placeName;

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

    void LateUpdate() {
        if (progMax > 0) progressBar.value = ((float)progCurrent / (float)progMax);
        progressText.text = string.Format("{0:p1}", (float)progCurrent / progMax);
    }

    public void SaveMap()
    {
        FileIO.SaveFile();
    }

    public void LoadedPlanet(Planet p)
    {
        planet = p;
    }

    public void LoadMap()
    {
        isLocationAllowed = false;
        foreach (GameObject pin in pins)
        {
            Destroy(pin);
            pins.Remove(pin);
        }

        GameRam.FromSaveData(FileIO.LoadFile());
        GenerateMapButton();
    }

    public void ToggleMetric(TextMeshProUGUI text)
    {
        if (useMiles)
        {
            useMiles = false;
            text.text = "Metric Units";
            OnValidate();
        }
        else
        {
            useMiles = true;
            text.text = "Imperial Units";
            OnValidate();
        }
    }

    void OnValidate()
    {
        float globeRadius = 6378.1f * planet.globeScale;
        float fullMapWidth = globeRadius * 2 * Mathf.PI;
        float mapWidthMiles = fullMapWidth * 0.621371f;
        mapSizeLabel.text = string.Format("Map Width: {0} {2} (across center only)\nMap Height: {1} {2}", useMiles ? mapWidthMiles : fullMapWidth, useMiles ? mapWidthMiles / 2 : fullMapWidth / 2, useMiles ? "mi": "km");

        if (autoUpdate) {
            GenerateMapButton();
        }
    }
}