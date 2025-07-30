using System;
using UnityEngine;
using UnityEngine.UI;

public class PlacementController : MonoBehaviour
{
    public Button validateButton;
    public Button cancelButton;

    private Transform target;
    private BuildingInstance instance;

    public void Init(Transform target, BuildingInstance instance, Action onValidate, Action onCancel)
    {
        this.target = target;
        this.instance = instance;

        Debug.Log($"[PlacementController] Init called. target = {target}, instance = {instance}");
        Debug.Log($"[PlacementController] Init with target: {target.name}, size: {instance.size}");

        SetupButtons(onValidate, onCancel);
        UpdatePosition();
    }

    void Update()
    {
        if (target != null && instance != null)
        {
            UpdatePosition();
        }
    }

    private void SetupButtons(Action onValidate, Action onCancel)
    {
        validateButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        validateButton.onClick.AddListener(() =>
        {
            Debug.Log("[PlacementController] Validate button clicked");
            onValidate?.Invoke();
        });

        cancelButton.onClick.AddListener(() =>
        {
            Debug.Log("[PlacementController] Cancel button clicked");
            onCancel?.Invoke();
        });
    }

    private void UpdatePosition()
    {
        if (target == null) return;

        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            Vector3 bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            Vector3 offset = new Vector3(0, 0.5f, 0);
            transform.position = bottomCenter + offset;
        }
        else
        {
            // Fallback : Placement simple basé sur size
            Vector2Int size = instance.size;
            Vector3 fallbackOffset = new Vector3(0, 0, 0.01f);
            transform.position = target.position + fallbackOffset;
        }
    }
}