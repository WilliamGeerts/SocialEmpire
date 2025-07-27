using System.Collections.Generic;

[System.Serializable]
public class SavedData
{
    public List<ResourceData> inventory = new();
    public List<PlacedBuildingData> buildings = new();
}