using UnityEngine;

[System.Serializable]
public class ProductionData
{
    public string resourceType;
    public int amountPerCycle;
    public float cycleDurationSeconds;
    public int storageCapacity;
    public string lastCollected;
}

[System.Serializable]
public class PlacedBuildingData
{
    public string buildingName;
    public Vector3Int cellPosition;

    public ProductionData production;
}