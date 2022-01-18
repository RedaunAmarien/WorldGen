using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public int yourIndex = 0;
    public Button menuButton, objtvButton, treatyButton;
    public TextMeshProUGUI selectedName, selectedHealth, selectedFlavor;
    public List<GameObject> selectedUnits = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedUnits.Count == 0)
        {
            selectedName.text = "";
            selectedHealth.text = "";
            selectedFlavor.text = "";
        }
        else if (selectedUnits.Count == 1) 
        {
            if (selectedUnits[0].GetComponentInParent<UnitCitizen>() != null)
            {
                UnitCitizen unit = selectedUnits[0].GetComponentInParent<UnitCitizen>();
                selectedName.text = unit.unitName;
                selectedHealth.text = unit.health + "/" + unit.maxHealth + " HP";
                selectedFlavor.text = "Player " + (unit.owner+1);
            }
            else if (selectedUnits[0].GetComponentInParent<UnitBuildingBase>() != null)
            {
                UnitBuildingBase unit = selectedUnits[0].GetComponentInParent<UnitBuildingBase>();
                selectedName.text = "Player " + (unit.player+1) + "'s Base";
                selectedHealth.text = unit.health + "/" + unit.maxHealth + " HP";
                selectedFlavor.text = "";
            }
            else
            {
                selectedName.text = selectedUnits[0].name;
                selectedHealth.text = "";
                selectedFlavor.text = "";
            }
        }
        else
        {

        }
    }
}
