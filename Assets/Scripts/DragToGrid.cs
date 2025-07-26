using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

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
            // Récupérer tous les colliders sous la souris
            Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos);

            if (hits.Length > 0)
            {
                // Sélectionner celui qui est visuellement au-dessus
                Collider2D topCollider = hits
                    .OrderByDescending(h => h.GetComponent<SpriteRenderer>()?.sortingOrder ?? 0)
                    .First();

                if (topCollider.gameObject == gameObject)
                {
                    isDragging = true;
                    CameraController.IsDraggingBuilding = true;
                    Debug.Log($"[DragToGrid] Drag activé sur {gameObject.name}");
                }
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            DragBuilding(mouseWorldPos);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            CameraController.IsDraggingBuilding = false;
            Debug.Log("[DragToGrid] Drag terminé");
        }
    }

    private void DragBuilding(Vector3 mouseWorldPos)
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
}