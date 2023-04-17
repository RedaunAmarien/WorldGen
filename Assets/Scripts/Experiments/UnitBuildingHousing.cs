using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBuildingHousing : MonoBehaviour
{
    public List<Citizen> currentTenants = new List<Citizen>();
    public int maxTenants;
    
    public void OnHouseDestroyed()
    {
        foreach (Citizen tenant in currentTenants)
        {
            //tenant.SetHomeless();
        }
    }
}
