using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

public class WorldUI : MonoBehaviour
{
    public WorldManager worldManager;
    UIDocument uiDocument;
    VisualElement root;

    private Button generateWorldButton, regenMapButton, saveWorldButton, loadWorldButton, exportMapButton, generatePinsButton, loadLocationButton, unitTogglebutton;
    public ProgressBar progressBar, mapProgressBar;
    private VisualElement worldMap;
    private VisualElement areaMap;
    private Label areaDescription;
    public UnityEvent loadLocation;

    void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        generateWorldButton = root.Q<Button>("GenerateWorldButton");
        regenMapButton = root.Q<Button>("RegenMapButton");
        saveWorldButton = root.Q<Button>("SaveWorldButton");
        loadWorldButton = root.Q<Button>("LoadWorldButton");
        exportMapButton = root.Q<Button>("ExportMapButton");
        generatePinsButton = root.Q<Button>("GeneratePinsButton");
        loadLocationButton = root.Q<Button>("LoadLocationButton");
        unitTogglebutton = root.Q<Button>("UnitToggleButton");
        worldMap = root.Q<VisualElement>("WorldMap");
        areaMap = root.Q<VisualElement>("AreaMap");
        progressBar = root.Q<ProgressBar>("GenerationProgress");
        mapProgressBar = root.Q<ProgressBar>("MapProgress");
        areaDescription = root.Q<Label>("AreaDescription");

        regenMapButton.SetEnabled(false);

        generateWorldButton.clickable.clicked += worldManager.GenerateMapButton;
        regenMapButton.clickable.clicked += worldManager.RegenMapButton;
        saveWorldButton.clickable.clicked += worldManager.SaveMap;
        loadWorldButton.clickable.clicked += worldManager.LoadMap;
        exportMapButton.clickable.clicked += worldManager.ExportMap;
        generatePinsButton.clickable.clicked += worldManager.GenerateLocationButton;
        loadLocationButton.clickable.clicked += loadLocation.Invoke;
        unitTogglebutton.clickable.clicked += worldManager.ToggleImperial;

    }

    public void SetWorldMap(Texture2D tex)
    {
        worldMap.style.backgroundImage = tex;
        areaDescription.text = string.Format("World Seed: {0}\nStyle: {1}\nFractal: {2}", worldManager.seed, worldManager.noiseType, worldManager.fractalType);
        regenMapButton.SetEnabled(true);
    }

    public void SetAreaMap(Texture2D tex, Locale locale)
    {
        areaMap.style.backgroundImage = tex;
        areaDescription.text = string.Format(
            "{0}\n{1:n2}°{2}, {3:n2}°{4}\nElevation: {5:n2} {6}",
            locale.placeName,
            Mathf.Abs(locale.coordinates.latitude),
            locale.coordinates.latitude < 0 ? "S" : "N",
            Mathf.Abs(locale.coordinates.longitude),
            locale.coordinates.longitude < 0 ? "W" : "E",
            worldManager.imperialUnits ? locale.avgElevation * 3.281f : locale.avgElevation,
            worldManager.imperialUnits ? "ft" : "m"
            );
        regenMapButton.SetEnabled(true);
    }

    void LateUpdate() {
        if (worldManager.progMax > 0)
        {
            progressBar.SetValueWithoutNotify((float)worldManager.progCurrent / (float)worldManager.progMax);
        }
        if (worldManager.mapProgMax > 0)
        {
            mapProgressBar.SetValueWithoutNotify((float)worldManager.mapProgCurrent / (float)worldManager.mapProgMax);
        }
        progressBar.title = string.Format("{0:p1}", (float)worldManager.progCurrent / worldManager.progMax);
        mapProgressBar.title = string.Format("{0:p1}", (float)worldManager.mapProgCurrent / (float)worldManager.mapProgMax);
        if (Mathf.Approximately(worldManager.progCurrent, 0))
        {
            progressBar.title = "0%";
        }
        if (Mathf.Approximately(worldManager.mapProgCurrent, 0))
        {
            mapProgressBar.title = "0%";
        }

        unitTogglebutton.text = worldManager.imperialUnits ? "Units: Imperial" : "Units: Metric";
    }
}
