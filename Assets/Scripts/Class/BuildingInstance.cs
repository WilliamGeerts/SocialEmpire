using UnityEngine;
using UnityEngine.UI;

public class BuildingInstance : MonoBehaviour
{
    public string buildingName;
    public Vector3Int cellPosition;
    public BuildingData data;
    public ProductionData production;

    public GameObject collectButton;

    public Vector3Int lastValidCell;

    void Start()
    {
        lastValidCell = cellPosition;

        // Récupère le bouton (si pas déjà assigné dans l’inspector)
        if (collectButton == null)
        {
            Transform btnTransform = transform.Find("WorldSpace/CollectButton");
            if (btnTransform != null)
                collectButton = btnTransform.gameObject;
        }

        if (collectButton != null)
        {
            collectButton.SetActive(false);
            var handler = collectButton.GetComponent<CollectButtonHandler>();
            if (handler != null)
                handler.Initialize(this);
        }
    }

    // Afficher ou cacher le bouton de collecte
    public void SetCollectButtonVisible(bool visible)
    {
        if (collectButton != null)
            collectButton.SetActive(visible);
    }

    public void SaveCurrentPosition()
    {
        lastValidCell = cellPosition;
    }

    public void RestoreLastPosition(UnityEngine.Tilemaps.Tilemap floor)
    {
        cellPosition = lastValidCell;
        transform.position = floor.GetCellCenterWorld(lastValidCell);
    }
}