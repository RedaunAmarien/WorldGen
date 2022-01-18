using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBuildingHousing : MonoBehaviour
{
    public List<UnitCitizen> currentTenants = new List<UnitCitizen>();
    public int maxTenants;
    
    public void OnHouseDestroyed()
    {
        foreach (UnitCitizen tenant in currentTenants)
        {
            tenant.SetHomeless();
        }
    }
}
