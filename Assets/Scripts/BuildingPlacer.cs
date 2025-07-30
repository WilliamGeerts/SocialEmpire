using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Bâtiments disponibles")]
    public List<GameObject> availableBuildingPrefabs = new();

    [Header("Tilemaps")]
    public Tilemap floor;
    public Tilemap noBuilds;

    [Header("UI")]
    public ShopController shopController;

    [Header("UI Placement Auto")]
    [SerializeField] private GameObject placementUIPrefab;
    private GameObject currentPlacementUI;

    [Header("Indicators")]
    public GameObject tileIndicator;

    private GameObject currentGhost;
    private BuildingInstance currentBuildingInstance;
    private bool isPlacing = false;
    private bool isRepositioning = false;
    private Vector3Int initialCellPosition;

    private readonly List<GameObject> currentIndicators = new();
    private readonly HashSet<Vector3Int> occupiedCells = new();

    #region Unity Methods
    private void Update()
    {
        if (isPlacing && currentGhost != null && currentBuildingInstance != null)
        {
            Vector2Int size = currentBuildingInstance.size;
            Vector3Int originCell = floor.WorldToCell(currentGhost.transform.position);

            Vector3Int centerCell = new Vector3Int(
                originCell.x + (size.x - 1) / 2,
                originCell.y + (size.y - 1) / 2,
                originCell.z
            );

            GeneratePlacementIndicators(centerCell);
        }
    }
    #endregion

    #region Placement Start / Cancel / Validate
    public void StartPlacingBuildingByName(string buildingName)
    {
        var prefab = availableBuildingPrefabs.Find(b => b.name == buildingName);
        if (prefab != null)
        {
            StartPlacingBuilding(prefab);
        }
        else
        {
            Debug.LogError($"[BuildingPlacer] Building '{buildingName}' not found.");
        }
    }

    public void StartPlacingBuilding(GameObject prefab)
    {
        if (isPlacing && currentGhost != null)
        {
            CancelPlacement();
        }

        shopController.ToggleShop();

        if (currentGhost != null)
        {
            Destroy(currentGhost);
        }

        ClearIndicators();

        // Récupérer les données depuis le prefab
        currentBuildingInstance = prefab.GetComponent<BuildingInstance>();
        if (currentBuildingInstance == null)
        {
            Debug.LogError($"[BuildingPlacer] Le prefab '{prefab.name}' n'a pas de BuildingInstance.");
            return;
        }

        Vector3 screenCenterWorld = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        screenCenterWorld.z = 0;

        Vector3Int centerCell = floor.WorldToCell(screenCenterWorld);
        Vector2Int size = currentBuildingInstance.size;

        Vector3Int originCell = new(
            centerCell.x - (size.x - 1) / 2,
            centerCell.y - (size.y - 1) / 2,
            centerCell.z
        );

        Vector3 originCellWorldPos = floor.GetCellCenterWorld(originCell);

        currentGhost = Instantiate(prefab, originCellWorldPos, Quaternion.identity);
        currentBuildingInstance = currentGhost.GetComponent<BuildingInstance>();

        SetGhostVisual(currentGhost, true);
        isPlacing = true;
        GeneratePlacementIndicators(centerCell);
    }

    public void ValidatePlacement()
    {
        if (!isPlacing || currentGhost == null || currentBuildingInstance == null)
            return;

        Vector3Int centerCell = floor.WorldToCell(currentGhost.transform.position);
        Vector2Int size = currentBuildingInstance.size;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int checkCell = new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z);
                if (!IsCellValid(checkCell))
                {
                    Debug.LogWarning("[ValidatePlacement] Placement invalide (cellule bloquée)");
                    return;
                }
            }
        }

        SetGhostVisual(currentGhost, false);
        currentGhost.tag = "Building";
        currentBuildingInstance.cellPosition = centerCell;
        RegisterBuildingCells(centerCell, currentBuildingInstance.size);

        SaveCurrentBuildings();
        EndPlacement(false);
    }

    public void CancelPlacement()
    {
        if (!isPlacing) return;

        if (currentPlacementUI != null)
        {
            Destroy(currentPlacementUI);
            currentPlacementUI = null;
        }

        if (isRepositioning)
        {
            // Replacer le bâtiment à sa position initiale
            currentGhost.transform.position = floor.GetCellCenterWorld(initialCellPosition);
            currentBuildingInstance.cellPosition = initialCellPosition;

            // Réenregistrer les cellules
            RegisterBuildingCells(initialCellPosition, currentBuildingInstance.size);

            // Fin du mode repositionnement
            SetGhostVisual(currentGhost, false);
            isRepositioning = false;
            isPlacing = false;
            ClearIndicators();
            currentGhost = null;
            currentBuildingInstance = null;
        }
        else
        {
            // Cas d'un nouveau bâtiment fantôme
            EndPlacement();
        }
    }

    private void EndPlacement(bool destroyGhost = true)
    {
        if (currentPlacementUI != null)
        {
            Destroy(currentPlacementUI);
            currentPlacementUI = null;
        }

        if (destroyGhost && currentGhost != null)
        {
            Destroy(currentGhost);
        }

        ClearIndicators();
        currentGhost = null;
        currentBuildingInstance = null;
        isPlacing = false;
        isRepositioning = false;
    }
    #endregion

    #region Visuals
    public void SetGhostVisual(GameObject ghost, bool ghostMode)
    {
        // Correction du bug : on récupère l'instance si elle est manquante
        if (ghostMode && currentBuildingInstance == null)
        {
            currentBuildingInstance = ghost.GetComponent<BuildingInstance>();
            Debug.Log($"[SetGhostVisual] currentBuildingInstance récupéré depuis le ghost : {currentBuildingInstance}");
        }

        SpriteRenderer renderer = ghost.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color color = renderer.color;
            color.a = ghostMode ? 0.65f : 1f;
            renderer.color = color;
            renderer.sortingLayerName = ghostMode ? "BuildingGhost" : "Floor";
        }

        ghost.GetComponent<DragToGrid>().enabled = ghostMode;
        ghost.GetComponent<BuildingLongPress>().enabled = !ghostMode;

        // Instanciation de l’UI
        if (ghostMode)
        {
            if (placementUIPrefab != null)
            {
                if (currentPlacementUI != null)
                    Destroy(currentPlacementUI);

                currentPlacementUI = Instantiate(placementUIPrefab);
                var controller = currentPlacementUI.GetComponentInChildren<PlacementController>();

                if (controller != null && currentBuildingInstance != null)
                {
                    controller.Init(
                        ghost.transform,
                        currentBuildingInstance,
                        onValidate: ValidatePlacement,
                        onCancel: CancelPlacement
                    );
                }
                else
                {
                    Debug.LogError("[SetGhostVisual] Impossible d'initialiser PlacementController — controller ou currentBuildingInstance est null !");
                }
            }
        }
        else
        {
            if (currentPlacementUI != null)
            {
                Destroy(currentPlacementUI);
            }
        }
    }
    #endregion

    #region Indicators
    private void GeneratePlacementIndicators(Vector3Int centerCell)
    {
        ClearIndicators();

        Vector2Int size = currentBuildingInstance.size;
        Vector3Int originCell = new Vector3Int(
            centerCell.x - (size.x - 1) / 2,
            centerCell.y - (size.y - 1) / 2,
            centerCell.z
        );

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(originCell.x + x, originCell.y + y, originCell.z);
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
    #endregion

    #region Cells Management
    private bool IsCellValid(Vector3Int cell)
    {
        if (occupiedCells.Contains(cell))
            return false;

        if (!floor.HasTile(cell))
            return false;

        Vector3Int aboveCell = new Vector3Int(cell.x, cell.y, cell.z + 1);
        if (floor.HasTile(aboveCell))
            return false;

        if (noBuilds.HasTile(aboveCell))
            return false;

        return true;
    }

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
    #endregion

    #region Placement Mode
    public void EnterPlacementModeFor(GameObject building)
    {
        if (isPlacing && currentGhost != null && currentGhost != building)
        {
            CancelPlacement();
        }

        currentGhost = building;
        currentBuildingInstance = building.GetComponent<BuildingInstance>();
        if (currentBuildingInstance == null)
        {
            Debug.LogError("[BuildingPlacer] EnterPlacementModeFor appelé sur un GameObject sans BuildingInstance !");
            return;
        }

        isPlacing = true;
        isRepositioning = true;

        // Libère les anciennes cellules pour pouvoir repositionner le bâtiment
        initialCellPosition = currentBuildingInstance.cellPosition;
        UnregisterBuildingCells(currentBuildingInstance.cellPosition, currentBuildingInstance.size);

        Vector3Int centerCell = floor.WorldToCell(building.transform.position);
        GeneratePlacementIndicators(centerCell);

        // Affiche l'UI de placement
        if (placementUIPrefab != null)
        {
            if (currentPlacementUI != null) Destroy(currentPlacementUI);

            currentPlacementUI = Instantiate(placementUIPrefab);
            var placementController = currentPlacementUI.GetComponent<PlacementController>();
            if (placementController != null)
            {
                placementController.Init(
                    building.transform,
                    currentBuildingInstance,
                    onValidate: ValidatePlacement,
                    onCancel: CancelPlacement
                );
            }
            else
            {
                Debug.LogError("[BuildingPlacer] Le prefab UI ne contient pas de PlacementController !");
            }
        }
    }

    public void UnregisterBuildingCells(Vector3Int originCell, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int cell = new Vector3Int(originCell.x + x, originCell.y + y, originCell.z);
                occupiedCells.Remove(cell);
            }
        }
    }
    #endregion

    #region Save
    private void SaveCurrentBuildings()
    {
        GameObject[] placedBuildings = GameObject.FindGameObjectsWithTag("Building");
        SavedController.Save(new List<GameObject>(placedBuildings), floor);
    }
    #endregion
}