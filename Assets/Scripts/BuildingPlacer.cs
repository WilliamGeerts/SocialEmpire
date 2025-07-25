using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Bâtiments disponibles")]
    public List<BuildingData> availableBuildings = new();

    [Header("Grille et Tilemaps")]
    public Grid grid;
    public Tilemap floor;

    [Header("UI")]
    public CameraController cameraController;
    public ShopController shopController;

    [Header("Indicators")]
    public GameObject tileIndicator;

    private GameObject currentGhost;
    private BuildingData currentBuildingData;
    private bool isPlacing = false;
    private Vector3Int lastCell = Vector3Int.zero;

    // Liste des cases d'indications (placement)
    private List<GameObject> currentIndicators = new();

    // Liste des cases occupées
    private HashSet<Vector3Int> occupiedCells = new();

    public void Update()
    {
        if (isPlacing && currentGhost != null)
        {
            Vector2Int size = currentBuildingData.size;

            Vector3Int originCell = floor.WorldToCell(currentGhost.transform.position);

            Vector3Int centerCell = new Vector3Int(
                originCell.x + (size.x - 1) / 2,
                originCell.y + (size.y - 1) / 2,
                originCell.z
            );

            GeneratePlacementIndicators(centerCell);
        }
    }

    public void StartPlacingBuildingByName(string buildingName)
    {
        var data = availableBuildings.Find(b => b.name == buildingName);
        if (data != null)
        {
            StartPlacingBuilding(data);
        }
        else
        {
            Debug.LogError($"[BuildingPlacer] Building '{buildingName}' not found in availableBuildings");
        }
    }

    public void StartPlacingBuilding(BuildingData data)
    {
        shopController.ToggleShop();

        if (currentGhost != null)
        {
            Debug.LogWarning("[StartPlacingBuilding] État invalide : placement annulé.");
            Destroy(currentGhost);
        }

        // Nettoyer les anciennes cellules d'indication
        ClearIndicators();

        currentBuildingData = data;

        // Récupérer la position du centre de l'écran
        Vector3 screenCenterWorld = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        screenCenterWorld.z = 0;

        // Convertir la position en cellule
        Vector3Int centerCell = floor.WorldToCell(screenCenterWorld);
        Debug.LogWarning($"[StartPlacingBuilding] Cellule au centre: {centerCell}.");

        Vector2Int size = currentBuildingData.size;
        Vector3Int originCell = new Vector3Int(
            centerCell.x - (size.x - 1) / 2,
            centerCell.y - (size.y - 1) / 2,
            centerCell.z
        );

        // Convertir la cellule en position
        Vector3 originCellWorldPos = floor.GetCellCenterWorld(originCell);

        currentGhost = Instantiate(data.prefab, originCellWorldPos, Quaternion.identity);
        Debug.LogWarning($"[StartPlacingBuilding] Position initial du batiment: {originCellWorldPos}.");

        SetGhostVisual(currentGhost, true);
        isPlacing = true;

        if (cameraController != null)
            cameraController.enabled = false;

        // Les indicateurs sont alignés sur la cellule d'origine (coin bas gauche)
        GeneratePlacementIndicators(centerCell);
    }

    public void ValidatePlacement()
    {
        if (!isPlacing || currentGhost == null || currentBuildingData == null)
        {
            Debug.LogWarning("[ValidatePlacement] État invalide : placement annulé.");
            return;
        }

        Vector3Int centerCell = floor.WorldToCell(currentGhost.transform.position);

        Vector2Int size = currentBuildingData.size;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int checkCell = new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z);

                if (!IsCellValid(checkCell))
                {
                    return;
                }
            }
        }

        SetGhostVisual(currentGhost, false);
        currentGhost.tag = "Building";

        var buildingInstance = currentGhost.GetComponent<BuildingInstance>();
        if (buildingInstance != null)
        {
            buildingInstance.cellPosition = centerCell;
        }
        else
        {
            Debug.LogWarning("[ValidatePlacement] Aucun composant BuildingInstance trouvé sur le bâtiment");
        }

        RegisterBuildingCells(centerCell, size);
        SaveCurrentBuildings();
        EndPlacement(false);
    }

    public void CancelPlacement()
    {
        EndPlacement();
    }

    private void EndPlacement(bool destroyGhost = true)
    {
        if (destroyGhost && currentGhost != null)
        {
            Destroy(currentGhost);
        }

        ClearIndicators();

        currentGhost = null;
        currentBuildingData = null;
        isPlacing = false;

        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }


    // Déterminer si le bâtiment est en mode fantôme (Placement)
    private void SetGhostVisual(GameObject ghost, bool ghostMode)
    {
        SpriteRenderer renderer = ghost.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color color = renderer.color;
            color.a = ghostMode ? 0.5f : 1f;
            renderer.color = color;
        }
        else
        {
            Debug.LogWarning("[SetGhostVisual] SpriteRenderer non trouvé sur le prefab fantôme");
        }

        GameObject ghostButtonsPanel = ghost.transform.Find("WorldSpace/PlacementButtons")?.gameObject;
        if (ghostButtonsPanel != null)
        {
            ghostButtonsPanel.SetActive(ghostMode);

            if (ghostMode)
            {
                var validateBtn = ghostButtonsPanel.transform.Find("ValidateButton").GetComponent<Button>();
                validateBtn.onClick.RemoveAllListeners();
                validateBtn.onClick.AddListener(ValidatePlacement);

                var cancelBtn = ghostButtonsPanel.transform.Find("CancelButton").GetComponent<Button>();
                cancelBtn.onClick.RemoveAllListeners();
                cancelBtn.onClick.AddListener(CancelPlacement);
            }
        }
        else
        {
            Debug.LogWarning("[SetGhostVisual] Panel des boutons non trouvé dans le prefab fantôme");
        }

        // Toggle le script permettant de déplacer un bâtiment
        ghost.GetComponent<DragToGrid>().enabled = ghostMode;
    }

    private void GeneratePlacementIndicators(Vector3Int centerCell)
    {
        // Debug.LogWarning($"[GeneratePlacementIndicators] Test: {centerCell}");
        
        if (centerCell == lastCell && currentIndicators.Count > 0) 
        {
            return;
        }

        lastCell = centerCell;

        ClearIndicators();

        Vector2Int size = currentBuildingData.size;
        Vector3Int originCell = new Vector3Int(
            centerCell.x - (size.x - 1) / 2, 
            centerCell.y - (size.y - 1) / 2, 
            centerCell.z
        );

        // Génèrer un indicateur pour chaque cellule que le bâtiment occupera
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(
                    originCell.x + x,
                    originCell.y + y, 
                    originCell.z
                );
                Vector3 worldPos = floor.GetCellCenterWorld(cellPos);

                GameObject indicator = Instantiate(tileIndicator, worldPos, Quaternion.Euler(63f, 0f, 0f));

                bool isValid = IsCellValid(cellPos);
                var renderer = indicator.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    Color color = isValid ? Color.green : Color.red;
                    color.a = 0.5f;
                    renderer.color = color;
                }

                currentIndicators.Add(indicator);
            }
        }
    }

    private bool IsCellValid(Vector3Int cell)
    {
        // Si la cellule est déjà occupée 
        if (occupiedCells.Contains(cell)) 
        {
            return false;
        }

        // Si la cellule n'est pas de type "Floor"
        if (!floor.HasTile(cell)) 
        {
            return false;
        }

        Vector3Int obstacleCell = new Vector3Int(cell.x, cell.y, 1);

        return true;
    }

    private void ClearIndicators()
    {
        foreach (var indicator in currentIndicators)
        {
            if (indicator != null) 
            { 
                Destroy(indicator);
            }
        }

        currentIndicators.Clear();
    }   

    // Déterminer les cellules occupées par un bâtiment
    public void RegisterBuildingCells(Vector3Int originCell, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int cell = new Vector3Int(originCell.x + x, originCell.y + y, originCell.z);
                occupiedCells.Add(cell);
            }
        }
    }

    // Enregistrer les bâtiments dans le fichier JSON
    private void SaveCurrentBuildings()
    {
        GameObject[] placedBuildings = GameObject.FindGameObjectsWithTag("Building");
        List<GameObject> buildingList = new List<GameObject>(placedBuildings);
        SavedController.Save(buildingList, floor);
    }
}