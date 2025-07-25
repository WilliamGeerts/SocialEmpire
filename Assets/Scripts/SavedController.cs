using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public static class SavedController
{
    private static string saveFile => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(List<GameObject> placedBuildings, Tilemap tilemap)
    {
        SavedData data = new SavedData();

        foreach (var building in placedBuildings)
        {
            var buildingComponent = building.GetComponent<BuildingInstance>();
            if (buildingComponent != null)
            {
                Vector3Int cellPos = tilemap.WorldToCell(building.transform.position);

                data.buildings.Add(new PlacedBuildingData
                {
                    buildingName = buildingComponent.buildingName,
                    cellPosition = cellPos
                });
            }
            else
            {
                Debug.LogWarning("Un bâtiment n'a pas de BuildingInstance !");
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFile, json);
        Debug.Log("Sauvegarde effectuée : " + saveFile);
    }

    public static SavedData Load()
    {
        if (!File.Exists(saveFile))
        {
            Debug.LogWarning("⚠️ Aucune sauvegarde trouvée. Nouveau fichier créé.");
            return new SavedData(); // sauvegarde vide
        }

        string json = File.ReadAllText(saveFile);
        SavedData data = JsonUtility.FromJson<SavedData>(json);
        return data;
    }
}