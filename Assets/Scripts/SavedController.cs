using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public static class SavedController
{
    private static string saveFile => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(List<GameObject> placedBuildings, Tilemap floor)
    {
        SavedData data = new SavedData
        {
            buildings = new List<PlacedBuildingData>(),
            inventory = PlayerInventory.GetInventory()
        };

        foreach (var go in placedBuildings)
        {
            BuildingInstance instance = go.GetComponent<BuildingInstance>();
            if (instance != null)
            {
                data.buildings.Add(new PlacedBuildingData
                {
                    buildingName = instance.buildingName,
                    cellPosition = instance.cellPosition,
                    lastCollected = instance.lastCollected
                });
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFile, json);
        Debug.Log($"[SavedController] Sauvegarde effectuée : {saveFile}");
    }

    public static SavedData Load()
    {
        if (!File.Exists(saveFile))
        {
            Debug.LogWarning("Aucune sauvegarde trouvée. Nouveau fichier créé.");
            return new SavedData();
        }

        string json = File.ReadAllText(saveFile);
        SavedData data = JsonUtility.FromJson<SavedData>(json);
        Debug.Log($"[SavedController] Sauvegarde chargée : {saveFile}");
        return data;
    }
}