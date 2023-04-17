using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TileableBase))]
public class WorldResource : MonoBehaviour
{
    public enum ResourceType
    {
        Iron, Copper, Meat, Wood, Stone
    }
    [SerializeField] ResourceType resourceType;
    [Min(0)]
    [SerializeField] int resourceCount = 100;

    public void SetResource(ResourceType resourceType, int resourceCount)
    {
        this.resourceType = resourceType;
        this.resourceCount = resourceCount;
    }

    public ResourceType GetResourceType()
    {
        return resourceType;
    }

    public int GetResourceCount()
    {
        return resourceCount;
    }

    public int Harvest(int amountToTake)
    {
        int returnedAmount = 0;
        for (int i = 0; i < amountToTake; i++)
        {
            resourceCount -= amountToTake;
            returnedAmount = i;
            if (resourceCount == 0)
                break;
        }
        return returnedAmount;
    }

    void Update()
    {
        if (resourceCount == 0)
            Destroy(gameObject);
    }
}
