using UnityEngine;
using System.Linq;

/// <summary>
/// Active le DragToGrid et le mode fantôme sur un bâtiment après un appui long (1s).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BuildingLongPress : MonoBehaviour
{
    public float holdDuration = 1f;  // Durée avant activation
    private float pressTime = 0f;
    private bool pressStarted = false;
    private Camera mainCamera;
    private DragToGrid dragToGrid;
    private BuildingPlacer buildingPlacer;

    void Start()
    {
        mainCamera = Camera.main;
        dragToGrid = GetComponent<DragToGrid>();
        buildingPlacer = FindObjectOfType<BuildingPlacer>();

        if (dragToGrid != null)
        {
            dragToGrid.enabled = false;  // Désactiver DragToGrid par défaut
        }
        else
        {
            Debug.LogWarning("[BuildingLongPress] Aucun DragToGrid trouvé sur ce GameObject !");
        }
    }

    void Update()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // Début du clic
        if (Input.GetMouseButtonDown(0))
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos);

            if (hits.Length > 0)
            {
                Collider2D topCollider = hits
                    .OrderByDescending(h => h.GetComponent<SpriteRenderer>()?.sortingOrder ?? 0)
                    .First();

                if (topCollider.gameObject == gameObject)
                {
                    pressStarted = true;
                    pressTime = 0f;
                    Debug.Log("[BuildingLongPress] Bâtiment sélectionné avec priorité de tri");
                }
            }
        }

        // Maintien
        if (Input.GetMouseButton(0) && pressStarted)
        {
            pressTime += Time.deltaTime;
            if (pressTime >= holdDuration)
            {
                ActivateDragAndGhost();
                pressStarted = false;  // Évite le déclenchement multiple
            }
        }

        // Relâchement
        if (Input.GetMouseButtonUp(0))
        {
            pressStarted = false;
        }
    }

    private void ActivateDragAndGhost()
    {
        if (dragToGrid != null)
        {
            dragToGrid.enabled = true;
            Debug.Log("[LongPressToEnableDrag] DragToGrid activé après appui long !");
        }

        if (buildingPlacer != null)
        {
            buildingPlacer.SetGhostVisual(gameObject, true);
            buildingPlacer.EnterPlacementModeFor(gameObject);
            Debug.Log("[LongPressToEnableDrag] Mode fantôme + indicateurs activés !");
        }
        else
        {
            Debug.LogWarning("[LongPressToEnableDrag] Aucun BuildingPlacer trouvé dans la scène !");
        }
    }
}