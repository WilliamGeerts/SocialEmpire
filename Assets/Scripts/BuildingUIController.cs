using UnityEngine;
using UnityEngine.UI;

public class BuildingUIController : MonoBehaviour
{
    public static ResourceIconLibrary iconLibrary;

    #region "Placement"
    public static void AttachPlacementUI(GameObject building, GameObject placementUIPrefab)
    {
        if (building.transform.Find("PlacementUI") != null) return;

        if (placementUIPrefab == null)
        {
            Debug.LogError("[BuildingUIController] Aucun prefab de PlacementUI assigné !");
            return;
        }

        GameObject ui = Instantiate(placementUIPrefab, building.transform);
        ui.name = "PlacementUI";
        ui.transform.localPosition = new Vector3(0, -1, 0);
        ui.SetActive(false);
    }

    public static void TogglePlacementUI(GameObject building, bool visible, System.Action onValidate, System.Action onCancel)
    {
        Transform panel = building.transform.Find("PlacementUI");
        if (panel == null) return;

        panel.gameObject.SetActive(visible);

        if (!visible) return;

        var validateBtn = panel.Find("WorldSpace/PlacementButtons/ValidateButton")?.GetComponent<Button>();
        var cancelBtn = panel.Find("WorldSpace/PlacementButtons/CancelButton")?.GetComponent<Button>();

        if (validateBtn != null)
        {
            validateBtn.onClick.RemoveAllListeners();
            validateBtn.onClick.AddListener(() => onValidate?.Invoke());
        }

        if (cancelBtn != null)
        {
            cancelBtn.onClick.RemoveAllListeners();
            cancelBtn.onClick.AddListener(() => onCancel?.Invoke());
        }
    }
    #endregion

    #region "Production"
    public static void AttachCollectUI(GameObject building, GameObject collectUIPrefab)
    {
        if (building.transform.Find("CollectUI") != null) return;

        GameObject ui = Instantiate(collectUIPrefab, building.transform);
        ui.name = "CollectUI";

        // Position au tiers supérieur
        BuildingInstance instance = building.GetComponent<BuildingInstance>();
        float offsetY = (instance != null ? instance.size.y : 1f) * (2f / 3f);
        ui.transform.localPosition = new Vector3(0f, offsetY, 0f);

        // Image d’icône de ressource
        Transform imageTransform = ui.transform.Find("CollectButton/CollectImage");
        if (imageTransform != null && instance != null && iconLibrary != null)
        {
            var image = imageTransform.GetComponent<Image>();
            var icon = iconLibrary.GetIcon(instance.production.resourceType);
            if (icon != null)
            {
                image.sprite = icon;
            }
        }

        // Assignation du handler
        Transform buttonTransform = ui.transform.Find("CollectButton");
        if (buttonTransform != null && instance != null)
        {
            var handler = buttonTransform.GetComponent<CollectButtonHandler>();
            handler?.Initialize(instance);
        }

        ui.SetActive(false);
    }

    public static void ShowCollectUI(GameObject building, bool show)
    {
        Transform ui = building.transform.Find("CollectUI");
        if (ui != null)
        {
            ui.gameObject.SetActive(show);
        }
    }
    #endregion
}