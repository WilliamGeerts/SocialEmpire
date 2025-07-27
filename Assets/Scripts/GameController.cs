using UnityEngine;
using System;
using System.Globalization;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public BuildingPlacer buildingPlacer;

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
        if (building.production == null)
            return;

        int produced = CalculateProducedAmount(building.production);
        if (produced > 0)
        {
            PlayerInventory.AddResource(building.production.resourceType, produced);
            building.production.lastCollected = DateTime.UtcNow.ToString("o");
            building.SetCollectButtonVisible(false);

            // Sauvegarde bâtiments + inventaire
            SavedController.Save(new List<GameObject>(GameObject.FindGameObjectsWithTag("Building")), buildingPlacer.floor);
            Debug.Log($"[CollectResources] {produced} {building.production.resourceType} collecté(s) et ajouté(s) à l'inventaire.");
        }
    }

    void CheckBuildingsProduction()
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");
        Debug.Log($"[CheckBuildingsProduction] {buildings.Length} bâtiment(s) détecté(s).");

        foreach (GameObject go in buildings)
        {
            BuildingInstance instance = go.GetComponent<BuildingInstance>();
            if (instance == null)
            {
                Debug.LogWarning($"[CheckBuildingsProduction] Aucun BuildingInstance sur {go.name}.");
                continue;
            }

            if (instance.production == null)
            {
                Debug.Log($"[CheckBuildingsProduction] {instance.buildingName} ne produit rien (production = null).");
                continue;
            }

            int produced = CalculateProducedAmount(instance.production);
            Debug.Log($"[CheckBuildingsProduction] {instance.buildingName} -> {produced} {instance.production.resourceType} disponible(s).");

            instance.SetCollectButtonVisible(produced > 0);

            // Vérification de l'état du bouton
            if (instance.collectButton != null)
            {
                Debug.Log($"[CheckBuildingsProduction] Bouton collect de {instance.buildingName} -> {(instance.collectButton.activeSelf ? "ACTIF" : "INACTIF")}.");
            }
            else
            {
                Debug.LogWarning($"[CheckBuildingsProduction] Aucun collectButton trouvé sur {instance.buildingName}.");
            }
        }
    }

    void LoadGame()
    {
        SavedData savedData = SavedController.Load();
        PlayerInventory.LoadInventory(savedData.inventory);

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
                    instance.data = data;

                    // Copie la production
                    instance.production = placed.production;

                    if (placed.production != null)
                    {
                        int produced = CalculateProducedAmount(placed.production);
                        Debug.Log($"{placed.buildingName} a produit {produced} {placed.production.resourceType} depuis ta dernière session !");
                    }
                }

                buildingPlacer.RegisterBuildingCells(placed.cellPosition, data.size);
            }
            else
            {
                Debug.LogWarning($"Impossible de trouver le prefab pour '{placed.buildingName}'");
            }
        }

        Debug.Log($"{savedData.buildings.Count} bâtiment(s) chargé(s).");
    }

    int CalculateProducedAmount(ProductionData production)
    {
        if (production == null)
            return 0;

        if (string.IsNullOrEmpty(production.lastCollected))
        {
            Debug.LogWarning($"[CalculateProducedAmount] {production.resourceType} : lastCollected vide. Initialisation (-5s).");
            production.lastCollected = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(5)).ToString("o");
            return 0;
        }

        DateTime lastCollectedTime;
        if (!DateTime.TryParse(production.lastCollected, null, DateTimeStyles.AdjustToUniversal, out lastCollectedTime))
        {
            Debug.LogWarning($"[CalculateProducedAmount] {production.resourceType} : Parsing impossible pour lastCollected '{production.lastCollected}'. Réinitialisation.");
            production.lastCollected = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(5)).ToString("o");
            return 0;
        }

        // Comparaison UTC
        DateTime now = DateTime.UtcNow;
        if (lastCollectedTime > now)
        {
            Debug.LogWarning($"[CalculateProducedAmount] {production.resourceType} : lastCollected est dans le futur ({lastCollectedTime}). Correction.");
            // On corrige UNE SEULE FOIS en retirant 5s
            production.lastCollected = now.Subtract(TimeSpan.FromSeconds(5)).ToString("o");
            return 0;
        }

        TimeSpan elapsed = now - lastCollectedTime;
        int cycles = (int)(elapsed.TotalSeconds / production.cycleDurationSeconds);
        int totalProduced = cycles * production.amountPerCycle;

        Debug.Log($"[CalculateProducedAmount] {production.resourceType} : {elapsed.TotalSeconds:F1}s écoulées, {cycles} cycles -> {totalProduced} produits (capacité {production.storageCapacity}).");

        return Mathf.Min(totalProduced, production.storageCapacity);
    }
}