using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterListController
{
    private List<CharacterData> m_AllCharacters;

    // UXML template for list entries
    private VisualTreeAsset m_ListEntryTemplate;

    // UI element references
    private ListView m_CharacterList;
    private Label m_CharClassLabel;
    private Label m_CharNameLabel;
    private VisualElement m_CharPortrait;
    private Button m_SelectCharButton;

    public void InitializeCharacterList(VisualElement root, VisualTreeAsset listElementTemplate)
    {
        // Enumerate all characters
        EnumerateAllCharacters();

        // Store a reference to the template for the list entries
        m_ListEntryTemplate = listElementTemplate;

        // Store a reference to the character list element
        m_CharacterList = root.Q<ListView>("CharacterList");

        // Store references to the selected character info elements
        m_CharClassLabel = root.Q<Label>("CharacterClass");
        m_CharNameLabel = root.Q<Label>("CharacterName");
        m_CharPortrait = root.Q<VisualElement>("CharacterPortrait");

        // Store a reference to the select button
        m_SelectCharButton = root.Q<Button>("SelectCharButton");

        FillCharacterList();
        
        // Register to get a callback when an item is selected
        m_CharacterList.onSelectionChange += OnCharacterSelected;
    }

    private void EnumerateAllCharacters()
    {
        m_AllCharacters = new List<CharacterData>();
        m_AllCharacters.AddRange(Resources.LoadAll<CharacterData>("Characters"));
    }
    
    private void FillCharacterList()
    {
        // Set up a make item function for a list entry
        m_CharacterList.makeItem = () =>
        {
            // Instantiate the UXML template for the entry
            var newListEntry = m_ListEntryTemplate.Instantiate();

            // Instantiate a controller for the data
            var newListEntryLogic = new CharacterListEntryController();

            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;
            
            // Initialize the controller script
            newListEntryLogic.SetVisualElement(newListEntry);

            // Return the root of the instantiated visual tree
            return newListEntry;
        };

        // Set up bind function for a specific list entry
        m_CharacterList.bindItem = (item, index) =>
        {
            (item.userData as CharacterListEntryController).SetCharacterData(m_AllCharacters[index]);
        };
        

        // Set a fixed item height
        m_CharacterList.fixedItemHeight = 45;
        // For Unity versions earlier than 2021.2 use this:
        //m_CharacterList.itemHeight = 45; 

        // Set the actual item's source list/array
        m_CharacterList.itemsSource = m_AllCharacters;
    }

    private void OnCharacterSelected(IEnumerable<object> selectedItems)
    {
        // Get the currently selected item directly from the ListView
        var selectedCharacter = m_CharacterList.selectedItem as CharacterData;

        // Handle none-selection (Escape to deselect everything)
        if (selectedCharacter == null)
        {
            // Clear
            m_CharClassLabel.text = "";
            m_CharNameLabel.text = "";
            m_CharPortrait.style.backgroundImage = null;

            // Disable the select button
            m_SelectCharButton.SetEnabled(false);

            return;
        }

        // Fill in character details
        m_CharClassLabel.text = selectedCharacter.m_Class.ToString();
        m_CharNameLabel.text = selectedCharacter.m_CharacterName;
        m_CharPortrait.style.backgroundImage = new StyleBackground(selectedCharacter.m_PortraitImage);

        // Enable the select button
        m_SelectCharButton.SetEnabled(true);
    }
}