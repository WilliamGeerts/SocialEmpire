using UnityEngine;
using System.Linq;

/// Activer le DragToGrid et le mode fantôme sur un bâtiment après un appui long (1s).
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

        // Désactiver DragToGrid par défaut
        if (dragToGrid != null)
        {
            dragToGrid.enabled = false;
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
                pressStarted = false;
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
        }

        if (buildingPlacer != null)
        {
            buildingPlacer.SetGhostVisual(gameObject, true);
            buildingPlacer.EnterPlacementModeFor(gameObject);
        }
        else
        {
            Debug.LogWarning("[LongPressToEnableDrag] Aucun BuildingPlacer trouvé dans la scène !");
        }
    }
}