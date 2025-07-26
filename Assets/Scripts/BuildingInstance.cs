using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingInstance : MonoBehaviour
{
    public string buildingName;       // Nom du bâtiment
    public Vector3Int cellPosition;   // Position actuelle sur la grille
    public BuildingData data;         // Données du bâtiment

    // Position validée précédemment
    public Vector3Int lastValidCell;

    void Start()
    {
        lastValidCell = cellPosition; // Initialisation
    }

    /// Sauvegarder la position actuelle comme position validée.
    public void SaveCurrentPosition()
    {
        lastValidCell = cellPosition;
    }

    /// Restaurer la dernière position validée.
    public void RestoreLastPosition(Tilemap floor)
    {
        cellPosition = lastValidCell;
        transform.position = floor.GetCellCenterWorld(lastValidCell);
    }
}