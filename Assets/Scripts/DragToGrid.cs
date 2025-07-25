using UnityEngine;
using UnityEngine.Tilemaps;

public class DragToGrid : MonoBehaviour
{
    private Tilemap tilemap;
    private Camera mainCamera;
    private bool isDragging = false;

    void Start()
    {
        CameraController.IsDraggingBuilding = false;
        mainCamera = Camera.main;

        // Cherche automatiquement la Tilemap nommée "Floor"
        if (tilemap == null)
        {
            GameObject floor = GameObject.Find("Floor");
            if (floor != null)
            {
                tilemap = floor.GetComponent<Tilemap>();
            }
            else 
            {
                Debug.LogError("Tilemap 'Floor' non trouvée ! Vérifie le nom exact dans la hiérarchie.");
            }
        }
    }

    void Update()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        if (Input.GetMouseButtonDown(0))
        {
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
            if (hit != null && hit.gameObject == gameObject)
            {
                isDragging = true;
                CameraController.IsDraggingBuilding = true;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            // Cellule centrale sous la souris
            Vector3Int centerCell = tilemap.WorldToCell(mouseWorldPos);

            // Taille du bâtiment
            BuildingInstance instance = GetComponent<BuildingInstance>();
            Vector2Int size = instance != null && instance.data != null ? instance.data.size : Vector2Int.one;

            // Convertit la cellule centrale en cellule d’origine (bas gauche)
            Vector3Int originCell = new Vector3Int(
                centerCell.x - (size.x - 1) / 2,
                centerCell.y - (size.y - 1) / 2,
                0
            );

            // Aligner la position du bâtiment sur le centre de la cellule d’origine
            Vector3 alignedPos = tilemap.GetCellCenterWorld(originCell);
            alignedPos.z = 0f;
            transform.position = alignedPos;
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            CameraController.IsDraggingBuilding = false;
        }
    }
}