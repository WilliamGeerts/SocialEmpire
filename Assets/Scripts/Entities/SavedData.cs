using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedData
{
    public List<ResourceData> inventory = new();
    public List<PlacedBuildingData> buildings = new();
}
