using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    private Vector3 touchStart;
    public float zoomOutMin = 3f;
    public float zoomInMax = 10f;

    public float minX = -10f, maxX = 10f;
    public float minY = -10f, maxY = 10f;

    private Camera cam;

    public static bool IsDraggingBuilding = false;

    // Empêcher les actions si on est sur l'UI
    private bool startedOnUI = false;

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[CameraController] Aucune caméra trouvée.");
            enabled = false;
        }
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
        ClampCameraPosition();
    }

    void HandlePan()
    {
        // Ne pas bouger la caméra si on déplace un bâtiment
        if (IsDraggingBuilding) 
        {
            return;
        }

        // Gérer le verrouillage souris (PC)
        if (Input.GetMouseButtonDown(0))
        {
            // On vérifie si le curseur est sur l'UI au moment du clic
            startedOnUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (!startedOnUI) 
            {
                touchStart = cam.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (!startedOnUI)
            {
                Vector3 direction = touchStart - cam.ScreenToWorldPoint(Input.mousePosition);
                cam.transform.position += direction;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            startedOnUI = false;
        }

        // Mobile
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startedOnUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);
                if (!startedOnUI)
                    touchStart = cam.ScreenToWorldPoint(touch.position);
            }

            if (touch.phase == TouchPhase.Moved && !startedOnUI)
            {
                Vector3 direction = touchStart - cam.ScreenToWorldPoint(touch.position);
                cam.transform.position += direction;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                startedOnUI = false;
            }
        }
    }

    void HandleZoom()
    {
        // Zoom souris
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Zoom tactile
        if (Input.touchCount == 2 &&
            (EventSystem.current == null ||
             (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) &&
              !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(1).fingerId))))
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 t0Prev = t0.position - t0.deltaPosition;
            Vector2 t1Prev = t1.position - t1.deltaPosition;

            float prevMag = (t0Prev - t1Prev).magnitude;
            float currMag = (t0.position - t1.position).magnitude;

            float diff = currMag - prevMag;
            cam.orthographicSize -= diff * 0.01f;
        }

        // Zoom molette
        if (Input.touchCount == 0)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
                cam.orthographicSize -= scroll * 5f;
        }

        // Clamp du zoom
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, zoomOutMin, zoomInMax);
    }

    void ClampCameraPosition()
    {
        Vector3 pos = cam.transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        cam.transform.position = pos;
    }
}