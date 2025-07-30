using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;

public class GameController : MonoBehaviour
{
    public BuildingPlacer buildingPlacer;
    public ResourceController resourceController;

    [Header("Icones de ressources")]
    public ResourceIconLibrary iconLibrary;

    void Awake()
    {
        BuildingUIController.iconLibrary = iconLibrary;
    }

    void Start()
    {
        LoadGame();
    }

    void Update()
    {
        CheckBuildingsProduction();
    }

    public void CollectResources(BuildingInstance building)
    {
        if (!building.hasProduction)
            return;

        int produced = CalculateProducedAmount(building, building.lastCollected);
        if (produced > 0)
        {
            PlayerInventory.AddResource(building.production.resourceType, produced);
            building.lastCollected = DateTime.UtcNow.ToString("o");

            // Cacher le bouton de collecte après récolte
            BuildingUIController.ShowCollectUI(building.gameObject, false);

            resourceController.UpdateResource(
                building.production.resourceType,
                PlayerInventory.GetResourceAmount(building.production.resourceType)
            );

            SavedController.Save(
                new List<GameObject>(GameObject.FindGameObjectsWithTag("Building")),
                buildingPlacer.floor
            );

            Debug.Log($"[CollectResources] {produced} {building.production.resourceType} collecté(s).");
        }
    }

    void CheckBuildingsProduction()
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");

        foreach (GameObject go in buildings)
        {
            BuildingInstance instance = go.GetComponent<BuildingInstance>();
            if (instance == null)
            {
                Debug.LogWarning($"[CheckBuildingsProduction] Aucun BuildingInstance sur {go.name}.");
                continue;
            }

            if (!instance.hasProduction)
                continue;

            int produced = CalculateProducedAmount(instance, instance.lastCollected);

            // Affiche ou cache le bouton de collecte selon la production
            BuildingUIController.ShowCollectUI(go, produced > 0);
        }
    }

    void LoadGame()
    {
        SavedData savedData = SavedController.Load();
        PlayerInventory.LoadInventory(savedData.inventory);
        resourceController.InitUI(savedData.inventory);

        foreach (PlacedBuildingData placed in savedData.buildings)
        {
            GameObject prefab = buildingPlacer.availableBuildingPrefabs.Find(b => b.name == placed.buildingName);
            if (prefab != null)
            {
                Vector3 finalPos = buildingPlacer.floor.GetCellCenterWorld(placed.cellPosition);
                finalPos.z = 0f;

                GameObject building = Instantiate(prefab, finalPos, Quaternion.identity);
                BuildingInstance instance = building.GetComponent<BuildingInstance>();

                if (instance != null)
                {
                    instance.buildingName = prefab.name;
                    instance.cellPosition = placed.cellPosition;
                    instance.lastCollected = placed.lastCollected ?? DateTime.UtcNow.ToString("o");

                    if (instance.hasProduction)
                    {
                        int produced = CalculateProducedAmount(instance, instance.lastCollected);
                        Debug.Log($"{placed.buildingName} a produit {produced} {instance.production.resourceType} depuis ta dernière session !");
                    }
                }

                // Attache les UIs dynamiquement
                BuildingUIController.AttachPlacementUI(building, buildingPlacer.placementUIPrefab);

                if (instance.hasProduction)
                {
                    BuildingUIController.AttachCollectUI(building, buildingPlacer.collectUIPrefab);
                }

                buildingPlacer.RegisterBuildingCells(placed.cellPosition, instance.size);
            }
            else
            {
                Debug.LogWarning($"Impossible de trouver le prefab pour '{placed.buildingName}'");
            }
        }

        Debug.Log($"{savedData.buildings.Count} bâtiment(s) chargé(s).");
    }

    int CalculateProducedAmount(BuildingInstance instance, string lastCollected)
    {
        if (!instance.hasProduction || instance.production.cycleDurationSeconds <= 0)
        {
            Debug.LogError($"[CalculateProducedAmount] {instance.buildingName} : production désactivée ou cycle invalide.");
            return 0;
        }

        DateTime lastCollectedTime;
        if (string.IsNullOrEmpty(lastCollected) ||
            !DateTime.TryParse(lastCollected, null, DateTimeStyles.AdjustToUniversal, out lastCollectedTime))
        {
            Debug.LogWarning($"[CalculateProducedAmount] {instance.production.resourceType} : lastCollected invalide. Init (-5s).");
            lastCollectedTime = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(5));
        }

        DateTime now = DateTime.UtcNow;
        if (lastCollectedTime > now)
        {
            Debug.LogWarning($"[CalculateProducedAmount] {instance.production.resourceType} : lastCollected dans le futur ({lastCollectedTime}). Correction.");
            lastCollectedTime = now.Subtract(TimeSpan.FromSeconds(5));
        }

        TimeSpan elapsed = now - lastCollectedTime;
        int cycles = (int)(elapsed.TotalSeconds / instance.production.cycleDurationSeconds);
        int totalProduced = cycles * instance.production.amountPerCycle;

        return Mathf.Min(totalProduced, instance.production.storageCapacity);
    }
}