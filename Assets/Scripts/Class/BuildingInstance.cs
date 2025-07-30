using UnityEngine;

public class BuildingInstance : MonoBehaviour
{
    public string buildingName;
    public Vector2Int size = new(1, 1);

    [Header("Production (optionnel)")]
    public bool hasProduction;
    public ProductionData production;

    [HideInInspector] public Vector3Int cellPosition;
    [HideInInspector] public string lastCollected;

    void Start()
    {
        // Initialisation du temps de dernière collecte si vide
        if (hasProduction && string.IsNullOrEmpty(lastCollected))
        {
            lastCollected = System.DateTime.UtcNow.ToString("o");
        }
    }
}