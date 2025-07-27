using UnityEngine;
using System.Collections.Generic;

public static class PlayerInventory
{
    private static List<ResourceData> resources = new();

    public static void LoadInventory(List<ResourceData> saved)
    {
        resources = saved ?? new List<ResourceData>();
    }

    public static List<ResourceData> GetInventory()
    {
        return resources;
    }

    public static void AddResource(string type, int amount)
    {
        var resource = resources.Find(r => r.resourceType == type);
        if (resource == null)
        {
            resource = new ResourceData { resourceType = type, amount = 0 };
            resources.Add(resource);
        }
        resource.amount += amount;
        Debug.Log($"[Inventory] +{amount} {type} (Total: {resource.amount})");
    }

    public static int GetResourceAmount(string type)
    {
        var resource = resources.Find(r => r.resourceType == type);
        return resource != null ? resource.amount : 0;
    }
}