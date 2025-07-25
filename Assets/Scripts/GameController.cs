using UnityEngine;

public class GameController : MonoBehaviour
{
    public BuildingPlacer buildingPlacer;

    void Start()
    {
        LoadGame();
    }

    void LoadGame()
    {
        SavedData savedData = SavedController.Load();

        foreach (PlacedBuildingData placed in savedData.buildings)
        {
            BuildingData data = buildingPlacer.availableBuildings.Find(b => b.name == placed.buildingName);

            if (data != null && data.prefab != null)
            {
                Vector3 finalPos = buildingPlacer.floor.GetCellCenterWorld(placed.cellPosition);
                finalPos.z = 0f;

                GameObject building = Instantiate(data.prefab, finalPos, Quaternion.identity);

                BuildingInstance instance = building.GetComponent<BuildingInstance>();
                if (instance != null)
                {
                    instance.buildingName = data.name;
                    instance.cellPosition = placed.cellPosition;
                    instance.data = data; // Important pour accéder à size
                }

                // Enregistrer les cellules occupées à partir de la cellule d’origine
                buildingPlacer.RegisterBuildingCells(placed.cellPosition, data.size);
            }
            else
            {
                Debug.LogWarning($"Impossible de trouver le prefab pour '{placed.buildingName}'");
            }
        }

        Debug.Log($"{savedData.buildings.Count} bâtiment(s) chargé(s).");
    }
}