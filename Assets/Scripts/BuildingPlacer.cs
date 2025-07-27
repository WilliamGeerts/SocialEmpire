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
    public Tilemap noBuilds;

    [Header("UI")]
    public ShopController shopController;

    [Header("Indicators")]
    public GameObject tileIndicator;

    private GameObject currentGhost;
    private BuildingData currentBuildingData;
    private bool isPlacing = false;
    private Vector3Int lastCell = Vector3Int.zero;
    private Vector3Int initialCellPosition;

    // Indicateurs de placement
    private readonly List<GameObject> currentIndicators = new();

    // Cellules occupées
    private readonly HashSet<Vector3Int> occupiedCells = new();

    #region Unity Methods
    private void Update()
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
    #endregion

    #region Placement Start / Cancel / Validate
    public void StartPlacingBuildingByName(string buildingName)
    {
        var data = availableBuildings.Find(b => b.name == buildingName);
        if (data != null)
        {
            StartPlacingBuilding(data);
        }
        else
        {
            Debug.LogError($"[BuildingPlacer] Building '{buildingName}' not found.");
        }
    }

    public void StartPlacingBuilding(BuildingData data)
    {
        // Annuler le bâtiment précédent s'il est encore en fantôme
        if (isPlacing && currentGhost != null)
        {
            CancelPlacement();
        }

        shopController.ToggleShop();

        if (currentGhost != null)
        {
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

        Vector2Int size = currentBuildingData.size;
        Vector3Int originCell = new Vector3Int(
            centerCell.x - (size.x - 1) / 2,
            centerCell.y - (size.y - 1) / 2,
            centerCell.z
        );

        // Convertir la cellule en position
        Vector3 originCellWorldPos = floor.GetCellCenterWorld(originCell);

        currentGhost = Instantiate(data.prefab, originCellWorldPos, Quaternion.identity);
        SetGhostVisual(currentGhost, true);
        isPlacing = true;

        // Les indicateurs sont alignés sur la cellule d'origine (coin bas gauche)
        GeneratePlacementIndicators(centerCell);
    }

    public void ValidatePlacement()
    {
        if (!isPlacing || currentGhost == null || currentBuildingData == null)
            return;

        Vector3Int centerCell = floor.WorldToCell(currentGhost.transform.position);
        Vector2Int size = currentBuildingData.size;

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

        var buildingInstance = currentGhost.GetComponent<BuildingInstance>();
        if (buildingInstance != null)
        {
            buildingInstance.cellPosition = centerCell;
            buildingInstance.SaveCurrentPosition();
            RegisterBuildingCells(centerCell, currentBuildingData.size);
        }
        else
        {
            Debug.LogWarning("[ValidatePlacement] Aucun BuildingInstance trouvé sur le bâtiment");
        }

        SaveCurrentBuildings();
        EndPlacement(false);
    }

    public void CancelPlacement()
    {
        if (currentGhost != null)
        {
            if (currentGhost.CompareTag("Building"))
            {
                var instance = currentGhost.GetComponent<BuildingInstance>();
                if (instance != null)
                {
                    instance.cellPosition = initialCellPosition;
                    currentGhost.transform.position = floor.GetCellCenterWorld(initialCellPosition);
                    RegisterBuildingCells(initialCellPosition, instance.data.size);
                }
                SetGhostVisual(currentGhost, false);
            }
            else
            {
                EndPlacement();
                return;
            }
        }
        EndPlacement(false);
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
    }
    #endregion

    #region Visuals
    public void SetGhostVisual(GameObject ghost, bool ghostMode)
    {
        SpriteRenderer renderer = ghost.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color color = renderer.color;
            color.a = ghostMode ? 0.65f : 1f;
            renderer.color = color;
            renderer.sortingLayerName = ghostMode ? "BuildingGhost" : "Floor";
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

        ghost.GetComponent<DragToGrid>().enabled = ghostMode;
        ghost.GetComponent<BuildingLongPress>().enabled = !ghostMode;
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
        currentBuildingData = building.GetComponent<BuildingInstance>().data;
        isPlacing = true;

        var instance = building.GetComponent<BuildingInstance>();
        initialCellPosition = instance.cellPosition;

        UnregisterBuildingCells(instance.cellPosition, instance.data.size);

        Vector3Int centerCell = floor.WorldToCell(building.transform.position);
        GeneratePlacementIndicators(centerCell);
    }
    #endregion

    #region Indicators
    private void GeneratePlacementIndicators(Vector3Int centerCell)
    {
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
                else
                {
                    Debug.LogWarning($"[GeneratePlacementIndicators] Aucun SpriteRenderer trouvé sur l'indicateur {indicator.name}");
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
        {
            return false;
        }

        // Floor (Z = 0)
        if (!floor.HasTile(cell))
        {
            return false;
        }

        // Floor (Z = 1)
        Vector3Int aboveCell = new Vector3Int(cell.x, cell.y, cell.z + 1);
        if (floor.HasTile(aboveCell))
        {
            return false;
        }

        if (noBuilds.HasTile(aboveCell))
        {
            return false;
        }

        return true;
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