using UnityEngine;

[System.Serializable]
public class BuildingProduction
{
    public string resourceType;        // ex: "food", "wood"
    public int amountPerCycle = 5;     // ex: 5 unités par cycle
    public float cycleDurationSeconds = 60f; // ex: 1 cycle par minute
    public int storageCapacity = 50;   // Capacité max avant collecte
}

[System.Serializable]
public class BuildingData
{
    public string name;
    public GameObject prefab;
    public Vector2Int size = Vector2Int.one;
    public BuildingProduction production; // Ajout de la production
}