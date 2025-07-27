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

    private GameObject collectButton;

    void Start()
    {
        // Initialisation du temps de dernière collecte si vide
        if (string.IsNullOrEmpty(lastCollected))
        {
            lastCollected = System.DateTime.UtcNow.ToString("o");
        }

        // Bouton de collecte
        if (collectButton == null)
        {
            Transform btnTransform = transform.Find("WorldSpace/CollectButton");
            if (btnTransform != null)
            {
                collectButton = btnTransform.gameObject;
            }
        }

        if (collectButton != null)
        {
            collectButton.SetActive(false);
            var handler = collectButton.GetComponent<CollectButtonHandler>();
            handler?.Initialize(this);
        }
    }

    // Afficher ou cacher le bouton de collecte
    public void SetCollectButtonVisible(bool visible)
    {
        collectButton?.SetActive(visible);
    }
}