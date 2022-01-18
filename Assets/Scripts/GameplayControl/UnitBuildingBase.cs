using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UnitBuildingHousing))]
public class UnitBuildingBase : MonoBehaviour
{
    public Material[] playerColors;
    [Range(0,7)]
    public int player;
    public Renderer[] flags;
    public List<UnitCitizen> homelessCitizens = new List<UnitCitizen>();
    public List<UnitBuildingHousing> allHousing = new List<UnitBuildingHousing>();

    public int health, maxHealth;

    void Start()
    {
        allHousing.Add(gameObject.GetComponent<UnitBuildingHousing>());
    }

    public void ChangeOwner(int newPlayer)
    {
        foreach (Renderer flag in flags)
        {
            flag.material = playerColors[newPlayer];
        }
        gameObject.transform.SetParent(GameObject.Find("Player Units (" + newPlayer + ")").transform);
        player = newPlayer;
    }

    public void AddHomeless(UnitCitizen citizen)
    {
        foreach (UnitBuildingHousing house in allHousing)
        {
            if (house.currentTenants.Count < house.maxTenants)
            {
                house.currentTenants.Add(citizen);
                citizen.myHome = house;
                return;
            }
        }

        //If housing cannot be found:
        homelessCitizens.Add(citizen);
    }

    public void AddHousing(UnitBuildingHousing house)
    {
        int newlyHomed = 0;
        allHousing.Add(house);
        foreach (UnitCitizen vagabond in homelessCitizens)
        {
            house.currentTenants.Add(vagabond);
            homelessCitizens.Remove(vagabond);
            newlyHomed ++;
            if (house.currentTenants.Count >= house.maxTenants)
            {
                Debug.LogFormat("{0} citizens housed, {1} homeless remaining.", newlyHomed, homelessCitizens.Count);
                return;
            }
        }
    }
}
